using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.SystemUsers.Services;
using Next_Future_ERP.Features.SystemUsers.Models;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Auth.Services;
using Next_Future_ERP.Features.InitialSystem.Models;
using Next_Future_ERP.Features.Permissions.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace Next_Future_ERP.Features.SystemUsers.ViewModels
{
    /// <summary>
    /// ViewModel لتعديل مستخدم النظام
    /// </summary>
    public partial class SystemUserEditViewModel : ObservableObject
    {
        private readonly ISystemUserService _systemUserService;
        private readonly IServiceProvider _serviceProvider;
        private readonly AppDbContext _context;
        private readonly ISessionService _sessionService;
        private bool _isEditMode = false;

        [ObservableProperty]
        private SystemUser _systemUser = new();

        [ObservableProperty]
        private ObservableCollection<BranchModel> _branches = new();

        [ObservableProperty]
        private ObservableCollection<SysRole> _roles = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        public SystemUserEditViewModel(ISystemUserService systemUserService, IServiceProvider serviceProvider, AppDbContext context, ISessionService sessionService)
        {
            _systemUserService = systemUserService;
            _serviceProvider = serviceProvider;
            _context = context;
            _sessionService = sessionService;

            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        public IAsyncRelayCommand LoadDataCommand { get; }
        public IAsyncRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public event EventHandler<bool>? CloseRequested;

        public string WindowTitle => _isEditMode ? "تعديل مستخدم النظام" : "إضافة مستخدم نظام جديد";

        public static async Task<SystemUserEditViewModel> FromExisting(ISystemUserService systemUserService, IServiceProvider serviceProvider, AppDbContext context, ISessionService sessionService, int userId)
        {
            var viewModel = new SystemUserEditViewModel(systemUserService, serviceProvider, context, sessionService);
            await viewModel.Initialize(userId);
            return viewModel;
        }

        public async Task Initialize(SystemUser systemUser)
        {
            _isEditMode = systemUser.Id > 0;
            SystemUser = systemUser;
            await LoadDataCommand.ExecuteAsync(null);
        }

        public async Task Initialize(int userId)
        {
            var user = await _systemUserService.GetByIdAsync(userId);
            if (user != null)
            {
                await Initialize(user);
            }
        }

        public async Task InitializeAsync()
        {
            await LoadDataCommand.ExecuteAsync(null);
        }

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

                // تحميل الفروع
                var branches = await _context.Branches
                    .OrderBy(b => b.BranchName)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"LoadDataAsync: Found {branches.Count} branches");

                Branches.Clear();
                foreach (var branch in branches)
                {
                    Branches.Add(branch);
                }

                // تحميل الأدوار
                var roles = await _context.SysRoles
                    .OrderBy(r => r.Name)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"LoadDataAsync: Found {roles.Count} roles");

                Roles.Clear();
                foreach (var role in roles)
                {
                    Roles.Add(role);
                }

                OnPropertyChanged(nameof(Branches));
                OnPropertyChanged(nameof(Roles));
                OnPropertyChanged(nameof(SystemUser));
                SaveCommand.NotifyCanExecuteChanged();

                StatusMessage = $"تم تحميل {Branches.Count} فرع و {Roles.Count} دور بنجاح.";
                
                // تحديث إضافي للتأكد من تحديث الـ ComboBox
                await Task.Delay(100);
                OnPropertyChanged(nameof(Branches));
                OnPropertyChanged(nameof(Roles));
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
                OnPropertyChanged(nameof(SystemUser));
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "جاري الحفظ...";

                // التحقق من كلمة المرور
                if (!_isEditMode && string.IsNullOrWhiteSpace(Password))
                {
                    MessageBox.Show("يرجى إدخال كلمة المرور", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(Password) && Password != ConfirmPassword)
                {
                    MessageBox.Show("كلمة المرور وتأكيد كلمة المرور غير متطابقتين", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // تعيين كلمة المرور إذا تم إدخالها
                if (!string.IsNullOrWhiteSpace(Password))
                {
                    SystemUser.Password = Password;
                }

                SystemUser savedUser;
                if (_isEditMode)
                {
                    savedUser = await _systemUserService.UpdateAsync(SystemUser);
                }
                else
                {
                    savedUser = await _systemUserService.AddAsync(SystemUser);
                }

                StatusMessage = _isEditMode ? "تم تحديث مستخدم النظام بنجاح" : "تم إضافة مستخدم النظام بنجاح";
                
                MessageBox.Show(_isEditMode ? "تم تحديث مستخدم النظام بنجاح" : "تم إضافة مستخدم النظام بنجاح",
                    "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                // تأخير صغير للتأكد من أن النافذة جاهزة
                await Task.Delay(100);

                // إغلاق النافذة
                CloseRequested?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في الحفظ: {ex.Message}";
                MessageBox.Show($"حدث خطأ في حفظ مستخدم النظام:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                SaveCommand.NotifyCanExecuteChanged();
            }
        }

        private bool CanSave()
        {
            if (IsLoading) return false;
            if (string.IsNullOrWhiteSpace(SystemUser.Code)) return false;
            if (string.IsNullOrWhiteSpace(SystemUser.Name)) return false;
            if (string.IsNullOrWhiteSpace(SystemUser.FirstName)) return false;
            if (string.IsNullOrWhiteSpace(SystemUser.Email)) return false;
            if (string.IsNullOrWhiteSpace(SystemUser.Mobile)) return false;
            if (string.IsNullOrWhiteSpace(SystemUser.Phone)) return false;
            if (string.IsNullOrWhiteSpace(SystemUser.Address)) return false;

            // التحقق من كلمة المرور للإضافة فقط
            if (!_isEditMode && string.IsNullOrWhiteSpace(Password)) return false;

            return true;
        }

        private void Cancel()
        {
            CloseRequested?.Invoke(this, false);
        }

        partial void OnSystemUserChanged(SystemUser value)
        {
            SaveCommand.NotifyCanExecuteChanged();
        }

        partial void OnPasswordChanged(string value)
        {
            SaveCommand.NotifyCanExecuteChanged();
        }

        partial void OnConfirmPasswordChanged(string value)
        {
            SaveCommand.NotifyCanExecuteChanged();
        }
    }
}
