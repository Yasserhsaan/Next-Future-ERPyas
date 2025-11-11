using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.Permissions.Models;
using System.Windows;

namespace Next_Future_ERP.Features.Permissions.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;

        public PermissionService()
        {
            _context = DbContextFactory.Create();
        }

        #region Menu Forms

        public async Task<List<MenuForm>> GetAllMenuFormsAsync()
        {
            try
            {
                return await _context.MenuForms
                    .AsNoTracking()
                    .OrderBy(m => m.MenuFormCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
               // MessageBox.Show($"❌ خطأ أثناء جلب قوائم النظام:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<MenuForm>();
            }
        }

        public async Task<List<MenuForm>> GetMenuFormsTreeAsync()
        {
            try
            {
                var menuForms = await _context.MenuForms
                    .AsNoTracking()
                    .OrderBy(m => m.MenuFormCode)
                    .ToListAsync();

                var lookup = menuForms.ToLookup(m => m.MenuFormParent);

                foreach (var menu in menuForms)
                    menu.Children = lookup[menu.MenuFormCode].ToList();

                return menuForms.Where(m => m.MenuFormParent == null).ToList();
            }
            catch (Exception ex)
            {
              //  MessageBox.Show($"❌ خطأ أثناء جلب شجرة القوائم:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<MenuForm>();
            }
        }

        public async Task<MenuForm?> GetMenuFormByIdAsync(int id)
        {
            try
            {
                return await _context.MenuForms
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.MenuFormCode == id);
            }
            catch (Exception ex)
            {
              //  MessageBox.Show($"❌ خطأ أثناء جلب القائمة:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<bool> AddMenuFormAsync(MenuForm menuForm)
        {
            try
            {
                //menuForm.DbTimestamp = DateTime.Now;
                _context.MenuForms.Add(menuForm);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء إضافة القائمة:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UpdateMenuFormAsync(MenuForm menuForm)
        {
            try
            {
                var existing = await _context.MenuForms.FindAsync(menuForm.MenuFormCode);
                if (existing == null) return false;

                existing.MenuName = menuForm.MenuName;
                existing.MenuArabicName = menuForm.MenuArabicName;
                existing.MenuFormParent = menuForm.MenuFormParent;
                existing.ProgramExecutable = menuForm.ProgramExecutable;
                existing.Visible = menuForm.Visible;
               

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء تحديث القائمة:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteMenuFormAsync(int id)
        {
            try
            {
                var menuForm = await _context.MenuForms.FindAsync(id);
                if (menuForm == null) return false;

                _context.MenuForms.Remove(menuForm);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء حذف القائمة:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        #endregion

        #region Users

        public async Task<List<Next_Future_ERP.Data.Models.Nextuser>> GetAllUsersAsync()
        {
            try
            {
                return await _context.Nextuser
                    .AsNoTracking()
                    .OrderBy(u => u.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
               // MessageBox.Show($"❌ خطأ أثناء جلب المستخدمين:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Next_Future_ERP.Data.Models.Nextuser>();
            }
        }

        #endregion

        #region Roles

        public async Task<List<SysRole>> GetAllRolesAsync()
        {
            try
            {
                return await _context.SysRoles
                    .AsNoTracking()
                    .OrderBy(r => r.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
              //  MessageBox.Show($"❌ خطأ أثناء جلب الأدوار:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<SysRole>();
            }
        }

        public async Task<SysRole?> GetRoleByIdAsync(int id)
        {
            try
            {
                return await _context.SysRoles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
              //  MessageBox.Show($"❌ خطأ أثناء جلب الدور:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<bool> AddRoleAsync(SysRole role)
        {
            try
            {
                // Create new role ensuring Id is 0 for identity column
                var newRole = new SysRole
                {
                    Id = 0, // Explicitly set to 0 for identity column
                    Name = role.Name,
                    RollType = role.RollType
                };
                
                _context.SysRoles.Add(newRole);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء إضافة الدور:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UpdateRoleAsync(SysRole role)
        {
            try
            {
                var existing = await _context.SysRoles.FindAsync(role.Id);
                if (existing == null) return false;

                existing.Name = role.Name;
                existing.RollType = role.RollType;
               
                //existing.DbTimestamp = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء تحديث الدور:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            try
            {
                var role = await _context.SysRoles.FindAsync(id);
                if (role == null) return false;

                _context.SysRoles.Remove(role);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء حذف الدور:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        #endregion

        #region Menu-Role Links

        public async Task<List<MenuRole>> GetMenuRolesByRoleIdAsync(int roleId)
        {
            try
            {
                return await _context.MenuRoles
                    .Include(mr => mr.MenuForm)
                    .AsNoTracking()
                    .Where(mr => mr.RoleId == roleId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء جلب صلاحيات الدور:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<MenuRole>();
            }
        }

        public async Task<bool> AssignMenuToRoleAsync(int menuId, int roleId)
        {
            try
            {
                var existing = await _context.MenuRoles
                    .FirstOrDefaultAsync(mr => mr.FormId == menuId && mr.RoleId == roleId);

                if (existing != null) return true; // Already assigned

                var menuRole = new MenuRole
                {
                    Id = 0, // Explicitly set to 0 for identity column
                    FormId = menuId,
                    RoleId = roleId,
                    //DbTimestamp = DateTime.Now
                };

                _context.MenuRoles.Add(menuRole);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء تعيين القائمة للدور:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> RemoveMenuFromRoleAsync(int menuId, int roleId)
        {
            try
            {
                var menuRole = await _context.MenuRoles
                    .FirstOrDefaultAsync(mr => mr.FormId == menuId && mr.RoleId == roleId);

                if (menuRole == null) return true; // Already removed

                _context.MenuRoles.Remove(menuRole);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء إزالة القائمة من الدور:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        #endregion

        #region User Permissions

        public async Task<List<UserPermission>> GetUserPermissionsAsync(int userId, int companyId, int branchId)
        {
            try
            {
                return await _context.UserPermissions
                    .Include(up => up.MenuForm)
                    .Include(up => up.SysRole)
                    .AsNoTracking()
                    .Where(up => up.UserId == userId && up.CompanyId == companyId && up.BranchId == branchId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
               // MessageBox.Show($"❌ خطأ أثناء جلب صلاحيات المستخدم:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<UserPermission>();
            }
        }

        public async Task<List<UserPermission>> GetRolePermissionsAsync(int roleId, int companyId, int branchId,int userId)
        {
            try
            {
                var existingCompany = await _context.CompanyInfo.FirstOrDefaultAsync();
                companyId = existingCompany!.CompId;
                return await _context.UserPermissions
                    .Include(up => up.MenuForm)
                    .Include(up => up.SysRole)
                    .AsNoTracking()
                    .Where(up => up.RoleId == roleId && up.CompanyId == companyId && up.BranchId == branchId && up.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
               // MessageBox.Show($"❌ خطأ أثناء جلب صلاحيات الدور:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<UserPermission>();
            }
        }

        public async Task<UserPermission?> GetUserPermissionAsync(int userId, int formId, int companyId, int branchId)
        {
            try
            {
                return await _context.UserPermissions
                    .Include(up => up.MenuForm)
                    .Include(up => up.SysRole)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(up => up.UserId == userId && up.FormId == formId && 
                                             up.CompanyId == companyId && up.BranchId == branchId);
            }
            catch (Exception ex)
            {
               // MessageBox.Show($"❌ خطأ أثناء جلب صلاحية المستخدم:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<bool> SaveUserPermissionAsync(UserPermission permission)
        {
            try
            {
                var existingCompany = await _context.CompanyInfo.FirstOrDefaultAsync();
                var existingBranch = await _context.Branches.FirstOrDefaultAsync();
                permission.CompanyId = existingCompany!.CompId;
                permission.BranchId = existingBranch!.BranchId;

                var existing = await _context.UserPermissions
                    .FirstOrDefaultAsync(up => up.UserId == permission.UserId && up.FormId == permission.FormId &&
                                             up.CompanyId == permission.CompanyId && up.BranchId == permission.BranchId &&
                                             up.RoleId == permission.RoleId);

                if (existing != null)
                {
                    // Update existing
                    existing.AllowAdd = permission.AllowAdd;
                    existing.AllowEdit = permission.AllowEdit;
                    existing.AllowDelete = permission.AllowDelete;
                    existing.AllowView = permission.AllowView;
                    existing.AllowPost = permission.AllowPost;
                    existing.AllowPrint = permission.AllowPrint;
                    existing.AllowRun = permission.AllowRun;
                }
                else
                {
                    // Create new permission - ensure PerId is 0 for identity column
                    var newPermission = new UserPermission
                    {
                        PerId = 0, // Explicitly set to 0 for identity column
                        UserId = permission.UserId,
                        FormId = permission.FormId,
                        RoleId = permission.RoleId,
                        CompanyId = permission.CompanyId,
                        BranchId = permission.BranchId,
                        AllowAdd = permission.AllowAdd,
                        AllowEdit = permission.AllowEdit,
                        AllowDelete = permission.AllowDelete,
                        AllowView = permission.AllowView,
                        AllowPost = permission.AllowPost,
                        AllowPrint = permission.AllowPrint,
                        AllowRun = permission.AllowRun
                    };
                    _context.UserPermissions.Add(newPermission);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
               // MessageBox.Show($"❌ خطأ أثناء حفظ صلاحيات المستخدم:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteUserPermissionAsync(int perId)
        {
            try
            {
                var permission = await _context.UserPermissions.FindAsync(perId);
                if (permission == null) return false;

                _context.UserPermissions.Remove(permission);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء حذف صلاحية المستخدم:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        #endregion

        #region Permission Checking

        public async Task<bool> HasPermissionAsync(int userId, int formId, string permissionType, int companyId, int branchId)
        {
            try
            {
                var permission = await GetUserPermissionAsync(userId, formId, companyId, branchId);
                if (permission == null) return false;

                return permissionType.ToLower() switch
                {
                    "add" => permission.AllowAdd,
                    "edit" => permission.AllowEdit,
                    "delete" => permission.AllowDelete,
                    "view" => permission.AllowView,
                    "post" => permission.AllowPost,
                    "print" => permission.AllowPrint,
                    "run" => permission.AllowRun,
                    _ => false
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء التحقق من الصلاحية:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<List<MenuTreeItem>> GetUserMenuTreeAsync(int userId, int companyId, int branchId)
        {
            try
            {
                var menuForms = await GetMenuFormsTreeAsync();
                var userPermissions = await GetUserPermissionsAsync(userId, companyId, branchId);
                var permissionLookup = userPermissions.ToLookup(up => up.FormId);

                var result = new List<MenuTreeItem>();

                foreach (var menuForm in menuForms)
                {
                    var treeItem = CreateMenuTreeItem(menuForm, permissionLookup, userId, companyId, branchId);
                    result.Add(treeItem);
                }

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء جلب شجرة القوائم للمستخدم:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<MenuTreeItem>();
            }
        }

        private MenuTreeItem CreateMenuTreeItem(MenuForm menuForm, ILookup<int, UserPermission> permissionLookup, 
            int userId, int companyId, int branchId)
        {
            var permissions = permissionLookup[menuForm.MenuFormCode].FirstOrDefault();
            
            var treeItem = new MenuTreeItem
            {
                MenuForm = menuForm,
                CanAdd = permissions?.AllowAdd ?? false,
                CanEdit = permissions?.AllowEdit ?? false,
                CanDelete = permissions?.AllowDelete ?? false,
                CanView = permissions?.AllowView ?? false,
                CanPost = permissions?.AllowPost ?? false,
                CanPrint = permissions?.AllowPrint ?? false,
                CanRun = permissions?.AllowRun ?? false
            };

            foreach (var child in menuForm.Children)
            {
                var childTreeItem = CreateMenuTreeItem(child, permissionLookup, userId, companyId, branchId);
                treeItem.Children.Add(childTreeItem);
            }

            return treeItem;
        }

        #endregion

        #region Bulk Operations

        public async Task<bool> CopyRolePermissionsAsync(int sourceRoleId, int targetRoleId)
        {
            try
            {
                var sourceMenuRoles = await GetMenuRolesByRoleIdAsync(sourceRoleId);
                
                foreach (var menuRole in sourceMenuRoles)
                {
                    await AssignMenuToRoleAsync(menuRole.FormId, targetRoleId);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء نسخ صلاحيات الدور:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> ResetUserPermissionsAsync(int userId, int companyId, int branchId)
        {
            try
            {
                var userPermissions = await GetUserPermissionsAsync(userId, companyId, branchId);
                
                foreach (var permission in userPermissions)
                {
                    await DeleteUserPermissionAsync(permission.PerId);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء إعادة تعيين صلاحيات المستخدم:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        #endregion

        #region Database Seeding

        public async Task<bool> SeedDatabaseAsync()
        {


             // Check if data already exists
                var existingMenuForms = await _context.MenuForms.CountAsync();
                var existingRoles = await _context.SysRoles.CountAsync();
                var existingUserPermissions = await _context.UserPermissions.CountAsync();

                if (existingMenuForms > 0 || existingRoles > 0 || existingUserPermissions > 0)
                {
                    MessageBox.Show("Database already contains permission data. Seeding skipped.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }

                // Seed Menu Forms
                var menuForms = PermissionSeedData.GetDefaultMenuForms();
                foreach (var menuForm in menuForms)
                {
                    //menuForm.DbTimestamp = DateTime.Now;
                    _context.MenuForms.Add(menuForm);
                // Save changes to get the IDs
                await _context.SaveChangesAsync();
            }

                // Seed Roles
                var roles = PermissionSeedData.GetDefaultRoles();
                foreach (var role in roles)
                {
                    //role.DbTimestamp = DateTime.Now;
                    _context.SysRoles.Add(role);
                // Save changes to get the IDs
                await _context.SaveChangesAsync();
            }

                //// Save changes to get the IDs
                //await _context.SaveChangesAsync();

                // Seed Menu Roles
                var menuRoles = PermissionSeedData.GetDefaultMenuRoles();
                foreach (var menuRole in menuRoles)
                {
                    //menuRole.DbTimestamp = DateTime.Now;
                    _context.MenuRoles.Add(menuRole);
                await _context.SaveChangesAsync();
            }

                // Seed User Permissions
                var userPermissions = PermissionSeedData.GetDefaultUserPermissions();
                foreach (var permission in userPermissions)
                {
                    _context.UserPermissions.Add(permission);
                await _context.SaveChangesAsync();
            }

              

                MessageBox.Show($"Database seeded successfully!\n- {menuForms.Count} Menu Forms\n- {roles.Count} Roles\n- {menuRoles.Count} Menu Roles\n- {userPermissions.Count} User Permissions", 
                    "Seeding Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                
                return true;


            //try
            //{
               
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"❌ خطأ أثناء تهيئة قاعدة البيانات:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return false;
            //}
        }

        public async Task<bool> ClearAllPermissionDataAsync()
        {
            try
            {
                var result = MessageBox.Show("هل أنت متأكد من حذف جميع بيانات الصلاحيات؟ هذا الإجراء لا يمكن التراجع عنه.", 
                    "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result != MessageBoxResult.Yes)
                    return false;

                // Clear in correct order due to foreign key constraints
                _context.UserPermissions.RemoveRange(_context.UserPermissions);
                _context.MenuRoles.RemoveRange(_context.MenuRoles);
                _context.MenuForms.RemoveRange(_context.MenuForms);
                _context.SysRoles.RemoveRange(_context.SysRoles);

                await _context.SaveChangesAsync();

                MessageBox.Show("تم حذف جميع بيانات الصلاحيات بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء حذف بيانات الصلاحيات:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        #endregion
    }
}
