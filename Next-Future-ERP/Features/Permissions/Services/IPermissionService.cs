using Next_Future_ERP.Features.Permissions.Models;

namespace Next_Future_ERP.Features.Permissions.Services
{
    public interface IPermissionService
    {
        // Menu Forms
        Task<List<MenuForm>> GetAllMenuFormsAsync();
        Task<List<MenuForm>> GetMenuFormsTreeAsync();
        Task<MenuForm?> GetMenuFormByIdAsync(int id);
        Task<bool> AddMenuFormAsync(MenuForm menuForm);
        Task<bool> UpdateMenuFormAsync(MenuForm menuForm);
        Task<bool> DeleteMenuFormAsync(int id);

        // Users
        Task<List<Next_Future_ERP.Data.Models.Nextuser>> GetAllUsersAsync();

        // Roles
        Task<List<SysRole>> GetAllRolesAsync();
        Task<SysRole?> GetRoleByIdAsync(int id);
        Task<bool> AddRoleAsync(SysRole role);
        Task<bool> UpdateRoleAsync(SysRole role);
        Task<bool> DeleteRoleAsync(int id);

        // Menu-Role Links
        Task<List<MenuRole>> GetMenuRolesByRoleIdAsync(int roleId);
        Task<bool> AssignMenuToRoleAsync(int menuId, int roleId);
        Task<bool> RemoveMenuFromRoleAsync(int menuId, int roleId);

        // User Permissions
        Task<List<UserPermission>> GetUserPermissionsAsync(int userId, int companyId, int branchId);
        Task<List<UserPermission>> GetRolePermissionsAsync(int roleId, int companyId, int branchId,int userId);
        Task<UserPermission?> GetUserPermissionAsync(int userId, int formId, int companyId, int branchId);
        Task<bool> SaveUserPermissionAsync(UserPermission permission);
        Task<bool> DeleteUserPermissionAsync(int perId);

        // Permission Checking
        Task<bool> HasPermissionAsync(int userId, int formId, string permissionType, int companyId, int branchId);
        Task<List<MenuTreeItem>> GetUserMenuTreeAsync(int userId, int companyId, int branchId);
        
        // Bulk Operations
        Task<bool> CopyRolePermissionsAsync(int sourceRoleId, int targetRoleId);
        Task<bool> ResetUserPermissionsAsync(int userId, int companyId, int branchId);

        // Database Seeding
        Task<bool> SeedDatabaseAsync();
        Task<bool> ClearAllPermissionDataAsync();
    }
}
