# ERP Permissions System

A comprehensive role-based access control (RBAC) system for the Next Future ERP application, built with WPF C# using MVVM pattern and CommunityToolkit.Mvvm.

## Features

- **Hierarchical Menu System**: Dynamic menu construction based on user permissions
- **Role-Based Access Control**: Granular permissions for different user roles
- **Permission Types**: Add, Edit, Delete, View, Post, Print, Run permissions
- **Multi-Company/Multi-Branch Support**: Company and branch-specific permissions
- **Modern WPF UI**: Built with WPF-UI for Fluent Design
- **Bilingual Support**: Arabic and English interface
- **Real-time Permission Checking**: Verify permissions before command execution

## Database Schema

### Tables

1. **MenwFomrs** - Hierarchical menu structure
2. **SYSROLLS** - User roles and types
3. **MenwRolls** - Role-menu assignments
4. **UsersPermissions** - Per-user permission flags

### Key Relationships

- Menu forms can have parent-child relationships
- Roles are assigned to menu forms
- Users have specific permissions for each form
- Permissions are scoped by company and branch

## Architecture

### Models

- `MenuForm`: Represents menu items with hierarchical structure
- `SysRole`: User roles (Administrator, Accountant, User, etc.)
- `MenuRole`: Links roles to menu forms
- `UserPermission`: Granular permissions for each user-form combination
- `MenuTreeItem`: Helper model for UI tree display

### Services

- `IPermissionService`: Main service interface
- `PermissionService`: Implementation with full CRUD operations
- `PermissionCommandHelper`: Static helper for permission checking
- `PermissionSeedData`: Sample data for testing

### ViewModels

- `PermissionsMainViewModel`: Main permissions management view
- `MenuEditorViewModel`: Menu form management
- Following MVVM pattern with CommunityToolkit.Mvvm

### Views

- `PermissionsMainView`: Main permissions management interface
- Modern WPF-UI design with RTL support
- Tree view for menu navigation
- Data grids for role and permission management

## Setup Instructions

### 1. Database Migration

```bash
# Add migration for permissions tables
dotnet ef migrations add AddPermissionsSystem

# Update database
dotnet ef database update
```

### 2. Seed Data

The system includes sample data for:
- Menu forms (Dashboard, Accounts, Inventory, Sales, Purchases, Reports, Settings)
- Roles (System Administrator, Account Manager, Storekeeper, Sales Clerk, Purchase Clerk, View Only)
- Sample permissions for each role

### 3. Service Registration

Services are automatically registered in `App.xaml.cs`:

```csharp
// Permissions System
services.AddTransient<IPermissionService, PermissionService>();
services.AddTransient<PermissionsMainViewModel>();
services.AddTransient<MenuEditorViewModel>();
services.AddTransient<PermissionsMainView>();
```

### 4. Initialize Permission System

In your application startup:

```csharp
// Initialize permission helper with current user context
PermissionCommandHelper.Initialize(
    permissionService, 
    currentUserId, 
    currentCompanyId, 
    currentBranchId
);
```

## Usage Examples

### 1. Permission Checking in Commands

```csharp
[RelayCommand]
public async Task AddAccountAsync()
{
    // Check if user can add accounts
    if (!await PermissionCommandHelper.CanAddAsync(PermissionCommandHelper.FormIds.ChartOfAccounts))
    {
        MessageBox.Show("❌ ليس لديك صلاحية لإضافة حسابات جديدة", "تنبيه");
        return;
    }

    // Proceed with account addition logic
    // ...
}
```

### 2. Dynamic Menu Construction

```csharp
// Get user's menu tree with permissions
var userMenuTree = await _permissionService.GetUserMenuTreeAsync(
    userId, companyId, branchId
);

// Display only accessible menu items
foreach (var menuItem in userMenuTree)
{
    if (menuItem.HasAnyPermission)
    {
        // Add to navigation menu
    }
}
```

### 3. Real-time Permission Verification

```csharp
// Check specific permission before operation
if (await PermissionCommandHelper.CanPostAsync(PermissionCommandHelper.FormIds.GeneralJournal))
{
    // Allow posting journal entry
    await PostJournalEntry();
}
else
{
    // Show permission denied message
    ShowPermissionDeniedMessage();
}
```

## Sample Roles and Permissions

### System Administrator
- **Full Access**: All forms with all permissions
- **Role Type**: Administrator (1)

### Account Manager
- **Accounts Module**: Full permissions (Add, Edit, Delete, View, Post, Print, Run)
- **Reports Module**: View and Print permissions
- **Dashboard**: View permission
- **Role Type**: Accountant (2)

### Storekeeper
- **Inventory Module**: Full permissions
- **Reports Module**: View and Print permissions
- **Dashboard**: View permission
- **Role Type**: User (3)

### Sales Clerk
- **Sales Module**: Full permissions
- **Reports Module**: View and Print permissions
- **Dashboard**: View permission
- **Role Type**: User (3)

### Purchase Clerk
- **Purchases Module**: Full permissions
- **Reports Module**: View and Print permissions
- **Dashboard**: View permission
- **Role Type**: User (3)

### View Only
- **Dashboard**: View permission
- **Reports Module**: View and Print permissions
- **Role Type**: User (3)

## Form IDs Reference

```csharp
public static class FormIds
{
    // Main modules
    public const int Dashboard = 1;
    public const int Accounts = 2;
    public const int Inventory = 3;
    public const int Sales = 4;
    public const int Purchases = 5;
    public const int Reports = 6;
    public const int Settings = 7;
    
    // Accounts sub-forms
    public const int ChartOfAccounts = 21;
    public const int GeneralJournal = 22;
    public const int PaymentVouchers = 23;
    public const int ReceiptVouchers = 24;
    public const int Currencies = 25;
    public const int Banks = 26;
    
    // Inventory sub-forms
    public const int Items = 31;
    public const int Categories = 32;
    public const int Units = 33;
    public const int Warehouses = 34;
    public const int StockMovement = 35;
    public const int StockTransfer = 36;
    
    // Sales sub-forms
    public const int SalesOrders = 41;
    public const int SalesInvoices = 42;
    public const int SalesReturns = 43;
    public const int Customers = 44;
    public const int SalesReports = 45;
    
    // Purchases sub-forms
    public const int PurchaseOrders = 51;
    public const int PurchaseInvoices = 52;
    public const int PurchaseReturns = 53;
    public const int Suppliers = 54;
    public const int PurchaseReports = 55;
}
```

## Permission Types

- **Add**: Create new records
- **Edit**: Modify existing records
- **Delete**: Remove records
- **View**: Read/display records
- **Post**: Approve/commit transactions
- **Print**: Generate reports and print documents
- **Run**: Execute operations and processes

## Best Practices

1. **Always Check Permissions**: Verify permissions before executing any operation
2. **Use Form IDs**: Use predefined form IDs for consistency
3. **Handle Permission Denials**: Provide clear feedback when permissions are denied
4. **Update Context**: Update user context when switching users or companies
5. **Cache Permissions**: Consider caching user permissions for performance
6. **Audit Trail**: Log permission checks for security auditing

## Security Considerations

- Permissions are checked at the application level
- Database constraints prevent unauthorized access
- User context is maintained throughout the session
- Permission changes require appropriate role access
- All permission operations are logged for audit purposes

## Troubleshooting

### Common Issues

1. **Permission Denied Errors**: Check if user has appropriate role and permissions
2. **Menu Items Not Showing**: Verify menu visibility and user permissions
3. **Database Connection Issues**: Ensure proper connection string and migrations
4. **UI Not Updating**: Check if ViewModels are properly bound to Views

### Debug Tips

- Use `PermissionCommandHelper.CanExecuteAsync()` for detailed permission checking
- Check user context with `PermissionCommandHelper.UpdateContext()`
- Verify seed data is properly loaded
- Test with different user roles and permissions

## Future Enhancements

- **Permission Templates**: Predefined permission sets for common roles
- **Time-based Permissions**: Temporary permissions with expiration
- **Permission Delegation**: Allow users to delegate permissions temporarily
- **Advanced Audit**: Detailed logging of all permission-related activities
- **API Integration**: REST API for external permission management
- **Mobile Support**: Permission system for mobile applications
