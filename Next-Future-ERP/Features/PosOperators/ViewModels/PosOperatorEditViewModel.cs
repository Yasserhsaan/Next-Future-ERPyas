using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.PosOperators.Services;
using Next_Future_ERP.Features.PosOperators.Models;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Models;
using Next_Future_ERP.Features.PosStations.Models;
using Next_Future_ERP.Features.PosStations.Services;
using Next_Future_ERP.Features.Auth.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.PosOperators.ViewModels
{
    /// <summary>
    /// ViewModel لنافذة تعديل مشغل نقطة البيع
    /// </summary>
    public partial class PosOperatorEditViewModel : ObservableObject, IDisposable
    {
        private readonly IPosOperatorService _posOperatorService;
        private readonly IServiceProvider _serviceProvider;
        private readonly AppDbContext _context;
        private readonly ISessionService _sessionService;
        private bool _disposed = false;
        private bool _isEditMode = false;

        [ObservableProperty]
        private PosOperator _posOperator = new();
        
        partial void OnPosOperatorChanged(PosOperator value)
        {
            SaveCommand?.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        private bool _isLoading;
        
        partial void OnIsLoadingChanged(bool value)
        {
            SaveCommand?.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(PosOperator));
        }

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<PosStation> _posStations = new();

        [ObservableProperty]
        private ObservableCollection<Nextuser> _users = new();

        public event EventHandler<bool>? CloseRequested;

        public PosOperatorEditViewModel(IPosOperatorService posOperatorService, IServiceProvider serviceProvider, AppDbContext context, ISessionService sessionService)
        {
            _posOperatorService = posOperatorService;
            _serviceProvider = serviceProvider;
            _context = context;
            _sessionService = sessionService;
            
            SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
            CancelCommand = new RelayCommand(Cancel);
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        }

        public IAsyncRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public IAsyncRelayCommand LoadDataCommand { get; }

        /// <summary>
        /// تحميل البيانات المساعدة
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "جاري تحميل البيانات...";

                var currentCompanyId = _sessionService.CurrentUser?.CompanyId;
                var currentBranchId = _sessionService.CurrentUser?.BranchId;

                System.Diagnostics.Debug.WriteLine($"LoadDataAsync: CompanyId={currentCompanyId}, BranchId={currentBranchId}");
                System.Diagnostics.Debug.WriteLine($"LoadDataAsync: CurrentUser={_sessionService.CurrentUser?.Name}");

                // تحميل نقاط البيع - جرب بدون فلتر أولاً
                var posStationService = _serviceProvider.GetRequiredService<IPosStationService>();
                
                // جرب تحميل جميع نقاط البيع أولاً للتأكد من وجودها
                var allPosStations = await posStationService.GetAllAsync();
                System.Diagnostics.Debug.WriteLine($"LoadDataAsync: Found {allPosStations.Count} total POS stations");
                
                // ثم جرب مع الفلتر
                var posStations = await posStationService.GetAllAsync(companyId: currentCompanyId, branchId: currentBranchId, isActive: true);
                System.Diagnostics.Debug.WriteLine($"LoadDataAsync: Found {posStations.Count} filtered POS stations");
                
                // إذا لم توجد نقاط بيع مع الفلتر، استخدم جميع النقاط
                if (posStations.Count == 0 && allPosStations.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("LoadDataAsync: No filtered stations found, using all stations");
                    posStations = allPosStations;
                }
                
                // إذا لم توجد أي نقاط بيع، أضف بيانات تجريبية
                if (posStations.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("LoadDataAsync: No stations found, adding sample data");
                    posStations = new List<PosStation>
                    {
                        new PosStation { PosId = 1, PosName = "نقطة البيع الرئيسية", IsActive = true, CompanyId = 1 },
                        new PosStation { PosId = 2, PosName = "نقطة البيع الفرعية", IsActive = true, CompanyId = 1 },
                        new PosStation { PosId = 3, PosName = "نقطة البيع المؤقتة", IsActive = true, CompanyId = 1 }
                    };
                }
                
                PosStations.Clear();
                foreach (var station in posStations)
                {
                    PosStations.Add(station);
                    System.Diagnostics.Debug.WriteLine($"LoadDataAsync: Added station {station.PosName} (ID: {station.PosId})");
                }

                // تحميل المستخدمين
                var users = await _context.Nextuser
                    .OrderBy(u => u.Name)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"LoadDataAsync: Found {users.Count} users");

                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }

                OnPropertyChanged(nameof(PosStations));
                OnPropertyChanged(nameof(Users));
                OnPropertyChanged(nameof(PosOperator));
                SaveCommand.NotifyCanExecuteChanged();

                StatusMessage = $"تم تحميل {PosStations.Count} نقطة بيع و {Users.Count} مستخدم بنجاح.";
                
                // تحديث إضافي للتأكد من تحديث الـ ComboBox
                await Task.Delay(100);
                OnPropertyChanged(nameof(PosStations));
                OnPropertyChanged(nameof(Users));
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في تحميل البيانات: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"LoadDataAsync Error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"حدث خطأ في تحميل البيانات:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                SaveCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(PosOperator));
            }
        }

        /// <summary>
        /// تهيئة ViewModel بشكل غير متزامن
        /// </summary>
        public async Task InitializeAsync()
        {
            await LoadDataCommand.ExecuteAsync(null);
        }

        /// <summary>
        /// تهيئة ViewModel مع مشغل نقطة بيع
        /// </summary>
        public async Task Initialize(PosOperator posOperator)
        {
            if (posOperator == null || posOperator.OperatorId == 0)
            {
                // وضع الإضافة - إنشاء مشغل نقطة بيع جديد
                PosOperator = new PosOperator
                {
                    IsActive = true,
                    IsPrimary = false,
                    StartDate = DateTime.Now,
                    CompanyId = _sessionService.CurrentUser?.CompanyId ?? 1,
                    BranchId = _sessionService.CurrentUser?.BranchId
                };
                _isEditMode = false;
            }
            else
            {
                // وضع التعديل - نسخ البيانات من السجل الموجود
                PosOperator = new PosOperator
                {
                    OperatorId = posOperator.OperatorId,
                    PosId = posOperator.PosId,
                    UserId = posOperator.UserId,
                    IsPrimary = posOperator.IsPrimary,
                    StartDate = posOperator.StartDate,
                    EndDate = posOperator.EndDate,
                    IsActive = posOperator.IsActive,
                    CompanyId = posOperator.CompanyId,
                    BranchId = posOperator.BranchId
                };
                _isEditMode = true;
            }
            
            // تشخيص
            System.Diagnostics.Debug.WriteLine($"Initialize: OperatorId={PosOperator.OperatorId}, _isEditMode={_isEditMode}, PosId={PosOperator.PosId}, UserId={PosOperator.UserId}");
            
            // تحديث العنوان
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(PosOperator));
            SaveCommand.NotifyCanExecuteChanged();
            
            // تحميل البيانات المساعدة
            await LoadDataCommand.ExecuteAsync(null);
        }

        /// <summary>
        /// عنوان النافذة
        /// </summary>
        public string WindowTitle => _isEditMode ? "تعديل مشغل نقطة البيع" : "إضافة مشغل نقطة بيع جديد";

        public static async Task<PosOperatorEditViewModel> FromExisting(
            IPosOperatorService posOperatorService,
            IServiceProvider serviceProvider,
            AppDbContext context,
            ISessionService sessionService,
            int operatorId)
        {
            var posOperator = await posOperatorService.GetByIdAsync(operatorId)
                                ?? throw new InvalidOperationException("مشغل نقطة البيع غير موجود.");
            var vm = new PosOperatorEditViewModel(posOperatorService, serviceProvider, context, sessionService);
            await vm.Initialize(posOperator);
            return vm;
        }

        /// <summary>
        /// حفظ مشغل نقطة البيع
        /// </summary>
        private async Task SaveAsync()
        {
            try
            {
                StatusMessage = "بدء عملية الحفظ...";
                IsLoading = true;
                StatusMessage = "جاري التحقق من البيانات...";

                // التحقق من البيانات المطلوبة
                if (PosOperator.PosId <= 0)
                {
                    StatusMessage = "يرجى اختيار نقطة البيع";
                    MessageBox.Show("يرجى اختيار نقطة البيع من القائمة المنسدلة.", "بيانات مطلوبة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (PosOperator.UserId <= 0)
                {
                    StatusMessage = "يرجى اختيار المستخدم";
                    MessageBox.Show("يرجى اختيار المستخدم من القائمة المنسدلة.", "بيانات مطلوبة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // التحقق من صحة البيانات
                var isValid = await _posOperatorService.ValidateAsync(PosOperator);
                if (!isValid)
                {
                    StatusMessage = "البيانات المدخلة غير صحيحة";
                    MessageBox.Show("يرجى التحقق من البيانات المدخلة والتأكد من عدم تكرار المستخدم لنفس نقطة البيع.",
                        "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                StatusMessage = _isEditMode ? "جاري تحديث مشغل نقطة البيع..." : "جاري إضافة مشغل نقطة البيع...";

                // تشخيص
                System.Diagnostics.Debug.WriteLine($"SaveAsync: _isEditMode={_isEditMode}, PosOperator.OperatorId={PosOperator.OperatorId}");

                PosOperator savedOperator;
                if (_isEditMode)
                {
                    savedOperator = await _posOperatorService.UpdateAsync(PosOperator);
                }
                else
                {
                    savedOperator = await _posOperatorService.AddAsync(PosOperator);
                }

                StatusMessage = _isEditMode ? "تم تحديث مشغل نقطة البيع بنجاح" : "تم إضافة مشغل نقطة البيع بنجاح";
                
                MessageBox.Show(_isEditMode ? "تم تحديث مشغل نقطة البيع بنجاح" : "تم إضافة مشغل نقطة البيع بنجاح",
                    "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                // تأخير صغير للتأكد من أن النافذة جاهزة
                await Task.Delay(100);

                // إغلاق النافذة
                CloseRequested?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في الحفظ: {ex.Message}";
                MessageBox.Show($"حدث خطأ في حفظ مشغل نقطة البيع:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                SaveCommand.NotifyCanExecuteChanged();
                
                // تحديث إضافي للتأكد من تحديث الزر
                OnPropertyChanged(nameof(PosOperator));
            }
        }

        /// <summary>
        /// إلغاء العملية
        /// </summary>
        private void Cancel()
        {
            var result = MessageBox.Show("هل أنت متأكد من إلغاء العملية؟ سيتم فقدان جميع التغييرات غير المحفوظة.",
                "تأكيد الإلغاء",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                CloseRequested?.Invoke(this, false);
            }
        }

        /// <summary>
        /// تحديث حالة زر الحفظ
        /// </summary>
        public void RefreshCanSave()
        {
            SaveCommand?.NotifyCanExecuteChanged();
        }
        
        /// <summary>
        /// التحقق من إمكانية الحفظ
        /// </summary>
        private bool CanSave()
        {
            var posIdValid = PosOperator.PosId > 0;
            var userIdValid = PosOperator.UserId > 0;
            var notLoading = !IsLoading;
            
            // في وضع الإضافة، نسمح بالحفظ حتى لو لم يتم اختيار PosId و UserId بعد
            // سيتم التحقق من هذه القيم في SaveAsync
            var canSave = notLoading;
            
            // في وضع التعديل، نطلب PosId و UserId
            if (_isEditMode)
            {
                canSave = canSave && posIdValid && userIdValid;
            }
            
            // تشخيص مفصل
            StatusMessage = $"CanSave: {canSave} | " +
                           $"PosId: {posIdValid} ({PosOperator.PosId}) | " +
                           $"UserId: {userIdValid} ({PosOperator.UserId}) | " +
                           $"NotLoading: {notLoading} ({IsLoading}) | " +
                           $"IsEditMode: {_isEditMode}";
            
            // تحديث إضافي للتأكد من تحديث الزر
            OnPropertyChanged(nameof(PosOperator));
            
            return canSave;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context?.Dispose();
                _disposed = true;
            }
        }

        ~PosOperatorEditViewModel()
        {
            Dispose(false);
        }
    }
}