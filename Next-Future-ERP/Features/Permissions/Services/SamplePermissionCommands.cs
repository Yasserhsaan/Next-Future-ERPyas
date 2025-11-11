using Next_Future_ERP.Features.Permissions.Services;
using System.Windows;

namespace Next_Future_ERP.Features.Permissions.Services
{
    public static class SamplePermissionCommands
    {
        // Example of how to integrate permission checking into existing commands
        public static class AccountsCommands
        {
            public static async Task AddAccountAsync()
            {
                // Check if user can add accounts
                if (!await PermissionCommandHelper.CanAddAsync(PermissionCommandHelper.FormIds.ChartOfAccounts))
                {
                    MessageBox.Show("❌ ليس لديك صلاحية لإضافة حسابات جديدة", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Proceed with account addition logic
                MessageBox.Show("✅ يمكنك إضافة حساب جديد", "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            public static async Task EditAccountAsync()
            {
                // Check if user can edit accounts
                if (!await PermissionCommandHelper.CanEditAsync(PermissionCommandHelper.FormIds.ChartOfAccounts))
                {
                    MessageBox.Show("❌ ليس لديك صلاحية لتعديل الحسابات", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Proceed with account editing logic
                MessageBox.Show("✅ يمكنك تعديل الحساب", "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            public static async Task DeleteAccountAsync()
            {
                // Check if user can delete accounts
                if (!await PermissionCommandHelper.CanDeleteAsync(PermissionCommandHelper.FormIds.ChartOfAccounts))
                {
                    MessageBox.Show("❌ ليس لديك صلاحية لحذف الحسابات", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Proceed with account deletion logic
                MessageBox.Show("✅ يمكنك حذف الحساب", "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            public static async Task PostJournalEntryAsync()
            {
                // Check if user can post journal entries
                if (!await PermissionCommandHelper.CanPostAsync(PermissionCommandHelper.FormIds.GeneralJournal))
                {
                    MessageBox.Show("❌ ليس لديك صلاحية لترحيل القيود المحاسبية", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Proceed with posting logic
                MessageBox.Show("✅ يمكنك ترحيل القيد المحاسبي", "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public static class InventoryCommands
        {
            public static async Task AddItemAsync()
            {
                if (!await PermissionCommandHelper.CanAddAsync(PermissionCommandHelper.FormIds.Items))
                {
                    MessageBox.Show("❌ ليس لديك صلاحية لإضافة أصناف جديدة", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show("✅ يمكنك إضافة صنف جديد", "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            public static async Task TransferStockAsync()
            {
                if (!await PermissionCommandHelper.CanRunAsync(PermissionCommandHelper.FormIds.StockTransfer))
                {
                    MessageBox.Show("❌ ليس لديك صلاحية لتحويل المخزون", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show("✅ يمكنك تحويل المخزون", "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public static class SalesCommands
        {
            public static async Task CreateSalesInvoiceAsync()
            {
                if (!await PermissionCommandHelper.CanAddAsync(PermissionCommandHelper.FormIds.SalesInvoices))
                {
                    MessageBox.Show("❌ ليس لديك صلاحية لإنشاء فواتير البيع", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show("✅ يمكنك إنشاء فاتورة بيع جديدة", "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            public static async Task PrintSalesInvoiceAsync()
            {
                if (!await PermissionCommandHelper.CanPrintAsync(PermissionCommandHelper.FormIds.SalesInvoices))
                {
                    MessageBox.Show("❌ ليس لديك صلاحية لطباعة فواتير البيع", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show("✅ يمكنك طباعة فاتورة البيع", "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public static class ReportsCommands
        {
            public static async Task ViewFinancialReportsAsync()
            {
                if (!await PermissionCommandHelper.CanViewAsync(PermissionCommandHelper.FormIds.Reports))
                {
                    MessageBox.Show("❌ ليس لديك صلاحية لعرض التقارير المالية", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show("✅ يمكنك عرض التقارير المالية", "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            public static async Task PrintReportsAsync()
            {
                if (!await PermissionCommandHelper.CanPrintAsync(PermissionCommandHelper.FormIds.Reports))
                {
                    MessageBox.Show("❌ ليس لديك صلاحية لطباعة التقارير", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show("✅ يمكنك طباعة التقارير", "معلومات", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
