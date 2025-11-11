using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.PosStations.Services;
using Next_Future_ERP.Features.PosStations.Models;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Models;
using Next_Future_ERP.Features.InitialSystem.Models;
using Next_Future_ERP.Features.Auth.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.PosStations.ViewModels
{
    /// <summary>
    /// ViewModel لنافذة تعديل محطة نقطة البيع
    /// </summary>
    public partial class PosStationEditViewModel : ObservableObject, IDisposable
    {
        private readonly IPosStationService _posStationService;
        private readonly IServiceProvider _serviceProvider;
        private readonly AppDbContext _context;
        private readonly ISessionService _sessionService;
        private bool _disposed = false;
        private bool _isEditMode = false;

        [ObservableProperty]
        private PosStation _station = new();
        
        partial void OnStationChanged(PosStation value)
        {
            SaveCommand?.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        private bool _isLoading;
        
        partial void OnIsLoadingChanged(bool value)
        {
            SaveCommand?.NotifyCanExecuteChanged();
            
            // تحديث إضافي للتأكد من تحديث الزر
            OnPropertyChanged(nameof(Station));
        }

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<BranchModel> _branches = new();

        [ObservableProperty]
        private ObservableCollection<Nextuser> _users = new();

        public PosStationEditViewModel(IPosStationService posStationService, IServiceProvider serviceProvider, AppDbContext context, ISessionService sessionService)
        {
            _posStationService = posStationService;
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

        public event EventHandler<bool>? CloseRequested;

        public static async Task<PosStationEditViewModel> FromExisting(
            IPosStationService posStationService,
            IServiceProvider serviceProvider,
            AppDbContext context,
            ISessionService sessionService,
            int posId)
        {
            var station = await posStationService.GetByIdAsync(posId)
                         ?? throw new InvalidOperationException("نقطة البيع غير موجودة.");
            var vm = new PosStationEditViewModel(posStationService, serviceProvider, context, sessionService);
            await vm.Initialize(station);
            await vm.LoadDataCommand.ExecuteAsync(null);
            return vm;
        }
        
        /// <summary>
        /// تهيئة ViewModel بشكل غير متزامن
        /// </summary>
        public async Task InitializeAsync()
        {
            await LoadDataCommand.ExecuteAsync(null);
        }
        
        /// <summary>
        /// تهيئة ViewModel مع نقطة بيع
        /// </summary>
        public async Task Initialize(PosStation station)
        {
            if (station == null || station.PosId == 0)
            {
                // وضع الإضافة - إنشاء نقطة بيع جديدة
                var nextCode = await _posStationService.GenerateNextCodeAsync();
                Station = new PosStation
                {
                    PosCode = nextCode, // توليد الكود تلقائياً
                    IsActive = true,
                    CompanyId = _sessionService.CurrentUser?.CompanyId ?? 1,
                    CreatedDate = DateTime.Now
                };
                _isEditMode = false;
            }
            else
            {
                // وضع التعديل - نسخ البيانات من السجل الموجود
                Station = new PosStation
                {
                    PosId = station.PosId,
                    PosName = station.PosName,
                    PosCode = station.PosCode,
                    BranchId = station.BranchId,
                    AssignedUser = station.AssignedUser,
                    GlCashAccount = station.GlCashAccount,
                    GlSalesAccount = station.GlSalesAccount,
                    AllowedPaymentMethods = station.AllowedPaymentMethods,
                    UserPermissions = station.UserPermissions,
                    IsActive = station.IsActive,
                    CompanyId = station.CompanyId,
                    CreatedDate = station.CreatedDate,
                    UpdatedDate = station.UpdatedDate
                };
                _isEditMode = true;
            }
            
            // تشخيص
            System.Diagnostics.Debug.WriteLine($"Initialize: PosId={Station.PosId}, _isEditMode={_isEditMode}, PosName={Station.PosName}, PosCode={Station.PosCode}");
            
            // تحديث العنوان
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(Station));
            
            // تحديث حالة الأزرار بعد توليد الكود
            SaveCommand.NotifyCanExecuteChanged();
            
            // تحديث إضافي للتأكد من تحديث الزر
            await Task.Delay(100); // تأخير بسيط للتأكد من تحديث الـ UI
            SaveCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// عنوان النافذة
        /// </summary>
        public string WindowTitle => _isEditMode ? "تعديل نقطة البيع" : "إضافة نقطة بيع جديدة";

        /// <summary>
        /// تحميل البيانات المساعدة
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "جاري تحميل البيانات...";

                // تحميل الفروع من قاعدة البيانات حسب الشركة الحالية
                var currentCompanyId = _sessionService.CurrentUser?.CompanyId ?? 1; // استخدام CompanyId من الجلسة الحالية
                var branches = await _context.Branches
                    .Where(b => b.IsActive == true && b.ComiId == currentCompanyId) // الفروع النشطة للشركة الحالية
                    .OrderBy(b => b.BranchName)
                    .ToListAsync();

                Branches.Clear();
                foreach (var branch in branches)
                {
                    Branches.Add(branch);
                }

                // تحميل المستخدمين من قاعدة البيانات
                var users = await _context.Nextuser
                    .OrderBy(u => u.Name) // استخدام Name بدلاً من Username
                    .ToListAsync();

                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }

                // تحديث الخصائص لإشعار الـ UI
                OnPropertyChanged(nameof(Branches));
                OnPropertyChanged(nameof(Users));
                OnPropertyChanged(nameof(Station));
                SaveCommand.NotifyCanExecuteChanged();

                StatusMessage = $"تم تحميل {Branches.Count} فرع و {Users.Count} مستخدم بنجاح (الشركة: {currentCompanyId})";
                
                // تحديث إضافي للتأكد من تحديث الـ ComboBox
                await Task.Delay(100); // تأخير بسيط للتأكد من تحديث الـ UI
                OnPropertyChanged(nameof(Branches));
                OnPropertyChanged(nameof(Users));
                SaveCommand.NotifyCanExecuteChanged();
                
                // تحديث إضافي للتأكد من تحديث الزر
                OnPropertyChanged(nameof(Station));
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في تحميل البيانات: {ex.Message}";
                MessageBox.Show($"حدث خطأ في تحميل البيانات:\n{ex.Message}\n\nتفاصيل إضافية:\n{ex.StackTrace}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                SaveCommand.NotifyCanExecuteChanged();
                
                // تحديث إضافي للتأكد من تحديث الزر
                OnPropertyChanged(nameof(Station));
                
                // تحديث نهائي للتأكد من تحديث الزر
                await Task.Delay(50);
                SaveCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// حفظ نقطة البيع
        /// </summary>
        private async Task SaveAsync()
        {
            try
            {
                StatusMessage = "بدء عملية الحفظ...";
                IsLoading = true;
                StatusMessage = "جاري التحقق من البيانات...";

                // التحقق من البيانات المطلوبة
                if (Station.BranchId <= 0)
                {
                    StatusMessage = "يرجى اختيار الفرع";
                    MessageBox.Show("يرجى اختيار الفرع من القائمة المنسدلة.", "بيانات مطلوبة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Station.AssignedUser <= 0)
                {
                    StatusMessage = "يرجى اختيار المستخدم المسند";
                    MessageBox.Show("يرجى اختيار المستخدم المسند من القائمة المنسدلة.", "بيانات مطلوبة", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // التحقق من صحة البيانات
                var isValid = await _posStationService.ValidateAsync(Station);
                if (!isValid)
                {
                    StatusMessage = "البيانات المدخلة غير صحيحة";
                    MessageBox.Show("يرجى التحقق من البيانات المدخلة والتأكد من ملء جميع الحقول المطلوبة.",
                        "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                StatusMessage = _isEditMode ? "جاري تحديث نقطة البيع..." : "جاري إضافة نقطة البيع...";

                // تشخيص
                System.Diagnostics.Debug.WriteLine($"SaveAsync: _isEditMode={_isEditMode}, Station.PosId={Station.PosId}");

                PosStation savedStation;
                if (_isEditMode)
                {
                    savedStation = await _posStationService.UpdateAsync(Station);
                }
                else
                {
                    savedStation = await _posStationService.AddAsync(Station);
                }

                StatusMessage = _isEditMode ? "تم تحديث نقطة البيع بنجاح" : "تم إضافة نقطة البيع بنجاح";
                
                MessageBox.Show(_isEditMode ? "تم تحديث نقطة البيع بنجاح" : "تم إضافة نقطة البيع بنجاح",
                    "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                // إغلاق النافذة
                CloseRequested?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في الحفظ: {ex.Message}";
                MessageBox.Show($"حدث خطأ في حفظ المحطة:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                SaveCommand.NotifyCanExecuteChanged();
                
                // تحديث إضافي للتأكد من تحديث الزر
                OnPropertyChanged(nameof(Station));
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
            var posNameValid = !string.IsNullOrWhiteSpace(Station.PosName);
            var posCodeValid = !string.IsNullOrWhiteSpace(Station.PosCode);
            var glCashAccountValid = !string.IsNullOrWhiteSpace(Station.GlCashAccount);
            var glSalesAccountValid = !string.IsNullOrWhiteSpace(Station.GlSalesAccount);
            var branchIdValid = Station.BranchId > 0;
            var assignedUserValid = Station.AssignedUser > 0;
            var notLoading = !IsLoading;
            
            // في وضع الإضافة، نسمح بالحفظ حتى لو لم يتم اختيار BranchId و AssignedUser بعد
            // سيتم التحقق من هذه القيم في SaveAsync
            var canSave = posNameValid && posCodeValid && glCashAccountValid && 
                         glSalesAccountValid && notLoading;
            
            // في وضع التعديل، نطلب BranchId و AssignedUser
            if (_isEditMode)
            {
                canSave = canSave && branchIdValid && assignedUserValid;
            }
            
            // تشخيص مفصل
            StatusMessage = $"CanSave: {canSave} | " +
                           $"PosName: {posNameValid} ({Station.PosName}) | " +
                           $"PosCode: {posCodeValid} ({Station.PosCode}) | " +
                           $"GlCashAccount: {glCashAccountValid} ({Station.GlCashAccount}) | " +
                           $"GlSalesAccount: {glSalesAccountValid} ({Station.GlSalesAccount}) | " +
                           $"BranchId: {branchIdValid} ({Station.BranchId}) | " +
                           $"AssignedUser: {assignedUserValid} ({Station.AssignedUser}) | " +
                           $"NotLoading: {notLoading} ({IsLoading}) | " +
                           $"IsEditMode: {_isEditMode}";
            
            // تحديث إضافي للتأكد من تحديث الزر
            OnPropertyChanged(nameof(Station));
            
            return canSave;
        }

        /// <summary>
        /// إغلاق النافذة
        /// </summary>
        private void CloseWindow(bool dialogResult)
        {
            // البحث عن النافذة الحالية وإغلاقها
            var currentWindow = Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);
            
            if (currentWindow != null)
            {
                currentWindow.DialogResult = dialogResult;
                currentWindow.Close();
            }
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

        ~PosStationEditViewModel()
        {
            Dispose(false);
        }
    }
}
