using Next_Future_ERP.Data.Models;
using Next_Future_ERP.Features.Permissions.Models;

namespace Next_Future_ERP.Features.Auth.Models
{
    /// <summary>
    /// Represents the current logged-in user session data
    /// </summary>
    public class SessionUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }
        public int? CompanyId { get; set; }
        public int? BranchId { get; set; }
        public string? CompanyName { get; set; }
        public string? BranchName { get; set; }
        
        /// <summary>
        /// User permissions for the current session
        /// </summary>
        public List<UserPermission> Permissions { get; set; } = new List<UserPermission>();
        
        /// <summary>
        /// Quick access to check if user has specific permission
        /// </summary>
        /// <param name="formId">Menu form ID</param>
        /// <param name="permissionType">Type of permission (canview, canadd, canedit, candelete, etc.)</param>
        /// <returns>True if user has the permission</returns>
        public bool HasPermission(int formId, string permissionType)
        {
            var permission = Permissions.FirstOrDefault(p => p.FormId == formId);
            if (permission == null) return false;

            return permissionType.ToLower() switch
            {
                "canview" => permission.AllowView,
                "canadd" => permission.AllowAdd,
                "canedit" => permission.AllowEdit,
                "candelete" => permission.AllowDelete,
                "canprint" => permission.AllowPrint,
                "canpost" => permission.AllowPost,
                "canrun" => permission.AllowRun,
                _ => false
            };
        }

        /// <summary>
        /// Check if user can access a specific menu form
        /// </summary>
        /// <param name="formId">Menu form ID</param>
        /// <returns>True if user can view the form</returns>
        public bool CanAccessForm(int formId)
        {
            return HasPermission(formId, "canview");
        }

        /// <summary>
        /// Get all accessible menu forms for the user
        /// </summary>
        /// <returns>List of form IDs the user can access</returns>
        public List<int> GetAccessibleForms()
        {
            return Permissions
                .Where(p => p.AllowView)
                .Select(p => p.FormId)
                .ToList();
        }
    }
}
