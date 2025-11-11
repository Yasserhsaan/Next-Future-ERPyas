using Next_Future_ERP.Data.Models;

namespace Next_Future_ERP.Features.Auth.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user with username and password and initializes session
        /// </summary>
        /// <param name="username">The username (can be Name, Code, or Email)</param>
        /// <param name="password">The password</param>
        /// <param name="companyId">Optional company ID</param>
        /// <param name="branchId">Optional branch ID</param>
        /// <returns>Authentication result with user information if successful</returns>
        Task<AuthResult> LoginAsync(string username, string password, int? companyId = null, int? branchId = null);

        /// <summary>
        /// Logs out the current user and clears session
        /// </summary>
        void Logout();

        /// <summary>
        /// Validates if a user exists with the given username
        /// </summary>
        /// <param name="username">The username to validate</param>
        /// <returns>True if user exists, false otherwise</returns>
        Task<bool> UserExistsAsync(string username);

        /// <summary>
        /// Updates the last login timestamp for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>True if updated successfully</returns>
        Task<bool> UpdateLastLoginAsync(int userId);

        /// <summary>
        /// Gets user information by ID
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>User information or null if not found</returns>
        Task<Nextuser?> GetUserByIdAsync(int userId);
    }

    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public Nextuser? User { get; set; }
        public string? Token { get; set; } // For future JWT implementation
    }
}
