using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Models;
using Next_Future_ERP.Features.Auth.Models;
using Next_Future_ERP.Features.Permissions.Services;
using Next_Future_ERP.Features.InitialSystem.Models;

namespace Next_Future_ERP.Features.Auth.Services
{
    /// <summary>
    /// Service for managing user session data and permissions
    /// </summary>
    public class SessionService : ISessionService
    {
        private readonly AppDbContext _context;
        private readonly IPermissionService _permissionService;
        private SessionUser? _currentUser;

        public SessionUser? CurrentUser => _currentUser;

        public event EventHandler<SessionUser?>? SessionChanged;

        public SessionService(AppDbContext context, IPermissionService permissionService)
        {
            _context = context;
            _permissionService = permissionService;
        }

        public async Task<bool> InitializeSessionAsync(int userId, int? companyId = null, int? branchId = null)
        {
            try
            {
                // Get user data
                var user = await _context.Nextuser.FindAsync(userId);
                if (user == null) return false;

                // Get company and branch info if not provided
                if (!companyId.HasValue || !branchId.HasValue)
                {
                    var companyInfo = await _context.CompanyInfo.FirstOrDefaultAsync();
                    companyId ??= companyInfo?.CompId;
                    branchId ??= 1; // Default branch
                }

                // Get company and branch names
                var company = await _context.CompanyInfo.FindAsync(companyId);
                var branch = await _context.Branches
                    .FirstOrDefaultAsync(b => b.BranchId == branchId);

                // Create session user
                _currentUser = new SessionUser
                {
                    Id = user.ID,
                    Name = user.Name ?? string.Empty,
                    Code = user.Code ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FullName = user.Fname ?? user.Name ?? string.Empty, // Use Fname as FullName
                    LastLogin = user.LastLogin,
                    CompanyId = companyId,
                    BranchId = branchId,
                    CompanyName = company?.CompName,
                    BranchName = branch?.BranchName
                };

                // Load user permissions
                await LoadUserPermissionsAsync();

                // Fire session changed event
                SessionChanged?.Invoke(this, _currentUser);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing session: {ex.Message}");
                return false;
            }
        }

        public void ClearSession()
        {
            _currentUser = null;
            SessionChanged?.Invoke(this, null);
        }

        public async Task<bool> RefreshPermissionsAsync()
        {
            if (_currentUser == null) return false;

            try
            {
                await LoadUserPermissionsAsync();
                SessionChanged?.Invoke(this, _currentUser);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool HasPermission(int formId, string permissionType)
        {
            return _currentUser?.HasPermission(formId, permissionType) ?? false;
        }

        public bool CanAccessForm(int formId)
        {
            return _currentUser?.CanAccessForm(formId) ?? false;
        }

        public List<int> GetAccessibleForms()
        {
            return _currentUser?.GetAccessibleForms() ?? new List<int>();
        }

        private async Task LoadUserPermissionsAsync()
        {
            if (_currentUser == null) return;

            try
            {
                // Load user permissions from permission service
                var permissions = await _permissionService.GetUserPermissionsAsync(
                    _currentUser.Id, 
                    _currentUser.CompanyId ?? 1, 
                    _currentUser.BranchId ?? 1);

                _currentUser.Permissions = permissions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user permissions: {ex.Message}");
                _currentUser.Permissions = new List<Next_Future_ERP.Features.Permissions.Models.UserPermission>();
            }
        }
    }
}
