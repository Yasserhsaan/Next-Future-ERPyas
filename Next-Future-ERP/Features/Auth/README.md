# Authentication & Session Management

This module provides a comprehensive authentication and session management system for the Next Future ERP application.

## Features

### 🔐 Authentication
- User login with username/password
- Support for multiple login identifiers (Name, Code, Email)
- Password hashing and verification
- Session initialization with user permissions

### 👤 Session Management
- Current user session data
- User permissions management
- Company and branch context
- Session events and notifications
- Permission checking utilities

### 🛡️ Security
- Password hashing with salt
- Session-based permission checking
- Secure logout functionality

## Components

### Models
- **`SessionUser`**: Represents the current logged-in user with permissions
- **`UserModel`**: Basic user model for UI binding
- **`AuthResult`**: Authentication result wrapper

### Services
- **`IAuthService`** / **`AuthService`**: Handles user authentication and logout
- **`ISessionService`** / **`SessionService`**: Manages user session and permissions

### Extensions
- **`SessionExtensions`**: Static helper methods for easy session access

## Usage Examples

### 1. Basic Login
```csharp
// Inject services
private readonly IAuthService _authService;
private readonly ISessionService _sessionService;

// Login user
var result = await _authService.LoginAsync("username", "password");
if (result.IsSuccess)
{
    // Session is automatically initialized with user data and permissions
    var currentUser = _sessionService.CurrentUser;
    Console.WriteLine($"Welcome {currentUser?.FullName}!");
}
```

### 2. Check Permissions
```csharp
// Using session service
var canView = _sessionService.HasPermission(formId: 1, "canview");
var canEdit = _sessionService.HasPermission(formId: 1, "canedit");

// Using session user directly
var currentUser = _sessionService.CurrentUser;
var canDelete = currentUser?.HasPermission(formId: 1, "candelete") ?? false;

// Using static extensions
var canAccess = SessionExtensions.CanAccessForm(formId: 1);
```

### 3. Get Session Information
```csharp
// Current user info
var user = SessionExtensions.GetCurrentUser();
var userName = SessionExtensions.GetCurrentUserName();
var companyName = SessionExtensions.GetCurrentCompanyName();
var branchName = SessionExtensions.GetCurrentBranchName();

// Check if logged in
if (SessionExtensions.IsUserLoggedIn())
{
    // User is logged in
}
```

### 4. Handle Session Events
```csharp
_sessionService.SessionChanged += (sender, user) =>
{
    if (user != null)
    {
        // User logged in
        UpdateUIForLoggedInUser(user);
    }
    else
    {
        // User logged out
        UpdateUIForLoggedOutUser();
    }
};
```

### 5. Logout
```csharp
_authService.Logout(); // Clears session automatically
```

## Permission Types

The system supports the following permission types:
- **`canview`**: Can view/access the form (maps to `AllowView`)
- **`canadd`**: Can add new records (maps to `AllowAdd`)
- **`canedit`**: Can edit existing records (maps to `AllowEdit`)
- **`candelete`**: Can delete records (maps to `AllowDelete`)
- **`canprint`**: Can print reports (maps to `AllowPrint`)
- **`canpost`**: Can post transactions (maps to `AllowPost`)
- **`canrun`**: Can run operations (maps to `AllowRun`)

## Session Data Structure

```csharp
public class SessionUser
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public DateTime? LastLogin { get; set; }
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
    public string? CompanyName { get; set; }
    public string? BranchName { get; set; }
    public List<UserPermission> Permissions { get; set; }
    
    // Helper methods
    public bool HasPermission(int formId, string permissionType);
    public bool CanAccessForm(int formId);
    public List<int> GetAccessibleForms();
}
```

## Dependency Injection Setup

Make sure to register the services in your DI container:

```csharp
// In your service registration
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<ISessionService, SessionService>();
services.AddScoped<IPermissionService, PermissionService>();
```

## Integration with ViewModels

### LoginViewModel Example
```csharp
public class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly ISessionService _sessionService;

    public LoginViewModel(IAuthService authService, ISessionService sessionService)
    {
        _authService = authService;
        _sessionService = sessionService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        var result = await _authService.LoginAsync(Username, Password);
        if (result.IsSuccess)
        {
            // Session initialized, navigate to main window
            NavigateToMainWindow();
        }
    }
}
```

## Best Practices

1. **Always check login status** before accessing user data
2. **Use permission checking** before allowing actions
3. **Handle session events** to update UI appropriately
4. **Refresh permissions** when roles change
5. **Clear session** on logout for security

## Security Considerations

- Passwords are hashed with salt before storage
- Session data is cleared on logout
- Permission checks are enforced at the service level
- User context is maintained throughout the session

## Testing

See `Features/Auth/Examples/SessionUsageExample.cs` for comprehensive usage examples and testing scenarios.
