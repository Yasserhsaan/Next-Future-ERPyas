using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Auth.Services;
using Next_Future_ERP.Features.Permissions.Models;
using Next_Future_ERP.Features.Permissions.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Next_Future_ERP.Features.Permissions.ViewModels
{
    public partial class PermissionsMainViewModel : ObservableObject, IDisposable
    {
        private readonly IPermissionService _permissionService;
        private readonly ISessionService _sessionService;

        [ObservableProperty]
        private ObservableCollection<MenuTreeItem> menuTreeItems = new();

        [ObservableProperty]
        private ObservableCollection<SysRole> roles = new();

        [ObservableProperty]
        private ObservableCollection<Next_Future_ERP.Data.Models.Nextuser> users = new();

        [ObservableProperty]
        private ObservableCollection<MenuForm> menuForms = new();

        [ObservableProperty]
        private ObservableCollection<UserPermission> userPermissions = new();

        [ObservableProperty]
        private ObservableCollection<UserPermission> rolePermissions = new();

        [ObservableProperty]
        private SysRole newRole = new();

        [ObservableProperty]
        private bool isAddingRole = false;

        [ObservableProperty]
        private bool isEditingRole = false;

        [ObservableProperty]
        private MenuTreeItem? selectedMenuTreeItem;

        [ObservableProperty]
        private SysRole? selectedRole;

        [ObservableProperty]
        private Next_Future_ERP.Data.Models.Nextuser? selectedUser;

        [ObservableProperty]
        private MenuForm? selectedMenuForm;

        [ObservableProperty]
        private MenuForm? selectedParentMenu;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private bool isLoadingMenuTree = false;

        [ObservableProperty]
        private bool isLoadingMenuForms = false;

        [ObservableProperty]
        private bool isLoadingRoles = false;

        [ObservableProperty]
        private bool isLoadingUsers = false;

        [ObservableProperty]
        private bool isLoadingUserPermissions = false;

        [ObservableProperty]
        private bool isLoadingRolePermissions = false;

        [ObservableProperty]
        private string loadingMessage = string.Empty;

        [ObservableProperty]
        private int loadingProgress = 0;

        [ObservableProperty]
        private int totalLoadingSteps = 0;

        [ObservableProperty]
        private string currentUserRoleContext = string.Empty;

        [ObservableProperty]
        private bool canSaveRolePermissions = false;

        [ObservableProperty]
        private bool isAutoUpdateEnabled = false;

        [ObservableProperty]
        private int autoUpdateInterval = 30; // seconds

        [ObservableProperty]
        private DateTime lastUpdateTime = DateTime.Now;

        [ObservableProperty]
        private string autoUpdateStatus = string.Empty;

        private DispatcherTimer? _autoUpdateTimer;
        private readonly object _updateLock = new object();

        [ObservableProperty]
        private bool isAddingNew = false;

        [ObservableProperty]
        private string currentView = "MenuEditor"; // MenuEditor, RoleEditor, UserPermissions

        [ObservableProperty]
        private MenuForm newMenuForm = new();

        // Collection Views for Grouping
        public ICollectionView MenuFormsView { get; private set; }
        public ICollectionView RolesView { get; private set; }
        public ICollectionView UserPermissionsView { get; private set; }

        // Current context
        public int CurrentUserId { get; set; } = 1;
        public int CurrentCompanyId { get; set; } = 1;
        public int CurrentBranchId { get; set; } = 1;

        public PermissionsMainViewModel(IPermissionService permissionService, ISessionService sessionService)
        {
            _permissionService = permissionService;
            _sessionService = sessionService;
            InitializeCollectionViews();
            InitializeAutoUpdate();
            // Start async initialization
            _ = InitializeAsync();
        }

        private void InitializeCollectionViews()
        {
            // Initialize MenuFormsView with grouping
            MenuFormsView = CollectionViewSource.GetDefaultView(MenuForms);
            MenuFormsView.GroupDescriptions?.Add(new PropertyGroupDescription("GroupName"));
            MenuFormsView.SortDescriptions.Add(new SortDescription("MenuFormCode", ListSortDirection.Ascending));

            // Initialize RolesView with grouping
            RolesView = CollectionViewSource.GetDefaultView(Roles);
            RolesView.GroupDescriptions?.Add(new PropertyGroupDescription("RollTypeName"));
            RolesView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            // Initialize UserPermissionsView with grouping
            UserPermissionsView = CollectionViewSource.GetDefaultView(UserPermissions);
            UserPermissionsView.GroupDescriptions?.Add(new PropertyGroupDescription("MenuForm.GroupName"));
            UserPermissionsView.SortDescriptions.Add(new SortDescription("MenuForm.MenuName", ListSortDirection.Ascending));
        }

        private async Task InitializeAsync()
        {
            await LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                LoadingProgress = 0;
                TotalLoadingSteps = 4;
                LoadingMessage = "بدء تحميل البيانات...";

                LoadingMessage = "تحميل شجرة القوائم...";
                await LoadMenuTreeAsync();
                LoadingProgress = 1;

                LoadingMessage = "تحميل الأدوار...";
                await LoadRolesAsync();
                LoadingProgress = 2;

                LoadingMessage = "تحميل المستخدمين...";
                await LoadUsersAsync();
                LoadingProgress = 3;

                LoadingMessage = "تحميل القوائم...";
                await LoadMenuFormsAsync();
                LoadingProgress = 4;

                LoadingMessage = "تم تحميل جميع البيانات بنجاح";
                
            }
            catch (Exception ex)
            {
                LoadingMessage = $"خطأ أثناء تحميل البيانات: {ex.Message}";
                //  MessageBox.Show($"❌ خطأ أثناء تحميل البيانات:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                LoadingProgress = 0;
                LoadingMessage = string.Empty;
            }
        }

        [RelayCommand]
        private async Task LoadMenuTreeAsync()
        {
            try
            {
                IsLoadingMenuTree = true;
                MenuTreeItems.Clear();
                var items = await _permissionService.GetUserMenuTreeAsync(CurrentUserId, CurrentCompanyId, CurrentBranchId);
                foreach (var item in items)
                {
                    MenuTreeItems.Add(item);
                }
            }
            catch (Exception ex)
            {
               // MessageBox.Show($"❌ خطأ أثناء تحميل شجرة القوائم:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoadingMenuTree = false;
            }
        }

        [RelayCommand]
        private async Task LoadMenuFormsAsync()
        {
            try
            {
                IsLoadingMenuForms = true;
                MenuForms.Clear();
                var items = await _permissionService.GetAllMenuFormsAsync();
                
                foreach (var item in items)
                {
                    MenuForms.Add(item);
                }
                MenuFormsView.Refresh();
            }
            catch (Exception ex)
            {
               // MessageBox.Show($"❌ خطأ أثناء تحميل القوائم:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoadingMenuForms = false;
            }
        }

        [RelayCommand]
        private async Task LoadRolesAsync()
        {
            try
            {
                IsLoadingRoles = true;
                Roles.Clear();
                var items = await _permissionService.GetAllRolesAsync();
                
                foreach (var item in items)
                {
                    Roles.Add(item);
                }
                RolesView.Refresh();
            }
            catch (Exception ex)
            {
               // MessageBox.Show($"❌ خطأ أثناء تحميل الأدوار:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoadingRoles = false;
            }
        }

        [RelayCommand]
        private async Task LoadUsersAsync()
        {
            try
            {
                IsLoadingUsers = true;
                Users.Clear();
                var items = await _permissionService.GetAllUsersAsync();
                
                foreach (var item in items)
                {
                    Users.Add(item);
                }
            }
            catch (Exception ex)
            {
               // MessageBox.Show($"❌ خطأ أثناء تحميل المستخدمين:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoadingUsers = false;
            }
        }

        [RelayCommand]
        private async Task LoadUserPermissionsAsync()
        {
            try
            {
                if (SelectedUser == null) return;

                IsLoadingUserPermissions = true;
                UserPermissions.Clear();
                var items = await _permissionService.GetUserPermissionsAsync(SelectedUser.ID, CurrentCompanyId, CurrentBranchId);
                foreach (var item in items)
                {
                    UserPermissions.Add(item);
                }
                UserPermissionsView.Refresh();
            }
            catch (Exception ex)
            {
               // MessageBox.Show($"❌ خطأ أثناء تحميل صلاحيات المستخدم:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoadingUserPermissions = false;
            }
        }

        [RelayCommand]
        private async Task LoadRolePermissionsAsync()
        {
            try
            {
                if (SelectedRole == null) return;

                IsLoadingRolePermissions = true;
                UserPermissions.Clear();
                // Load permissions for the selected role
                var items = await _permissionService.GetRolePermissionsAsync(SelectedRole.Id, CurrentCompanyId, CurrentBranchId,SelectedUser.ID);
                foreach (var item in items)
                {
                    UserPermissions.Add(item);
                }
                UserPermissionsView.Refresh();
            }
            catch (Exception ex)
            {
               // MessageBox.Show($"❌ خطأ أثناء تحميل صلاحيات الدور:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoadingRolePermissions = false;
            }
        }

        [RelayCommand]
        private void SwitchToMenuEditor()
        {
            CurrentView = "MenuEditor";
        }

        [RelayCommand]
        private void SwitchToRoleEditor()
        {
            CurrentView = "RoleEditor";
        }

        [RelayCommand]
        private void SwitchToUserPermissions()
        {
            CurrentView = "UserPermissions";
        }

        [RelayCommand]
        private void StartAddingNew()
        {
            IsAddingNew = true;
            NewMenuForm = new MenuForm();
        }

        [RelayCommand]
        private async Task AddMenuForm()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewMenuForm.MenuName))
                {
                    MessageBox.Show("يرجى إدخال اسم القائمة", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _permissionService.AddMenuFormAsync(NewMenuForm);
                await LoadMenuFormsAsync();
                await LoadMenuTreeAsync();
                
                IsAddingNew = false;
                NewMenuForm = new MenuForm();
                
                MessageBox.Show("تم إضافة القائمة بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء إضافة القائمة:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task EditMenuForm()
        {
            try
            {
                if (SelectedMenuForm == null)
                {
                    MessageBox.Show("يرجى اختيار قائمة للتعديل", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _permissionService.UpdateMenuFormAsync(SelectedMenuForm);
                await LoadMenuFormsAsync();
                await LoadMenuTreeAsync();
                
                MessageBox.Show("تم تعديل القائمة بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء تعديل القائمة:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteMenuForm()
        {
            try
            {
                if (SelectedMenuForm == null)
                {
                    MessageBox.Show("يرجى اختيار قائمة للحذف", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"هل أنت متأكد من حذف القائمة '{SelectedMenuForm.MenuName}'؟", 
                    "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    await _permissionService.DeleteMenuFormAsync(SelectedMenuForm.MenuFormCode);
                    await LoadMenuFormsAsync();
                    await LoadMenuTreeAsync();
                    
                    MessageBox.Show("تم حذف القائمة بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء حذف القائمة:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task RefreshData()
        {
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task SeedDatabase()
        {
            try
            {
                MessageBox.Show("بدء عملية تهيئة قاعدة البيانات...", "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
                
                IsLoading = true;
                var success = await _permissionService.SeedDatabaseAsync();
                if (success)
                {
                    await LoadDataAsync();
                    MessageBox.Show("تم تهيئة قاعدة البيانات بنجاح!", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء تهيئة قاعدة البيانات:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ClearAllData()
        {
            try
            {
                IsLoading = true;
                var success = await _permissionService.ClearAllPermissionDataAsync();
                if (success)
                {
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء حذف البيانات:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSelectedMenuFormChanged(MenuForm? value)
        {
            if (value != null)
            {
                NewMenuForm = new MenuForm
                {
                    MenuFormCode = value.MenuFormCode,
                    MenuName = value.MenuName,
                    MenuArabicName = value.MenuArabicName,
                    ProgramExecutable = value.ProgramExecutable,
                    MenuFormParent = value.MenuFormParent,
                    Visible = value.Visible
                };
            }
        }

        partial void OnSelectedUserChanged(Next_Future_ERP.Data.Models.Nextuser? value)
        {
            if (value != null)
            {
                // Auto-select the user's role based on UserRollid
                _ = SelectUserRoleAsync(value.UserRollid);
                
                // Load user-specific permissions
                _ = LoadUserPermissionsAsync();
                
                // Update context display
                UpdateUserRoleContext();
            }
            else
            {
                // Clear permissions when no user is selected
                UserPermissions.Clear();
                RolePermissions.Clear();
                SelectedRole = null;
                CurrentUserRoleContext = string.Empty;
            }
        }

        partial void OnSelectedRoleChanged(SysRole? value)
        {
            if (value != null && SelectedUser != null)
            {
                _ = LoadRolePermissionsForSelectedRoleAsync();
            }
            else if (value == null)
            {
                // Clear role permissions when no role is selected
                RolePermissions.Clear();
            }
            
            // Update context display
            UpdateUserRoleContext();
        }

        [RelayCommand]
        private async Task LoadRolePermissionsForSelectedRoleAsync()
        {
            try
            {
                if (SelectedRole == null || SelectedUser == null) 
                {
                    RolePermissions.Clear();
                    return;
                }

                IsLoadingRolePermissions = true;
                RolePermissions.Clear();
                
                // Get all menu forms
                var allMenuForms = await _permissionService.GetAllMenuFormsAsync();

                // Get existing permissions for this role and user
                var existingPermissions = await _permissionService.GetRolePermissionsAsync(SelectedRole.Id, CurrentCompanyId, CurrentBranchId, SelectedUser.ID);
                var permissionLookup = existingPermissions.ToLookup(p => p.FormId);

                // Create permission entries for all menu forms
                foreach (var menuForm in allMenuForms)
                {
                    var existingPermission = permissionLookup[menuForm.MenuFormCode].FirstOrDefault();
                    
                    var permission = new UserPermission
                    {
                        UserId = SelectedUser.ID,
                        FormId = menuForm.MenuFormCode,
                        RoleId = SelectedRole.Id,
                        CompanyId = CurrentCompanyId,
                        BranchId = CurrentBranchId,
                        MenuForm = menuForm,
                        SysRole = SelectedRole,
                        AllowAdd = existingPermission?.AllowAdd ?? false,
                        AllowEdit = existingPermission?.AllowEdit ?? false,
                        AllowDelete = existingPermission?.AllowDelete ?? false,
                        AllowView = existingPermission?.AllowView ?? false,
                        AllowPost = existingPermission?.AllowPost ?? false,
                        AllowPrint = existingPermission?.AllowPrint ?? false,
                        AllowRun = existingPermission?.AllowRun ?? false
                    };
                    
                    RolePermissions.Add(permission);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"❌ خطأ أثناء تحميل صلاحيات الدور:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoadingRolePermissions = false;
            }
        }

        [RelayCommand]
        private void StartAddingRole()
        {
            IsAddingRole = true;
            IsEditingRole = false;
            NewRole = new SysRole { RollType = 3 }; // Default to User type
        }

        [RelayCommand]
        private void StartEditingRole()
        {
            if (SelectedRole == null)
            {
                MessageBox.Show("يرجى اختيار دور للتعديل", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsEditingRole = true;
            IsAddingRole = false;
            NewRole = new SysRole
            {
                Id = SelectedRole.Id,
                Name = SelectedRole.Name,
                RollType = SelectedRole.RollType
            };
        }

        [RelayCommand]
        private async Task AddRole()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewRole.Name))
                {
                    MessageBox.Show("يرجى إدخال اسم الدور", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var success = await _permissionService.AddRoleAsync(NewRole);
                if (success)
                {
                    await LoadRolesAsync();
                    IsAddingRole = false;
                    NewRole = new SysRole();
                    MessageBox.Show("تم إضافة الدور بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء إضافة الدور:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task EditRole()
        {
            try
            {
                if (SelectedRole == null)
                {
                    MessageBox.Show("يرجى اختيار دور للتعديل", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(NewRole.Name))
                {
                    MessageBox.Show("يرجى إدخال اسم الدور", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var success = await _permissionService.UpdateRoleAsync(NewRole);
                if (success)
                {
                    await LoadRolesAsync();
                    IsEditingRole = false;
                    NewRole = new SysRole();
                    MessageBox.Show("تم تعديل الدور بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء تعديل الدور:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void SelectAllAdd()
        {
            foreach (var permission in RolePermissions)
            {
                permission.AllowAdd = true;
            }
        }

        [RelayCommand]
        private void DeselectAllAdd()
        {
            foreach (var permission in RolePermissions)
            {
                permission.AllowAdd = false;
            }
        }

        [RelayCommand]
        private void SelectAllEdit()
        {
            foreach (var permission in RolePermissions)
            {
                permission.AllowEdit = true;
            }
        }

        [RelayCommand]
        private void DeselectAllEdit()
        {
            foreach (var permission in RolePermissions)
            {
                permission.AllowEdit = false;
            }
        }

        [RelayCommand]
        private void SelectAllView()
        {
            foreach (var permission in RolePermissions)
            {
                permission.AllowView = true;
            }
        }

        [RelayCommand]
        private void DeselectAllView()
        {
            foreach (var permission in RolePermissions)
            {
                permission.AllowView = false;
            }
        }

        [RelayCommand]
        private void SelectAllDelete()
        {
            foreach (var permission in RolePermissions)
            {
                permission.AllowDelete = true;
            }
        }

        [RelayCommand]
        private void DeselectAllDelete()
        {
            foreach (var permission in RolePermissions)
            {
                permission.AllowDelete = false;
            }
        }

        [RelayCommand]
        private void SelectAllPost()
        {
            foreach (var permission in RolePermissions)
            {
                permission.AllowPost = true;
            }
        }

        [RelayCommand]
        private void DeselectAllPost()
        {
            foreach (var permission in RolePermissions)
            {
                permission.AllowPost = false;
            }
        }

        [RelayCommand]
        private void SelectAllPrint()
        {
            foreach (var permission in RolePermissions)
            {
                permission.AllowPrint = true;
            }
        }

        [RelayCommand]
        private void DeselectAllPrint()
        {
            foreach (var permission in RolePermissions)
            {
                permission.AllowPrint = false;
            }
        }

        [RelayCommand]
        private void SelectAllPermissions()
        {
            foreach (var permission in RolePermissions)
            {
                permission.AllowAdd = true;
                permission.AllowEdit = true;
                permission.AllowView = true;
                permission.AllowDelete = true;
                permission.AllowPost = true;
                permission.AllowPrint = true;
                permission.AllowRun = true;
            }
        }

        [RelayCommand]
        private void DeselectAllPermissions()
        {
            foreach (var permission in RolePermissions)
            {
                permission.AllowAdd = false;
                permission.AllowEdit = false;
                permission.AllowView = false;
                permission.AllowDelete = false;
                permission.AllowPost = false;
                permission.AllowPrint = false;
                permission.AllowRun = false;
            }
        }

        [RelayCommand]
        private async Task SaveRolePermissions()
        {
            try
            {
                if (SelectedRole == null)
                {
                    MessageBox.Show("يرجى اختيار دور", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedUser == null)
                {
                    MessageBox.Show("يرجى اختيار مستخدم", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if we're updating the user's actual role or a different role
                var isUserRole = SelectedRole.Id == SelectedUser.UserRollid;
                var confirmMessage = isUserRole 
                    ? $"سيتم حفظ صلاحيات الدور '{SelectedRole.Name}' للمستخدم '{SelectedUser.Name}' (دور المستخدم الأساسي)"
                    : $"تحذير: سيتم حفظ صلاحيات الدور '{SelectedRole.Name}' للمستخدم '{SelectedUser.Name}' ولكن دور المستخدم الأساسي هو {SelectedUser.UserRollid}";

                var result = MessageBox.Show($"{confirmMessage}\n\nهل تريد المتابعة؟", "تأكيد الحفظ", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result != MessageBoxResult.Yes) return;

                IsLoadingRolePermissions = true;
                LoadingMessage = "حفظ صلاحيات الدور...";

                foreach (var permission in RolePermissions)
                {
                    permission.RoleId = SelectedRole.Id;
                    permission.UserId = SelectedUser.ID;
                    await _permissionService.SaveUserPermissionAsync(permission);
                }

                // If we're updating a role that's not the user's primary role, 
                // consider updating the user's UserRollid if requested
                if (!isUserRole)
                {
                    var updateUserRoleResult = MessageBox.Show(
                        $"هل تريد تحديث دور المستخدم الأساسي من {SelectedUser.UserRollid} إلى {SelectedRole.Id}؟", 
                        "تحديث دور المستخدم", 
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (updateUserRoleResult == MessageBoxResult.Yes)
                    {
                        await UpdateUserRoleAsync(SelectedUser.ID, SelectedRole.Id);
                    }
                }

                // Refresh both user permissions and role permissions
                await LoadUserPermissionsAsync();
                await LoadRolePermissionsForSelectedRoleAsync();

                // Refresh menu tree to reflect new permissions
                await LoadMenuTreeAsync();

                // Refresh session permissions to update the main menu
                LoadingMessage = "تحديث قائمة التنقل الرئيسية...";
                await _sessionService.RefreshPermissionsAsync();

                var successMessage = isUserRole 
                    ? $"تم حفظ صلاحيات دور المستخدم '{SelectedRole.Name}' للمستخدم '{SelectedUser.Name}' بنجاح\n\n✅ تم تحديث قائمة التنقل الرئيسية"
                    : $"تم حفظ صلاحيات الدور '{SelectedRole.Name}' للمستخدم '{SelectedUser.Name}' بنجاح (دور إضافي)\n\n✅ تم تحديث قائمة التنقل الرئيسية";
                    
                MessageBox.Show(successMessage, "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء حفظ صلاحيات الدور:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoadingRolePermissions = false;
                LoadingMessage = string.Empty;
            }
        }

        [RelayCommand]
        private void CancelRoleEdit()
        {
            IsAddingRole = false;
            IsEditingRole = false;
            NewRole = new SysRole();
        }

        private void UpdateUserRoleContext()
        {
            if (SelectedUser != null && SelectedRole != null)
            {
                var roleMatchesUser = SelectedRole.Id == SelectedUser.UserRollid;
                var roleIndicator = roleMatchesUser ? "🔗" : "⚠️";
                var statusText = roleMatchesUser ? "(دور المستخدم)" : "(دور مختلف)";
                
                CurrentUserRoleContext = $"{roleIndicator} صلاحيات الدور '{SelectedRole.Name}' للمستخدم '{SelectedUser.Name}' {statusText}";
                CanSaveRolePermissions = true;
            }
            else if (SelectedUser != null)
            {
                CurrentUserRoleContext = $"صلاحيات المستخدم '{SelectedUser.Name}' (دور المستخدم: {SelectedUser.UserRollid}) - يرجى اختيار دور";
                CanSaveRolePermissions = false;
            }
            else
            {
                CurrentUserRoleContext = "يرجى اختيار مستخدم ودور لعرض الصلاحيات";
                CanSaveRolePermissions = false;
            }
        }

        [RelayCommand]
        private async Task RefreshRolePermissions()
        {
            if (SelectedRole != null && SelectedUser != null)
            {
                await LoadRolePermissionsForSelectedRoleAsync();
            }
        }

        private async Task SelectUserRoleAsync(int userRoleId)
        {
            try
            {
                // Find the role that matches the user's UserRollid
                var userRole = Roles.FirstOrDefault(r => r.Id == userRoleId);
                
                if (userRole != null)
                {
                    // Set the selected role without triggering the change handler recursively
                    SelectedRole = userRole;
                    
                    // Load role permissions for this user
                    await LoadRolePermissionsForSelectedRoleAsync();
                }
                else
                {
                    // If role not found, try to load it from the service
                    var roleFromService = await _permissionService.GetRoleByIdAsync(userRoleId);
                    if (roleFromService != null)
                    {
                        // Add to roles collection if not already there
                        if (!Roles.Any(r => r.Id == roleFromService.Id))
                        {
                            Roles.Add(roleFromService);
                            RolesView.Refresh();
                        }
                        
                        SelectedRole = roleFromService;
                        await LoadRolePermissionsForSelectedRoleAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                AutoUpdateStatus = $"خطأ في تحديد دور المستخدم: {ex.Message}";
            }
        }

        private async Task UpdateUserRoleAsync(int userId, int newRoleId)
        {
            try
            {
                // This would require a service method to update the user's UserRollid
                // For now, we'll update it locally and show a message
                if (SelectedUser != null && SelectedUser.ID == userId)
                {
                    SelectedUser.UserRollid = newRoleId;
                    
                    // Update the context to reflect the change
                    UpdateUserRoleContext();
                    
                    // Note: In a real implementation, you would call a service method here
                    // await _userService.UpdateUserRoleAsync(userId, newRoleId);
                    
                    MessageBox.Show($"تم تحديث دور المستخدم إلى {newRoleId} محلياً. يرجى تحديث قاعدة البيانات.", 
                        "تحديث الدور", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحديث دور المستخدم: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Auto Update Functionality

        private void InitializeAutoUpdate()
        {
            _autoUpdateTimer = new DispatcherTimer();
            _autoUpdateTimer.Tick += AutoUpdateTimer_Tick;
            UpdateAutoUpdateStatus();
        }

        partial void OnIsAutoUpdateEnabledChanged(bool value)
        {
            if (value)
            {
                StartAutoUpdate();
            }
            else
            {
                StopAutoUpdate();
            }
            UpdateAutoUpdateStatus();
        }

        partial void OnAutoUpdateIntervalChanged(int value)
        {
            if (_autoUpdateTimer != null && IsAutoUpdateEnabled)
            {
                _autoUpdateTimer.Interval = TimeSpan.FromSeconds(value);
                UpdateAutoUpdateStatus();
            }
        }

        [RelayCommand]
        private void ToggleAutoUpdate()
        {
            IsAutoUpdateEnabled = !IsAutoUpdateEnabled;
        }

        [RelayCommand]
        private void StartAutoUpdate()
        {
            if (_autoUpdateTimer != null && !_autoUpdateTimer.IsEnabled)
            {
                _autoUpdateTimer.Interval = TimeSpan.FromSeconds(AutoUpdateInterval);
                _autoUpdateTimer.Start();
                IsAutoUpdateEnabled = true;
                UpdateAutoUpdateStatus();
            }
        }

        [RelayCommand]
        private void StopAutoUpdate()
        {
            if (_autoUpdateTimer != null && _autoUpdateTimer.IsEnabled)
            {
                _autoUpdateTimer.Stop();
                IsAutoUpdateEnabled = false;
                UpdateAutoUpdateStatus();
            }
        }

        private async void AutoUpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (Monitor.TryEnter(_updateLock))
            {
                try
                {
                    await PerformAutoUpdate();
                }
                finally
                {
                    Monitor.Exit(_updateLock);
                }
            }
        }

        private async Task PerformAutoUpdate()
        {
            try
            {
                AutoUpdateStatus = "تحديث تلقائي جاري...";
                
                if (SelectedUser != null)
                {
                    // Auto-update user permissions
                    await LoadUserPermissionsAsync();
                    
                    // If role is selected, update role permissions too
                    if (SelectedRole != null)
                    {
                        await LoadRolePermissionsForSelectedRoleAsync();
                    }
                }

                LastUpdateTime = DateTime.Now;
                AutoUpdateStatus = $"آخر تحديث: {LastUpdateTime:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                AutoUpdateStatus = $"خطأ في التحديث التلقائي: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task ForceUpdate()
        {
            await PerformAutoUpdate();
        }

        private void UpdateAutoUpdateStatus()
        {
            if (IsAutoUpdateEnabled)
            {
                AutoUpdateStatus = $"التحديث التلقائي مفعل - كل {AutoUpdateInterval} ثانية";
            }
            else
            {
                AutoUpdateStatus = "التحديث التلقائي متوقف";
            }
        }

        [RelayCommand]
        private void SetAutoUpdateInterval(string intervalString)
        {
            if (int.TryParse(intervalString, out int interval) && interval >= 5)
            {
                AutoUpdateInterval = interval;
            }
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _autoUpdateTimer?.Stop();
                _autoUpdateTimer = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
