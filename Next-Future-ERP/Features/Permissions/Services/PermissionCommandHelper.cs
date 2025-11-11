using Next_Future_ERP.Features.Permissions.Services;

namespace Next_Future_ERP.Features.Permissions.Services
{
    public static class PermissionCommandHelper
    {
        private static IPermissionService? _permissionService;
        private static int _currentUserId = 1;
        private static int _currentCompanyId = 1;
        private static int _currentBranchId = 1;

        public static void Initialize(IPermissionService permissionService, int userId, int companyId, int branchId)
        {
            _permissionService = permissionService;
            _currentUserId = userId;
            _currentCompanyId = companyId;
            _currentBranchId = branchId;
        }

        public static async Task<bool> CanExecuteAsync(int formId, string permissionType)
        {
            if (_permissionService == null)
            {
                // If no permission service is configured, allow all operations
                return true;
            }

            return await _permissionService.HasPermissionAsync(_currentUserId, formId, permissionType, _currentCompanyId, _currentBranchId);
        }

        public static async Task<bool> CanAddAsync(int formId)
        {
            return await CanExecuteAsync(formId, "add");
        }

        public static async Task<bool> CanEditAsync(int formId)
        {
            return await CanExecuteAsync(formId, "edit");
        }

        public static async Task<bool> CanDeleteAsync(int formId)
        {
            return await CanExecuteAsync(formId, "delete");
        }

        public static async Task<bool> CanViewAsync(int formId)
        {
            return await CanExecuteAsync(formId, "view");
        }

        public static async Task<bool> CanPostAsync(int formId)
        {
            return await CanExecuteAsync(formId, "post");
        }

        public static async Task<bool> CanPrintAsync(int formId)
        {
            return await CanExecuteAsync(formId, "print");
        }

        public static async Task<bool> CanRunAsync(int formId)
        {
            return await CanExecuteAsync(formId, "run");
        }

        public static void UpdateContext(int userId, int companyId, int branchId)
        {
            _currentUserId = userId;
            _currentCompanyId = companyId;
            _currentBranchId = branchId;
        }

        // Predefined form IDs for common operations
        public static class FormIds
        {
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
    }
}
