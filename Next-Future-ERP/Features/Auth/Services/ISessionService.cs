using Next_Future_ERP.Features.Auth.Models;

namespace Next_Future_ERP.Features.Auth.Services
{
    /// <summary>
    /// Service interface for managing user session data
    /// </summary>
    public interface ISessionService
    {
        /// <summary>
        /// Gets the current logged-in user
        /// </summary>
        SessionUser? CurrentUser { get; }

        /// <summary>
        /// Initializes a user session after successful login
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="companyId">Company ID (optional)</param>
        /// <param name="branchId">Branch ID (optional)</param>
        /// <returns>True if session initialized successfully</returns>
        Task<bool> InitializeSessionAsync(int userId, int? companyId = null, int? branchId = null);

        /// <summary>
        /// Clears the current user session (logout)
        /// </summary>
        void ClearSession();

        /// <summary>
        /// Refreshes user permissions for the current session
        /// </summary>
        /// <returns>True if permissions refreshed successfully</returns>
        Task<bool> RefreshPermissionsAsync();

        /// <summary>
        /// Checks if user has specific permission
        /// </summary>
        /// <param name="formId">Menu form ID</param>
        /// <param name="permissionType">Permission type</param>
        /// <returns>True if user has permission</returns>
        bool HasPermission(int formId, string permissionType);

        /// <summary>
        /// Checks if user can access a specific form
        /// </summary>
        /// <param name="formId">Menu form ID</param>
        /// <returns>True if user can access the form</returns>
        bool CanAccessForm(int formId);

        /// <summary>
        /// Gets all accessible forms for current user
        /// </summary>
        /// <returns>List of accessible form IDs</returns>
        List<int> GetAccessibleForms();

        /// <summary>
        /// Event fired when user session changes
        /// </summary>
        event EventHandler<SessionUser?>? SessionChanged;
    }
}
