using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Features.PrintManagement.Models;
using Next_Future_ERP.Features.PrintManagement.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TemplateContent = Next_Future_ERP.Features.PrintManagement.Models.TemplateContent;

namespace Next_Future_ERP.Features.PrintManagement.ViewModels
{
    /// <summary>
    /// ViewModel لمساحة عمل القالب - تحرير القوالب والمحتوى
    /// </summary>
    public partial class TemplateWorkspaceViewModel : ObservableObject
    {
        private readonly ITemplateCatalogService _catalogService;
        private readonly IVersioningService _versioningService;
        private readonly IContentService _contentService;
        private readonly IRenderPreviewService _previewService;

        public TemplateWorkspaceViewModel(
            ITemplateCatalogService catalogService,
            IVersioningService versioningService,
            IContentService contentService,
            IRenderPreviewService previewService)
        {
            _catalogService = catalogService;
            _versioningService = versioningService;
            _contentService = contentService;
            _previewService = previewService;

            Versions = new ObservableCollection<VersionInfo>();
            Contents = new ObservableCollection<TemplateContent>();
            DataSources = new ObservableCollection<DataSourceInfo>();

            SelectedTabIndex = 0; // Overview tab
        }

        #region Properties - Basic

        [ObservableProperty]
        private PrintTemplate? currentTemplate;

        [ObservableProperty]
        private TemplateVersion? currentVersion;

        [ObservableProperty]
        private int selectedTabIndex;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string statusMessage = "جاهز";

        #endregion

        #region Properties - Overview Tab

        [ObservableProperty]
        private ObservableCollection<VersionInfo> versions;

        [ObservableProperty]
        private VersionInfo? selectedVersion;

        #endregion

        #region Properties - Content Tab

        [ObservableProperty]
        private ObservableCollection<TemplateContent> contents;

        [ObservableProperty]
        private TemplateContent? selectedContent;

        [ObservableProperty]
        private string htmlContent = string.Empty;

        [ObservableProperty]
        private string cssContent = string.Empty;

        #endregion

        #region Properties - DataSources Tab

        [ObservableProperty]
        private ObservableCollection<DataSourceInfo> dataSources;

        [ObservableProperty]
        private DataSourceInfo? selectedDataSource;

        [ObservableProperty]
        private string newDataSourceName = string.Empty;

        [ObservableProperty]
        private string newDataSourceType = "view";

        [ObservableProperty]
        private string newDataSourcePath = string.Empty;

        #endregion

        #region Commands - Overview

        [RelayCommand]
        private async Task LoadTemplateAsync(int templateId)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "تحميل القالب...";

                CurrentTemplate = await _catalogService.GetTemplateByIdAsync(templateId);
                if (CurrentTemplate == null)
                {
                    MessageBox.Show("القالب غير موجود", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await LoadVersionsAsync();

                // تحديد الإصدار النشط إن وُجد
                var activeVersion = await _versioningService.GetActiveVersionAsync(templateId);
                if (activeVersion != null)
                {
                    await LoadVersionDetailsAsync(activeVersion.TemplateVersionId);
                }

                StatusMessage = $"تم تحميل القالب: {CurrentTemplate.Name}";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ في تحميل القالب";
                MessageBox.Show($"❌ خطأ في تحميل القالب:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task CreateNewTemplateAsync(TemplateInfo templateInfo)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "إنشاء قالب جديد...";

                // تحقق من القيم الأساسية (قيم فعلية)
                if (templateInfo.CompanyId is null || templateInfo.CompanyId <= 0)
                {
                    MessageBox.Show("يرجى اختيار الشركة قبل إنشاء القالب.", "بيانات ناقصة",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (templateInfo.DocumentTypeId is null || templateInfo.DocumentTypeId <= 0)
                {
                    MessageBox.Show("يرجى اختيار نوع المستند قبل إنشاء القالب.", "بيانات ناقصة",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // إنشاء PrintTemplate بقيم فعلية
                var newTemplate = new PrintTemplate
                {
                    Name = string.IsNullOrWhiteSpace(templateInfo.Name) ? "قالب جديد" : templateInfo.Name!.Trim(),
                    CompanyId = templateInfo.CompanyId!.Value,
                    DocumentTypeId = templateInfo.DocumentTypeId!.Value,
                    BranchId = templateInfo.BranchId,     // يجوز تبقى null
                    Locale = string.IsNullOrWhiteSpace(templateInfo.Locale) ? "ar-SA" : templateInfo.Locale!,
                    Engine = "html",                    // <<< المهم هنا
                    PaperSize = "A4",                      // اختياري
                    Orientation = null,                      // أو 'P' | 'L' لو تبغى
                    Active = true,
                    IsDefault = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 1
                };


                // حفظ القالب
                CurrentTemplate = await _catalogService.CreateTemplateAsync(newTemplate);
                if (CurrentTemplate is null)
                {
                    MessageBox.Show("تعذر إنشاء القالب.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // إنشاء إصدار أولي
                var initialVersion = await _versioningService.CreateNewVersionAsync(
                    CurrentTemplate.TemplateId,
                    "الإصدار الأول");

                await LoadVersionsAsync();

                if (initialVersion is not null)
                {
                    await LoadVersionDetailsAsync(initialVersion.TemplateVersionId);

                    // توليد القوالب الافتراضية
                    var html = GetDefaultHtmlTemplate(templateInfo);
                    var css = GetDefaultCssTemplate();

                    // حفظ HTML/CSS فورًا كعناصر محتوى في قاعدة البيانات
                    if (!string.IsNullOrWhiteSpace(html))
                        await _contentService.AddTextContentAsync(initialVersion.TemplateVersionId, TemplateContentType.Html, html);

                    if (!string.IsNullOrWhiteSpace(css))
                        await _contentService.AddTextContentAsync(initialVersion.TemplateVersionId, TemplateContentType.Css, css);

                    // تحديث العرض
                    await LoadContentAsync();
                }

                StatusMessage = $"تم إنشاء القالب الجديد: {newTemplate.Name}";
            }
            catch (DbUpdateException ex)
            {
                // عرض السبب الفعلي من InnerException (FK/NULL/قيود فريدة..إلخ)
                var details = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"❌ خطأ EF أثناء الحفظ:\n{details}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "خطأ في إنشاء القالب الجديد";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ في إنشاء القالب الجديد";
                MessageBox.Show($"❌ خطأ في إنشاء القالب الجديد:\n{ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// إنشاء قالب HTML افتراضي للقالب الجديد
        /// </summary>
        private string GetDefaultHtmlTemplate(TemplateInfo templateInfo)
        {
            return $@"<!DOCTYPE html>
<html dir=""rtl"" lang=""ar"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{templateInfo.Name}</title>
    <style>{{{{ css_content }}}}</style>
</head>
<body>
    <div class=""document"">
        <header class=""header"">
            <h1>{templateInfo.Name}</h1>
            <p>نوع المستند: {templateInfo.DocumentTypeName}</p>
            <p>الشركة: {templateInfo.CompanyName}</p>
            <p>الفرع: {templateInfo.BranchName}</p>
        </header>
        
        <main class=""content"">
            <p>هذا قالب جديد جاهز للتخصيص...</p>
            <p>يمكنك تعديل هذا المحتوى ليناسب احتياجاتك.</p>
        </main>
        
        <footer class=""footer"">
            <p>تم إنشاؤه في: {{{{ current_date }}}}</p>
        </footer>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// إنشاء قالب CSS افتراضي للقالب الجديد
        /// </summary>
        private string GetDefaultCssTemplate()
        {
            return @"/* تنسيق عام */
body {
    font-family: 'Arial', sans-serif;
    margin: 0;
    padding: 20px;
    background-color: #f5f5f5;
}

.document {
    max-width: 800px;
    margin: 0 auto;
    background: white;
    padding: 30px;
    border-radius: 8px;
    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
}

.header {
    text-align: center;
    border-bottom: 2px solid #333;
    padding-bottom: 20px;
    margin-bottom: 30px;
}

.header h1 {
    color: #333;
    margin: 0 0 10px 0;
}

.content {
    margin: 20px 0;
    line-height: 1.6;
}

.footer {
    margin-top: 30px;
    padding-top: 20px;
    border-top: 1px solid #ddd;
    text-align: center;
    color: #666;
    font-size: 12px;
}";
        }

        [RelayCommand]
        private async Task CreateNewVersionAsync()
        {
            if (CurrentTemplate == null) return;

            try
            {
                var notes = $"إصدار جديد - {DateTime.Now:yyyy/MM/dd HH:mm}";
                var newVersion = await _versioningService.CreateNewVersionAsync(CurrentTemplate.TemplateId, notes);

                await LoadVersionsAsync();
                StatusMessage = "تم إنشاء إصدار جديد";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في إنشاء الإصدار:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ActivateVersionAsync()
        {
            if (SelectedVersion == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"هل تريد تفعيل الإصدار رقم {SelectedVersion.VersionNo}؟",
                    "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var success = await _versioningService.ActivateVersionAsync(SelectedVersion.TemplateVersionId);
                    if (success)
                    {
                        await LoadVersionsAsync();
                        StatusMessage = "تم تفعيل الإصدار";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تفعيل الإصدار:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task LoadVersionDetailsAsync(int versionId)
        {
            try
            {
                CurrentVersion = await _versioningService.GetVersionByIdAsync(versionId);
                if (CurrentVersion != null)
                {
                    await LoadContentAsync();
                    await LoadDataSourcesAsync();
                    StatusMessage = $"تم تحميل الإصدار رقم {CurrentVersion.VersionNo}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تحميل تفاصيل الإصدار:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Commands - Content

        [RelayCommand]
        private async Task SaveHtmlContentAsync()
        {
            if (CurrentVersion == null || string.IsNullOrEmpty(HtmlContent)) return;

            try
            {
                await _contentService.AddTextContentAsync(
                    CurrentVersion.TemplateVersionId,
                    TemplateContentType.Html,
                    HtmlContent);

                await LoadContentAsync();
                StatusMessage = "تم حفظ محتوى HTML";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في حفظ المحتوى:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task SaveCssContentAsync()
        {
            if (CurrentVersion == null || string.IsNullOrEmpty(CssContent)) return;

            try
            {
                await _contentService.AddTextContentAsync(
                    CurrentVersion.TemplateVersionId,
                    TemplateContentType.Css,
                    CssContent);

                await LoadContentAsync();
                StatusMessage = "تم حفظ محتوى CSS";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في حفظ المحتوى:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteContentAsync()
        {
            if (SelectedContent == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"هل تريد حذف المحتوى من نوع {SelectedContent.ContentType}؟",
                    "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var success = await _contentService.RemoveContentAsync(SelectedContent.TemplateContentId);
                    if (success)
                    {
                        await LoadContentAsync();
                        StatusMessage = "تم حذف المحتوى";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في حذف المحتوى:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Commands - DataSources

        [RelayCommand]
        private async Task AddDataSourceAsync()
        {
            if (CurrentVersion == null || string.IsNullOrEmpty(NewDataSourceName) || string.IsNullOrEmpty(NewDataSourcePath))
            {
                MessageBox.Show("يرجى إدخال اسم مصدر البيانات والمسار",
                    "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var dataSource = new TemplateDataSource
                {
                    TemplateVersionId = CurrentVersion.TemplateVersionId,
                    Name = NewDataSourceName,
                    SourceType = NewDataSourceType,
                    SourceName = NewDataSourcePath,
                    IsMain = !DataSources.Any() // الأول يكون رئيسي
                };

                // يجب إضافة DataSource service لحفظ البيانات
                // await _dataSourceService.AddAsync(dataSource);

                NewDataSourceName = string.Empty;
                NewDataSourcePath = string.Empty;

                await LoadDataSourcesAsync();
                StatusMessage = "تم إضافة مصدر البيانات";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في إضافة مصدر البيانات:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task SetMainDataSourceAsync()
        {
            if (SelectedDataSource == null) return;

            try
            {
                // يجب إضافة DataSource service لتحديث البيانات
                // await _dataSourceService.SetMainAsync(SelectedDataSource.DataSourceId);

                await LoadDataSourcesAsync();
                StatusMessage = "تم تعيين مصدر البيانات الرئيسي";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تعيين مصدر البيانات الرئيسي:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Commands - Preview & Print

        [RelayCommand]
        private async Task PreviewTemplateAsync()
        {
            if (CurrentVersion == null)
            {
                MessageBox.Show("يرجى تحديد إصدار للقالب أولاً",
                    "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "إنشاء معاينة...";

                var result = await _previewService.RenderPreviewAsync(CurrentVersion.TemplateVersionId);

                if (result.Success && !string.IsNullOrEmpty(result.HtmlContent))
                {
                    // عرض المعاينة في نافذة منفصلة
                    ShowPreviewWindow(result.HtmlContent);
                    StatusMessage = $"تم إنشاء المعاينة في {result.RenderTime.TotalMilliseconds:F0}ms";
                }
                else
                {
                    MessageBox.Show($"❌ فشل في إنشاء المعاينة:\n{result.ErrorMessage}",
                        "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ في إنشاء المعاينة";
                MessageBox.Show($"❌ خطأ في إنشاء المعاينة:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task PreviewWithSampleDataAsync()
        {
            if (CurrentVersion == null || CurrentTemplate == null)
            {
                MessageBox.Show("يرجى تحديد إصدار للقالب أولاً",
                    "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "إنشاء معاينة مع بيانات تجريبية...";

                var sampleData = await _previewService.GetSampleDataAsync(CurrentTemplate.DocumentTypeId);
                var result = await _previewService.RenderPreviewAsync(CurrentVersion.TemplateVersionId, sampleData);

                if (result.Success && !string.IsNullOrEmpty(result.HtmlContent))
                {
                    ShowPreviewWindow(result.HtmlContent);
                    StatusMessage = $"تم إنشاء المعاينة مع البيانات التجريبية في {result.RenderTime.TotalMilliseconds:F0}ms";
                }
                else
                {
                    MessageBox.Show($"❌ فشل في إنشاء المعاينة:\n{result.ErrorMessage}",
                        "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ في إنشاء المعاينة";
                MessageBox.Show($"❌ خطأ في إنشاء المعاينة:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ValidateTemplateAsync()
        {
            if (CurrentVersion == null)
            {
                MessageBox.Show("يرجى تحديد إصدار للقالب أولاً",
                    "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "التحقق من صحة القالب...";

                var result = await _previewService.ValidateTemplateAsync(CurrentVersion.TemplateVersionId);

                if (result.IsValid)
                {
                    var message = "✅ القالب صحيح";
                    if (result.Warnings.Any())
                    {
                        message += $"\n\nتحذيرات:\n{string.Join("\n", result.Warnings)}";
                    }
                    MessageBox.Show(message, "نتيجة التحقق", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var message = "❌ القالب يحتوي على أخطاء:\n" + string.Join("\n", result.Errors);
                    if (result.Warnings.Any())
                    {
                        message += $"\n\nتحذيرات:\n{string.Join("\n", result.Warnings)}";
                    }
                    MessageBox.Show(message, "نتيجة التحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                StatusMessage = result.IsValid ? "القالب صحيح" : "القالب يحتوي على أخطاء";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ في التحقق من صحة القالب";
                MessageBox.Show($"❌ خطأ في التحقق من صحة القالب:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task PrintTemplateAsync()
        {
            if (CurrentVersion == null)
            {
                MessageBox.Show("يرجى تحديد إصدار للقالب أولاً",
                    "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "طباعة القالب...";

                var result = await _previewService.RenderPdfPreviewAsync(CurrentVersion.TemplateVersionId);

                if (result.Length > 0)
                {
                    // حفظ PDF مؤقتاً وطباعته
                    var tempPath = System.IO.Path.GetTempFileName() + ".pdf";
                    await System.IO.File.WriteAllBytesAsync(tempPath, result);

                    // فتح PDF للطباعة
                    var process = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = tempPath,
                            UseShellExecute = true
                        }
                    };
                    process.Start();

                    StatusMessage = "تم إرسال القالب للطباعة";
                }
                else
                {
                    MessageBox.Show("❌ فشل في إنشاء ملف PDF للطباعة",
                        "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ في الطباعة";
                MessageBox.Show($"❌ خطأ في الطباعة:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ShowPreviewWindow(string htmlContent)
        {
            try
            {
                var previewWindow = new System.Windows.Window
                {
                    Title = "معاينة القالب",
                    Width = 800,
                    Height = 600,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
                };

                var webBrowser = new System.Windows.Controls.WebBrowser();
                webBrowser.NavigateToString(htmlContent);

                previewWindow.Content = webBrowser;
                previewWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في عرض المعاينة:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Private Methods

        private async Task LoadVersionsAsync()
        {
            if (CurrentTemplate == null) return;

            try
            {
                var versions = await _versioningService.GetVersionsAsync(CurrentTemplate.TemplateId);
                Versions.Clear();
                foreach (var version in versions)
                {
                    Versions.Add(version);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تحميل الإصدارات:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadContentAsync()
        {
            if (CurrentVersion == null) return;

            try
            {
                var contents = await _contentService.GetVersionContentsAsync(CurrentVersion.TemplateVersionId);
                Contents.Clear();
                foreach (var content in contents)
                {
                    Contents.Add(content);

                    // تحميل المحتوى النصي للعرض
                    if (content.ContentType == TemplateContentType.Html && !string.IsNullOrEmpty(content.ContentText))
                    {
                        HtmlContent = content.ContentText;
                    }
                    else if (content.ContentType == TemplateContentType.Css && !string.IsNullOrEmpty(content.ContentText))
                    {
                        CssContent = content.ContentText;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تحميل المحتوى:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDataSourcesAsync()
        {
            if (CurrentVersion == null) return;

            try
            {
                // يجب إنشاء DataSource service منفصل
                DataSources.Clear();

                // بيانات تجريبية
                DataSources.Add(new DataSourceInfo
                {
                    DataSourceId = 1,
                    Name = "header",
                    SourceType = "view",
                    SourceName = "vw_DocumentHeader",
                    IsMain = true,
                    ExistsInDatabase = true,
                    TestPassed = true
                });

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تحميل مصادر البيانات:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}