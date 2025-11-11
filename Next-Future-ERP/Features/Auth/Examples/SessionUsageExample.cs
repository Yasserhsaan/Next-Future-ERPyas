using Next_Future_ERP.Features.Auth.Extensions;
using Next_Future_ERP.Features.Auth.Services;
using System.Windows;

namespace Next_Future_ERP.Features.Auth.Examples
{
    /// <summary>
    /// Example class showing how to use the session login system
    /// </summary>
    public class SessionUsageExample
    {
        private readonly IAuthService _authService;
        private readonly ISessionService _sessionService;

        public SessionUsageExample(IAuthService authService, ISessionService sessionService)
        {
            _authService = authService;
            _sessionService = sessionService;
        }

        /// <summary>
        /// Example: Login user and get session data
        /// </summary>
        public async Task<bool> LoginUserExample()
        {
            try
            {
                // Login user (this automatically initializes session with permissions)
                var result = await _authService.LoginAsync("username", "password");
                
                if (result.IsSuccess)
                {
                    // Get current user session data
                    var currentUser = _sessionService.CurrentUser;
                    
                    if (currentUser != null)
                    {
                        Console.WriteLine($"✅ User logged in successfully:");
                        Console.WriteLine($"   Name: {currentUser.FullName}");
                        Console.WriteLine($"   Email: {currentUser.Email}");
                        Console.WriteLine($"   Company: {currentUser.CompanyName}");
                        Console.WriteLine($"   Branch: {currentUser.BranchName}");
                        Console.WriteLine($"   Permissions Count: {currentUser.Permissions.Count}");
                        
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine($"❌ Login failed: {result.Message}");
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Login error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Example: Check user permissions
        /// </summary>
        public void CheckPermissionsExample()
        {
            var currentUser = _sessionService.CurrentUser;
            if (currentUser == null)
            {
                Console.WriteLine("❌ No user logged in");
                return;
            }

            // Check specific permission
            bool canViewAccounts = currentUser.HasPermission(1, "canview"); // Form ID 1 = Accounts
            bool canAddAccounts = currentUser.HasPermission(1, "canadd");
            bool canEditAccounts = currentUser.HasPermission(1, "canedit");
            bool canDeleteAccounts = currentUser.HasPermission(1, "candelete");

            Console.WriteLine($"📋 User Permissions for Accounts (Form ID: 1):");
            Console.WriteLine($"   Can View: {canViewAccounts}");
            Console.WriteLine($"   Can Add: {canAddAccounts}");
            Console.WriteLine($"   Can Edit: {canEditAccounts}");
            Console.WriteLine($"   Can Delete: {canDeleteAccounts}");

            // Get all accessible forms
            var accessibleForms = currentUser.GetAccessibleForms();
            Console.WriteLine($"📂 Accessible Forms: {string.Join(", ", accessibleForms)}");
        }

        /// <summary>
        /// Example: Using session extensions (static methods)
        /// </summary>
        public void UseSessionExtensionsExample()
        {
            // Check if user is logged in
            if (!SessionExtensions.IsUserLoggedIn())
            {
                Console.WriteLine("❌ No user logged in");
                return;
            }

            // Get user info using extensions
            var userName = SessionExtensions.GetCurrentUserName();
            var companyName = SessionExtensions.GetCurrentCompanyName();
            var branchName = SessionExtensions.GetCurrentBranchName();

            Console.WriteLine($"👤 Current Session Info:");
            Console.WriteLine($"   User: {userName}");
            Console.WriteLine($"   Company: {companyName}");
            Console.WriteLine($"   Branch: {branchName}");

            // Check permissions using extensions
            bool canAccessInventory = SessionExtensions.CanAccessForm(5); // Form ID 5 = Inventory
            bool canAddItems = SessionExtensions.HasPermission(6, "canadd"); // Form ID 6 = Items
            bool canPostTransactions = SessionExtensions.HasPermission(7, "canpost"); // Form ID 7 = Transactions
            bool canRunReports = SessionExtensions.HasPermission(8, "canrun"); // Form ID 8 = Reports

            Console.WriteLine($"🔐 Permission Checks:");
            Console.WriteLine($"   Can Access Inventory: {canAccessInventory}");
            Console.WriteLine($"   Can Add Items: {canAddItems}");
            Console.WriteLine($"   Can Post Transactions: {canPostTransactions}");
            Console.WriteLine($"   Can Run Reports: {canRunReports}");
        }

        /// <summary>
        /// Example: Logout user
        /// </summary>
        public void LogoutExample()
        {
            try
            {
                _authService.Logout();
                Console.WriteLine("✅ User logged out successfully");
                
                // Verify session is cleared
                var currentUser = _sessionService.CurrentUser;
                Console.WriteLine($"Current user after logout: {(currentUser == null ? "None" : currentUser.Name)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Logout error: {ex.Message}");
            }
        }

        /// <summary>
        /// Example: Handle session events
        /// </summary>
        public void SetupSessionEventsExample()
        {
            // Subscribe to session changes
            _sessionService.SessionChanged += (sender, user) =>
            {
                if (user != null)
                {
                    MessageBox.Show($"User {user.FullName} logged in with {user.Permissions.Count} permissions", 
                                  "Session Started", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("User session ended", "Session Ended", 
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };
        }

        /// <summary>
        /// Example: Refresh permissions during session
        /// </summary>
        public async Task RefreshPermissionsExample()
        {
            if (_sessionService.CurrentUser == null)
            {
                Console.WriteLine("❌ No user logged in");
                return;
            }

            try
            {
                var refreshed = await _sessionService.RefreshPermissionsAsync();
                if (refreshed)
                {
                    var currentUser = _sessionService.CurrentUser;
                    Console.WriteLine($"✅ Permissions refreshed. Current count: {currentUser?.Permissions.Count}");
                }
                else
                {
                    Console.WriteLine("❌ Failed to refresh permissions");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Refresh error: {ex.Message}");
            }
        }
    }
}
