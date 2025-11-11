using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Auth.Models;
using Next_Future_ERP.Features.Auth.Services;
using System.Windows;

namespace Next_Future_ERP.Features.Auth.Extensions
{
    /// <summary>
    /// Extension methods for easy access to session functionality
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>
        /// Gets the current session service from DI container
        /// </summary>
        public static ISessionService? GetSessionService()
        {
            try
            {
                // Access the static ServiceProvider from App
                return App.ServiceProvider?.GetService<ISessionService>();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the current logged-in user
        /// </summary>
        public static SessionUser? GetCurrentUser()
        {
            return GetSessionService()?.CurrentUser;
        }

        /// <summary>
        /// Checks if user has permission for a specific form and action
        /// </summary>
        /// <param name="formId">Menu form ID</param>
        /// <param name="permissionType">Permission type (canview, canadd, canedit, candelete, etc.)</param>
        /// <returns>True if user has permission</returns>
        public static bool HasPermission(int formId, string permissionType)
        {
            return GetSessionService()?.HasPermission(formId, permissionType) ?? false;
        }

        /// <summary>
        /// Checks if user can access a specific form
        /// </summary>
        /// <param name="formId">Menu form ID</param>
        /// <returns>True if user can access the form</returns>
        public static bool CanAccessForm(int formId)
        {
            return GetSessionService()?.CanAccessForm(formId) ?? false;
        }

        /// <summary>
        /// Gets all accessible forms for current user
        /// </summary>
        /// <returns>List of accessible form IDs</returns>
        public static List<int> GetAccessibleForms()
        {
            return GetSessionService()?.GetAccessibleForms() ?? new List<int>();
        }

        /// <summary>
        /// Checks if a user is currently logged in
        /// </summary>
        /// <returns>True if user is logged in</returns>
        public static bool IsUserLoggedIn()
        {
            return GetCurrentUser() != null;
        }

        /// <summary>
        /// Gets the current user's full name
        /// </summary>
        /// <returns>User's full name or empty string</returns>
        public static string GetCurrentUserName()
        {
            var user = GetCurrentUser();
            return user?.FullName ?? user?.Name ?? string.Empty;
        }

        /// <summary>
        /// Gets the current company name
        /// </summary>
        /// <returns>Company name or empty string</returns>
        public static string GetCurrentCompanyName()
        {
            return GetCurrentUser()?.CompanyName ?? string.Empty;
        }

        /// <summary>
        /// Gets the current branch name
        /// </summary>
        /// <returns>Branch name or empty string</returns>
        public static string GetCurrentBranchName()
        {
            return GetCurrentUser()?.BranchName ?? string.Empty;
        }
    }
}
