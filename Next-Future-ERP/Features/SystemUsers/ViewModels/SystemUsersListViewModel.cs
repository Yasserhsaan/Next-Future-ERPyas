using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.SystemUsers.Services;
using Next_Future_ERP.Features.SystemUsers.Models;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Auth.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Next_Future_ERP.Features.SystemUsers.Views;

namespace Next_Future_ERP.Features.SystemUsers.ViewModels
{
    /// <summary>
    /// ViewModel لقائمة مستخدمي النظام
    /// </summary>
    public partial class SystemUsersListViewModel : ObservableObject, IDisposable
    {
        private readonly ISystemUserService _systemUserService;
        private readonly IServiceProvider _serviceProvider;
        private bool _disposed = false;

        [ObservableProperty]
        private ObservableCollection<SystemUser> _users = new();

        [ObservableProperty]
        private SystemUser? _selectedUser;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _showActiveOnly = true;

        public SystemUsersListViewModel(ISystemUserService systemUserService, IServiceProvider serviceProvider)
        {
            _systemUserService = systemUserService;
            _serviceProvider = serviceProvider;
            
            LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync);
            SearchCommand = new AsyncRelayCommand(SearchAsync);
            AddUserCommand = new RelayCommand(AddUser);
            EditUserCommand = new RelayCommand<SystemUser>(EditUser);
            EditSelectedUserCommand = new RelayCommand(EditSelectedUser);
            DeleteUserCommand = new AsyncRelayCommand<SystemUser>(DeleteUserAsync);
            ToggleLockCommand = new AsyncRelayCommand<SystemUser>(ToggleLockAsync);
            RefreshCommand = new AsyncRelayCommand(LoadUsersAsync);
            ExportCommand = new AsyncRelayCommand(ExportAsync);
            ResetPasswordCommand = new AsyncRelayCommand<SystemUser>(ResetPasswordAsync);
        }

        public IAsyncRelayCommand LoadUsersCommand { get; }
        public IAsyncRelayCommand SearchCommand { get; }
        public IRelayCommand AddUserCommand { get; }
        public IRelayCommand<SystemUser> EditUserCommand { get; }
        public IRelayCommand EditSelectedUserCommand { get; }
        public IAsyncRelayCommand<SystemUser> DeleteUserCommand { get; }
        public IAsyncRelayCommand<SystemUser> ToggleLockCommand { get; }
        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand ExportCommand { get; }
        public IAsyncRelayCommand<SystemUser> ResetPasswordCommand { get; }

        public async Task InitializeAsync()
        {
            await LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            IsLoading = true;
            StatusMessage = "جاري تحميل مستخدمي النظام...";
            try
            {
                // جرب تحميل جميع البيانات أولاً للتأكد من وجودها
                var allResult = await _systemUserService.GetAllAsync();
                System.Diagnostics.Debug.WriteLine($"LoadUsersAsync: Found {allResult.Count} total users");
                
                // ثم جرب مع الفلتر
                var result = await _systemUserService.GetAllAsync(
                    companyId: null, // جرب بدون فلتر أولاً
                    branchId: null,
                    isActive: ShowActiveOnly ? true : null
                );
                System.Diagnostics.Debug.WriteLine($"LoadUsersAsync: Found {result.Count} filtered users");
                
                // إذا لم توجد بيانات مع الفلتر، استخدم جميع البيانات
                if (result.Count == 0 && allResult.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("LoadUsersAsync: No filtered users found, using all users");
                    result = allResult;
                }
                
                Users = new ObservableCollection<SystemUser>(result.OrderBy(u => u.Name).ThenBy(u => u.FirstName));
                StatusMessage = $"تم تحميل {Users.Count} مستخدم نظام.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في تحميل مستخدمي النظام: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"LoadUsersAsync Error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"خطأ في تحميل مستخدمي النظام: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchAsync()
        {
            IsLoading = true;
            StatusMessage = "جاري البحث...";
            try
            {
                var result = await _systemUserService.SearchAsync(SearchText);
                Users = new ObservableCollection<SystemUser>(result.OrderBy(u => u.Name).ThenBy(u => u.FirstName));
                StatusMessage = $"تم العثور على {Users.Count} مستخدم نظام.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في البحث عن مستخدمي النظام: {ex.Message}";
                MessageBox.Show($"خطأ في البحث عن مستخدمي النظام: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// إضافة مستخدم نظام جديد
        /// </summary>
        private async void AddUser()
        {
            try
            {
                var editWindow = _serviceProvider.GetRequiredService<SystemUserEditWindow>();
                var editViewModel = _serviceProvider.GetRequiredService<SystemUserEditViewModel>();
                
                await editViewModel.Initialize(new SystemUser());
                await editViewModel.InitializeAsync(); // تحميل البيانات المساعدة

                editWindow.DataContext = editViewModel;
                
                if (editWindow.ShowDialog() == true)
                {
                    LoadUsersAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ في فتح نافذة إضافة مستخدم النظام:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// تعديل مستخدم نظام
        /// </summary>
        private async void EditUser(SystemUser? user)
        {
            if (user == null) return;

            try
            {
                var editViewModel = await SystemUserEditViewModel.FromExisting(
                    _systemUserService,
                    _serviceProvider,
                    _serviceProvider.GetRequiredService<AppDbContext>(),
                    _serviceProvider.GetRequiredService<ISessionService>(),
                    user.Id);

                var editWindow = _serviceProvider.GetRequiredService<SystemUserEditWindow>();
                editWindow.DataContext = editViewModel;
                
                if (editWindow.ShowDialog() == true)
                {
                    LoadUsersAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ في فتح نافذة تعديل مستخدم النظام:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditSelectedUser()
        {
            if (SelectedUser == null)
            {
                MessageBox.Show("يرجى اختيار مستخدم نظام للتعديل عليها", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EditUser(SelectedUser);
        }

        private async Task DeleteUserAsync(SystemUser? user)
        {
            if (user == null) return;

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف مستخدم النظام '{user.FullName}'؟\nهذا الإجراء لا يمكن التراجع عنه.",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                IsLoading = true;
                StatusMessage = "جاري حذف مستخدم النظام...";
                try
                {
                    await _systemUserService.DeleteAsync(user.Id);
                    Users.Remove(user);
                    StatusMessage = "تم حذف مستخدم النظام بنجاح.";
                    MessageBox.Show("تم حذف مستخدم النظام بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    StatusMessage = $"خطأ في حذف مستخدم النظام: {ex.Message}";
                    MessageBox.Show($"خطأ في حذف مستخدم النظام: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task ToggleLockAsync(SystemUser? user)
        {
            if (user == null) return;

            IsLoading = true;
            StatusMessage = "جاري تحديث حالة قفل المستخدم...";
            try
            {
                await _systemUserService.ToggleLockAsync(user.Id);
                user.IsLocked = !user.IsLocked; // Update UI
                StatusMessage = "تم تحديث حالة قفل المستخدم بنجاح.";
                MessageBox.Show("تم تحديث حالة قفل المستخدم بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في تحديث حالة قفل المستخدم: {ex.Message}";
                MessageBox.Show($"خطأ في تحديث حالة قفل المستخدم: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ResetPasswordAsync(SystemUser? user)
        {
            if (user == null) return;

            var result = MessageBox.Show(
                $"هل أنت متأكد من إعادة تعيين كلمة المرور للمستخدم '{user.FullName}'؟\nسيتم تعيين كلمة مرور جديدة: '123456'",
                "تأكيد إعادة تعيين كلمة المرور",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                IsLoading = true;
                StatusMessage = "جاري إعادة تعيين كلمة المرور...";
                try
                {
                    await _systemUserService.ResetPasswordAsync(user.Id, "123456");
                    StatusMessage = "تم إعادة تعيين كلمة المرور بنجاح.";
                    MessageBox.Show("تم إعادة تعيين كلمة المرور بنجاح.\nكلمة المرور الجديدة: 123456", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    StatusMessage = $"خطأ في إعادة تعيين كلمة المرور: {ex.Message}";
                    MessageBox.Show($"خطأ في إعادة تعيين كلمة المرور: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private Task ExportAsync()
        {
            StatusMessage = "وظيفة التصدير قيد التنفيذ.";
            MessageBox.Show("وظيفة التصدير قيد التنفيذ.", "تصدير", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
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
                _disposed = true;
            }
        }

        ~SystemUsersListViewModel()
        {
            Dispose(false);
        }
    }
}
