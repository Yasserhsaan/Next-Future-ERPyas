using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.InitialSystem.Models;
using Next_Future_ERP.Features.PrintManagement.Models;
using Next_Future_ERP.Features.PrintManagement.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.PrintManagement.ViewModels
{
    /// <summary>
    /// ViewModel لشاشة مكتبة القوالب
    /// </summary>
    public partial class TemplateLibraryViewModel : ObservableObject
    {
        private readonly ITemplateCatalogService _catalogService;
        private readonly IVersioningService _versioningService;
        private readonly PrintManagementInitializationService _initService;

        public TemplateLibraryViewModel(
            ITemplateCatalogService catalogService,
            IVersioningService versioningService)
        {
            _catalogService = catalogService;
            _versioningService = versioningService;

            // ← استخدم DI بدل new AppDbContext()
            _initService = App.ServiceProvider.GetRequiredService<PrintManagementInitializationService>();

            Templates = new ObservableCollection<TemplateInfo>();
            CompanyOptions = new ObservableCollection<CompanyInfoModel>();
            BranchOptions = new ObservableCollection<BranchModel>();
            DocumentTypeOptions = new ObservableCollection<KeyValuePair<int, string>>();

            InitializeFilters(); // async void مقصودة للتهيئة الأولية
        }

        #region Properties - Filters

        // لا تضع 1 افتراضيًا، نخليه null ثم نختار أول شركة حقيقية من القاعدة
        private int? _selectedCompanyId = null;
        public int? SelectedCompanyId
        {
            get => _selectedCompanyId;
            set
            {
                if (SetProperty(ref _selectedCompanyId, value))
                {
                    _branchLoadTask = LoadBranchOptionsAsync(value);
                }
            }
        }

        [ObservableProperty] private int? selectedBranchId;
        [ObservableProperty] private int? selectedDocumentTypeId;
        [ObservableProperty] private string? selectedLocale;
        [ObservableProperty] private string? selectedEngine;
        [ObservableProperty] private bool? filterActive;
        [ObservableProperty] private bool? filterDefault;

        #endregion

        #region Properties - Data

        [ObservableProperty] private ObservableCollection<TemplateInfo> templates;
        [ObservableProperty] private TemplateInfo? selectedTemplate;
        [ObservableProperty] private ObservableCollection<CompanyInfoModel> companyOptions;
        [ObservableProperty] private ObservableCollection<BranchModel> branchOptions;
        [ObservableProperty] private ObservableCollection<KeyValuePair<int, string>> documentTypeOptions;

        #endregion

        #region Properties - UI State

        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private string statusMessage = "جاهز";
        [ObservableProperty] private Dictionary<string, int> templateStats = new();

        #endregion

        #region Commands

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "تحميل البيانات...";
                await LoadTemplatesAsync();
                await LoadStatsAsync();
                StatusMessage = $"تم تحميل {Templates.Count} قالب";
                if (Templates.Count == 0) StatusMessage = "لا توجد قوالب متاحة حالياً";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ في تحميل البيانات";
                MessageBox.Show($"❌ خطأ في تحميل البيانات:\n{ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task CompanyChangedAsync()
        {
            SelectedBranchId = null;
            await LoadBranchOptionsAsync(SelectedCompanyId);
            await LoadTemplatesAsync();
        }

        [RelayCommand] private async Task BranchChangedAsync() => await LoadTemplatesAsync();
        [RelayCommand] private async Task SearchAsync() => await LoadTemplatesAsync();

        [RelayCommand]
        private async Task ClearFiltersAsync()
        {
            SelectedCompanyId = null;
            SelectedBranchId = null;
            SelectedDocumentTypeId = null;
            SelectedLocale = null;
            SelectedEngine = null;
            FilterActive = null;
            FilterDefault = null;

            await InitializeSelectionsAsync(); // يعيد اختيار أول قيم فعلية
            await LoadTemplatesAsync();
        }

        /// <summary>
        /// إنشاء قالب جديد باستخدام قيم فعلية من القاعدة حتى لو المستخدم لم يختر.
        /// </summary>
        [RelayCommand]
        private async Task CreateNewTemplateAsync()
        {
            try
            {
                StatusMessage = "إنشاء قالب جديد...";

                // نضمن وجود اختيارات فعلية من القاعدة
                await InitializeSelectionsAsync();

                var docTypeName = DocumentTypeOptions
                    .FirstOrDefault(d => d.Key == SelectedDocumentTypeId).Value ?? "غير محدد";
                var compName = CompanyOptions
                    .FirstOrDefault(c => c.CompId == SelectedCompanyId)?.CompName ?? "غير محدد";
                var branchName = BranchOptions
                    .FirstOrDefault(b => b.BranchId == SelectedBranchId)?.BranchName ?? "الفرع الرئيسي";

                var newTemplate = new TemplateInfo
                {
                    TemplateId = 0,
                    Name = "قالب جديد",
                    // 👇 هذه المعرفات مهمة للحفظ في PrintTemplates
                    CompanyId = SelectedCompanyId,
                    BranchId = SelectedBranchId,          // قد تكون null وهذا مقبول
                    DocumentTypeId = SelectedDocumentTypeId,

                    // أسماء للعرض فقط
                    DocumentTypeName = docTypeName,
                    CompanyName = compName,
                    BranchName = branchName,

                    Locale = string.IsNullOrWhiteSpace(SelectedLocale) ? "ar-SA" : SelectedLocale,
                    Engine = string.IsNullOrWhiteSpace(SelectedEngine) ? "Liquid" : SelectedEngine,
                    Active = true,
                    IsDefault = false,
                    ActiveVersionNo = 1,
                    Status = "جديد"
                };

                await OpenTemplateWorkspaceAsync(newTemplate, isNewTemplate: true);
                StatusMessage = "تم فتح مساحة العمل للقالب الجديد";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ في إنشاء القالب الجديد";
                MessageBox.Show($"❌ خطأ في إنشاء القالب الجديد:\n{ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DuplicateTemplateAsync()
        {
            if (SelectedTemplate == null)
            {
                MessageBox.Show("يرجى اختيار قالب للنسخ", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var newName = $"{SelectedTemplate.Name} - نسخة";
                await _catalogService.DuplicateTemplateAsync(SelectedTemplate.TemplateId, newName);
                await LoadTemplatesAsync();
                StatusMessage = "تم نسخ القالب بنجاح";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في نسخ القالب:\n{ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task SetDefaultAsync()
        {
            if (SelectedTemplate == null) return;

            try
            {
                var actionQ = MessageBox.Show(
                    $"هل تريد تعيين القالب '{SelectedTemplate.Name}' كافتراضي؟",
                    "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (actionQ == MessageBoxResult.Yes)
                {
                    if (await _catalogService.SetDefaultAsync(SelectedTemplate.TemplateId))
                    {
                        await LoadTemplatesAsync();
                        StatusMessage = "تم تعيين القالب كافتراضي";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تعيين القالب كافتراضي:\n{ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ToggleActiveAsync()
        {
            if (SelectedTemplate == null) return;

            try
            {
                var newStatus = !SelectedTemplate.Active;
                var action = newStatus ? "تفعيل" : "إيقاف";

                var ask = MessageBox.Show(
                    $"هل تريد {action} القالب '{SelectedTemplate.Name}'؟",
                    "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (ask == MessageBoxResult.Yes)
                {
                    if (await _catalogService.ToggleActiveAsync(SelectedTemplate.TemplateId, newStatus))
                    {
                        await LoadTemplatesAsync();
                        StatusMessage = $"تم {action} القالب";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تغيير حالة القالب:\n{ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task OpenWorkspaceAsync()
        {
            if (SelectedTemplate == null)
            {
                MessageBox.Show("يرجى اختيار قالب لفتح مساحة العمل", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await OpenTemplateWorkspaceAsync(SelectedTemplate, isNewTemplate: false);
        }

        private async Task OpenTemplateWorkspaceAsync(TemplateInfo template, bool isNewTemplate = false)
        {
            try
            {
                StatusMessage = $"فتح مساحة العمل: {template.Name}...";

                // حلّ مساحة العمل من الـDI (تم تسجيلها في App.xaml.cs)
                var workspaceView = App.ServiceProvider.GetRequiredService<Features.PrintManagement.Views.TemplateWorkspaceView>();

                if (isNewTemplate)
                {
                    // نطلب من مساحة العمل بدء إنشاء قالب جديد استناداً للـTemplateInfo
                    // ملاحظة: داخل TemplateWorkspaceView/VM تأكّد من وجود public method تقبل TemplateInfo
                    // مثلاً: workspaceView.CreateNewTemplate(template);
                    workspaceView.CreateNewTemplate(template);
                }
                else
                {
                    // تحميل قالب موجود
                    workspaceView.LoadTemplate(template.TemplateId);
                }

                // نافذة مستقلة لمساحة العمل
                var window = new System.Windows.Window
                {
                    Title = $"مساحة عمل: {template.Name}",
                    Content = workspaceView,
                    Width = 1400,
                    Height = 900,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                    WindowState = System.Windows.WindowState.Maximized
                };

                window.Show();
                StatusMessage = $"تم فتح مساحة العمل: {template.Name}";
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ في فتح مساحة العمل";
                MessageBox.Show($"❌ خطأ في فتح مساحة العمل:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        [RelayCommand] private async Task RefreshAsync() => await LoadDataAsync();

        #endregion

        #region Commands - System Management

        [RelayCommand]
        private async Task InitializeSystemAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "تهيئة النظام...";
                var success = await _initService.InitializeAsync();
                StatusMessage = success ? "تم تهيئة النظام بنجاح" : "فشل في تهيئة النظام";
                if (success) await LoadDataAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ في تهيئة النظام";
                MessageBox.Show($"❌ خطأ في تهيئة النظام:\n{ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task CheckSystemStatusAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "فحص حالة النظام...";
                var status = await _initService.CheckSystemStatusAsync();

                var message =
                    $"📊 تقرير حالة النظام:\n\n" +
                    $"🔗 الاتصال بقاعدة البيانات: {(status.DatabaseConnected ? "✅ متصل" : "❌ غير متصل")}\n" +
                    $"🗃️ جداول البيانات: {(status.TablesExist ? "✅ موجودة" : "❌ غير موجودة")}\n" +
                    $"📄 البيانات التجريبية: {(status.HasSampleData ? "✅ موجودة" : "❌ غير موجودة")}\n\n" +
                    $"• القوالب: {status.TemplatesCount}\n" +
                    $"• الإصدارات: {status.VersionsCount}\n" +
                    $"• المحتويات: {status.ContentsCount}\n" +
                    $"• الأصول: {status.AssetsCount}\n" +
                    $"• المهام: {status.JobsCount}\n\n" +
                    $"الحالة العامة: {status.StatusMessage}";

                MessageBox.Show(message, "حالة النظام", MessageBoxButton.OK,
                    status.IsReady ? MessageBoxImage.Information : MessageBoxImage.Warning);

                StatusMessage = status.StatusMessage;
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ في فحص النظام";
                MessageBox.Show($"❌ خطأ في فحص النظام:\n{ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task OpenWorkspaceWithCheckAsync()
        {
            try
            {
                var status = await _initService.CheckSystemStatusAsync();
                if (!status.IsReady)
                {
                    var result = MessageBox.Show(
                        $"⚠️ النظام غير جاهز:\n{status.StatusMessage}\n\nهل تريد تهيئة النظام الآن؟",
                        "تحذير", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                        await InitializeSystemAsync();

                    return;
                }

                MessageBox.Show("يمكنك الآن فتح مساحة العمل من جدول القوالب.", "معلومات",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                StatusMessage = "النظام جاهز";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في فتح مساحة العمل:\n{ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Private Methods

        private async void InitializeFilters()
        {
            try
            {
                StatusMessage = "تهيئة المرشحات...";
                await LoadCompanyOptionsAsync();

                if (SelectedCompanyId.HasValue)
                    await LoadBranchOptionsAsync(SelectedCompanyId);

                await LoadDocumentTypeOptionsAsync();
                await InitializeSelectionsAsync(); // يضمن تعيين أول قيم فعلية

                StatusMessage = "تم تحميل المرشحات بنجاح";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ في تهيئة المرشحات";
                MessageBox.Show($"❌ خطأ في تهيئة المرشحات:\n{ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>اختر أول شركة/فرع/نوع مستند فعّال من القاعدة إن لم تكن محددة.</summary>
        private async Task InitializeSelectionsAsync()
        {
            using var db = DbContextFactory.Create();

            if (!SelectedCompanyId.HasValue || SelectedCompanyId <= 0)
                SelectedCompanyId = await db.CompanyInfo
                    .OrderBy(c => c.CompId).Select(c => c.CompId).FirstAsync();

            if (!SelectedDocumentTypeId.HasValue || SelectedDocumentTypeId <= 0)
                SelectedDocumentTypeId = await db.DocumentTypes
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.DocumentTypeId)
                    .Select(d => d.DocumentTypeId)
                    .FirstAsync();

            if (!SelectedBranchId.HasValue)
            {
                SelectedBranchId = await db.Branches
                    .Where(b => b.ComiId == SelectedCompanyId)
                    .OrderBy(b => b.BranchId)
                    .Select(b => (int?)b.BranchId)
                    .FirstOrDefaultAsync(); // قد يرجع null وهذا مقبول
            }
        }

        /// <summary>تحميل الشركات من القاعدة</summary>
        private async Task LoadCompanyOptionsAsync()
        {
            try
            {
                CompanyOptions.Clear();
                StatusMessage = "تحميل الشركات...";

                using var db = DbContextFactory.Create();
                var companies = await db.CompanyInfo
                    .AsNoTracking().OrderBy(c => c.CompName).ToListAsync();

                foreach (var company in companies) CompanyOptions.Add(company);

                if (CompanyOptions.Any() && !SelectedCompanyId.HasValue)
                    SelectedCompanyId = CompanyOptions.First().CompId;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تحميل الشركات:\n{ex.Message}", "تحذير",
                    MessageBoxButton.OK, MessageBoxImage.Warning);

                // احتياطي
                CompanyOptions.Clear();
                CompanyOptions.Add(new CompanyInfoModel { CompId = 1, CompName = "شركة المستقبل التالي" });
            }
        }

        private readonly SemaphoreSlim _branchLock = new(1, 1);
        private Task _branchLoadTask = Task.CompletedTask;
        private int _lastCompanyIdLoaded = -1;

        private async Task LoadBranchOptionsAsync(int? companyId = null)
        {
            await _branchLock.WaitAsync();
            try
            {
                BranchOptions.Clear();

                if (!companyId.HasValue || companyId <= 0)
                {
                    _lastCompanyIdLoaded = 0;
                    return;
                }

                if (_lastCompanyIdLoaded == companyId && BranchOptions.Count > 0)
                    return;

                StatusMessage = $"تحميل فروع الشركة {companyId}...";

                using var db = DbContextFactory.Create();
                var branches = await db.Branches
                    .AsNoTracking()
                    .Where(b => b.ComiId == companyId)
                    .OrderBy(b => b.BranchName)
                    .ToListAsync();

                foreach (var branch in branches) BranchOptions.Add(branch);

                _lastCompanyIdLoaded = companyId.Value;

                if (BranchOptions.Any() && !SelectedBranchId.HasValue)
                    SelectedBranchId = BranchOptions.First().BranchId;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تحميل فروع الشركة {companyId}:\n{ex.Message}", "تحذير",
                    MessageBoxButton.OK, MessageBoxImage.Warning);

                // احتياطي
                BranchOptions.Clear();
                if (companyId.HasValue)
                    BranchOptions.Add(new BranchModel { BranchId = 1, ComiId = companyId.Value, BranchName = "الفرع الرئيسي" });
            }
            finally { _branchLock.Release(); }
        }

        private async Task LoadDocumentTypeOptionsAsync()
        {
            try
            {
                DocumentTypeOptions.Clear();
                StatusMessage = "تحميل أنواع المستندات...";

                // إن كان عندك خدمة موثقة استخدمها من DI، وإلا استعلم مباشرة
                using var db = DbContextFactory.Create();
                var types = await db.DocumentTypes
                    .AsNoTracking()
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.DocumentNameAr ?? d.DocumentNameEn)
                    .Select(d => new { d.DocumentTypeId, Name = d.DocumentNameAr ?? d.DocumentNameEn ?? "غير محدد" })
                    .ToListAsync();

                foreach (var t in types)
                    DocumentTypeOptions.Add(new KeyValuePair<int, string>(t.DocumentTypeId, t.Name));

                if (DocumentTypeOptions.Any() && (!SelectedDocumentTypeId.HasValue || SelectedDocumentTypeId == 0))
                    SelectedDocumentTypeId = DocumentTypeOptions.First().Key;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تحميل أنواع المستندات:\n{ex.Message}", "تحذير",
                    MessageBoxButton.OK, MessageBoxImage.Warning);

                // احتياطي
                DocumentTypeOptions.Clear();
                DocumentTypeOptions.Add(new KeyValuePair<int, string>(1, "سند قبض"));
            }
        }

        private async Task LoadTemplatesAsync()
        {
            try
            {
                var list = await _catalogService.GetTemplatesAsync(
                    companyId: SelectedCompanyId,
                    branchId: SelectedBranchId,
                    documentTypeId: SelectedDocumentTypeId,
                    locale: SelectedLocale,
                    engine: SelectedEngine,
                    isActive: FilterActive,
                    isDefault: FilterDefault);

                Templates.Clear();
                foreach (var t in list) Templates.Add(t);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تحميل القوالب:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadStatsAsync()
        {
            try
            {
                TemplateStats = await _catalogService.GetTemplateStatsAsync(SelectedCompanyId);
            }
            catch
            {
                TemplateStats = new Dictionary<string, int>();
            }
        }

        #endregion
    }
}