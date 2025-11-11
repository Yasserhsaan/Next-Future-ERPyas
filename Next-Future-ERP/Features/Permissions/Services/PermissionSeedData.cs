using Next_Future_ERP.Features.Permissions.Models;

namespace Next_Future_ERP.Features.Permissions.Services
{
    public static class PermissionSeedData
    {
        public static List<MenuForm> GetDefaultMenuForms()
        {
            return new List<MenuForm>
            {
                // Main Categories
                new MenuForm { MenuFormCode = 1, MenuFormParent = 0, MenuName = "Dashboard", MenuArabicName = "لوحة التحكم", ProgramExecutable = "DashboardView", Visible = 1,NSync=1 },
                new MenuForm { MenuFormCode = 2, MenuFormParent = 0, MenuName = "Accounts", MenuArabicName = "الحسابات", ProgramExecutable = "AccountsView", Visible = 1,NSync=1 },
                new MenuForm { MenuFormCode = 3, MenuFormParent = 0, MenuName = "Inventory", MenuArabicName = "المخزون", ProgramExecutable = "InventoryView", Visible = 1 ,NSync=1},
                new MenuForm { MenuFormCode = 4, MenuFormParent = 0, MenuName = "Sales", MenuArabicName = "المبيعات", ProgramExecutable = "SalesView", Visible = 1  ,NSync=1 },
                new MenuForm { MenuFormCode = 5, MenuFormParent = 0, MenuName = "Purchases", MenuArabicName = "المشتريات", ProgramExecutable = "PurchasesView", Visible = 1,NSync=1 },
                new MenuForm { MenuFormCode = 6, MenuFormParent = 0, MenuName = "Reports", MenuArabicName = "التقارير", ProgramExecutable = "ReportsView", Visible = 1,NSync=1 },
                new MenuForm { MenuFormCode = 7, MenuFormParent = 0, MenuName = "Settings", MenuArabicName = "الإعدادات", ProgramExecutable = "SettingsView", Visible = 1,NSync=1 },

                // Accounts Sub-Menus
                new MenuForm { MenuFormCode = 21, MenuFormParent = 2, MenuName = "Chart of Accounts", MenuArabicName = "دليل الحسابات", ProgramExecutable = "ChartOfAccountsView", Visible = 1,NSync=1 },
                new MenuForm { MenuFormCode = 22, MenuFormParent = 2, MenuName = "General Journal", MenuArabicName = "الدفتر العام", ProgramExecutable = "GeneralJournalView", Visible = 1,NSync=1 },
                new MenuForm { MenuFormCode = 23, MenuFormParent = 2, MenuName = "Payment Vouchers", MenuArabicName = "أوامر الصرف", ProgramExecutable = "PaymentVouchersView", Visible = 1,NSync=1 },
                new MenuForm { MenuFormCode = 24, MenuFormParent = 2, MenuName = "Receipt Vouchers", MenuArabicName = "أوامر القبض", ProgramExecutable = "ReceiptVouchersView", Visible = 1,NSync=1 },
                new MenuForm { MenuFormCode = 25, MenuFormParent = 2, MenuName = "Currencies", MenuArabicName = "العملات", ProgramExecutable = "CurrenciesView", Visible = 1,NSync=1 },
                new MenuForm { MenuFormCode = 26, MenuFormParent = 2, MenuName = "Banks", MenuArabicName = "البنوك", ProgramExecutable = "BanksView", Visible = 1,NSync=1 },

                // Inventory Sub-Menus
                new MenuForm { MenuFormCode = 31, MenuFormParent = 3, MenuName = "Items", MenuArabicName = "الأصناف", ProgramExecutable = "ItemsView", Visible = 1,NSync=1 },
                new MenuForm { MenuFormCode = 32, MenuFormParent = 3, MenuName = "Categories", MenuArabicName = "الفئات", ProgramExecutable = "CategoriesView", Visible = 1,NSync=1 },
                new MenuForm { MenuFormCode = 33, MenuFormParent = 3, MenuName = "Units", MenuArabicName = "الوحدات", ProgramExecutable = "UnitsView", Visible = 1,NSync=1 },
                new MenuForm { MenuFormCode = 34, MenuFormParent = 3, MenuName = "Warehouses", MenuArabicName = "المخازن", ProgramExecutable = "WarehousesView", Visible = 1,NSync=1 },
                new MenuForm { MenuFormCode = 35, MenuFormParent = 3, MenuName = "Stock Movement", MenuArabicName = "حركة المخزون", ProgramExecutable = "StockMovementView", Visible = 1 , NSync = 1},
                new MenuForm { MenuFormCode = 36, MenuFormParent = 3, MenuName = "Stock Transfer", MenuArabicName = "تحويل المخزون", ProgramExecutable = "StockTransferView", Visible = 1 , NSync = 1},

                // Sales Sub-Menus
                new MenuForm { MenuFormCode = 41, MenuFormParent = 4, MenuName = "Sales Orders", MenuArabicName = "أوامر البيع", ProgramExecutable = "SalesOrdersView", Visible = 1 , NSync = 1},
                new MenuForm { MenuFormCode = 42, MenuFormParent = 4, MenuName = "Sales Invoices", MenuArabicName = "فواتير البيع", ProgramExecutable = "SalesInvoicesView", Visible = 1 , NSync = 1},
                new MenuForm { MenuFormCode = 43, MenuFormParent = 4, MenuName = "Sales Returns", MenuArabicName = "مرتجع البيع", ProgramExecutable = "SalesReturnsView", Visible = 1 , NSync = 1},
                new MenuForm { MenuFormCode = 44, MenuFormParent = 4, MenuName = "Customers", MenuArabicName = "العملاء", ProgramExecutable = "CustomersView", Visible = 1 , NSync = 1},
                new MenuForm { MenuFormCode = 45, MenuFormParent = 4, MenuName = "Sales Reports", MenuArabicName = "تقارير المبيعات", ProgramExecutable = "SalesReportsView", Visible = 1 , NSync = 1},

                // Purchases Sub-Menus
                new MenuForm { MenuFormCode = 51, MenuFormParent = 5, MenuName = "Purchase Orders", MenuArabicName = "أوامر الشراء", ProgramExecutable = "PurchaseOrdersView", Visible = 1 , NSync = 1},
                new MenuForm { MenuFormCode = 52, MenuFormParent = 5, MenuName = "Purchase Invoices", MenuArabicName = "فواتير الشراء", ProgramExecutable = "PurchaseInvoicesView", Visible = 1 , NSync = 1},
                new MenuForm { MenuFormCode = 53, MenuFormParent = 5, MenuName = "Purchase Returns", MenuArabicName = "مرتجع الشراء", ProgramExecutable = "PurchaseReturnsView", Visible = 1 , NSync = 1},
                new MenuForm { MenuFormCode = 54, MenuFormParent = 5, MenuName = "Suppliers", MenuArabicName = "الموردين", ProgramExecutable = "SuppliersView", Visible = 1 , NSync = 1},
                new MenuForm { MenuFormCode = 55, MenuFormParent = 5, MenuName = "Purchase Reports", MenuArabicName = "تقارير المشتريات", ProgramExecutable = "PurchaseReportsView", Visible = 1 , NSync = 1},

                // Reports Sub-Menus
                new MenuForm { MenuFormCode = 61, MenuFormParent = 6, MenuName = "Financial Reports", MenuArabicName = "التقارير المالية", ProgramExecutable = "FinancialReportsView", Visible = 1 , NSync = 1},
                new MenuForm { MenuFormCode = 62, MenuFormParent = 6, MenuName = "Inventory Reports", MenuArabicName = "تقارير المخزون", ProgramExecutable = "InventoryReportsView", Visible = 1 , NSync = 1},
                new MenuForm { MenuFormCode = 63, MenuFormParent = 6, MenuName = "Sales Reports", MenuArabicName = "تقارير المبيعات", ProgramExecutable = "SalesReportsView", Visible = 1 , NSync = 1},
                new MenuForm { MenuFormCode = 64, MenuFormParent = 6, MenuName = "Purchase Reports", MenuArabicName = "تقارير المشتريات", ProgramExecutable = "PurchaseReportsView", Visible = 1 , NSync = 1},

                // Settings Sub-Menus
                new MenuForm { MenuFormCode = 71, MenuFormParent = 7, MenuName = "Company Settings", MenuArabicName = "إعدادات الشركة", ProgramExecutable = "CompanySettingsView", Visible = 1 , NSync = 1},
                new MenuForm { MenuFormCode = 72, MenuFormParent = 7, MenuName = "User Management", MenuArabicName = "إدارة المستخدمين", ProgramExecutable = "UserManagementView", Visible = 1 , NSync = 1},
                new MenuForm { MenuFormCode = 73, MenuFormParent = 7, MenuName = "Role Management", MenuArabicName = "إدارة الأدوار", ProgramExecutable = "RoleManagementView", Visible = 1 , NSync = 1},
                new MenuForm { MenuFormCode = 74, MenuFormParent = 7, MenuName = "System Settings", MenuArabicName = "إعدادات النظام", ProgramExecutable = "SystemSettingsView", Visible = 1 , NSync = 1    }
            };
        }

        public static List<SysRole> GetDefaultRoles()
        {
            return new List<SysRole>
            {
                new SysRole {  Name = "System Administrator", RollType = 1,  },
                new SysRole {   Name = "Account Manager", RollType = 2,  },
                new SysRole { Name = "Storekeeper", RollType = 3 },
                new SysRole { Name = "Sales Clerk", RollType = 3 },
                new SysRole { Name = "Purchase Clerk", RollType = 3 },
                new SysRole { Name = "View Only", RollType = 3 }
            };
        }

        public static List<MenuRole> GetDefaultMenuRoles()
        {
            return new List<MenuRole>
            {
                // System Administrator - Full Access
                new MenuRole { Id = 1, FormId = 1, RoleId = 1 }, // Dashboard
                new MenuRole { Id = 2, FormId = 2, RoleId = 1 }, // Accounts
                new MenuRole { Id = 3, FormId = 3, RoleId = 1 }, // Inventory
                new MenuRole { Id = 4, FormId = 4, RoleId = 1 }, // Sales
                new MenuRole { Id = 5, FormId = 5, RoleId = 1 }, // Purchases
                new MenuRole { Id = 6, FormId = 6, RoleId = 1 }, // Reports
                new MenuRole { Id = 7, FormId = 7, RoleId = 1 }, // Settings

                // Account Manager - Accounts + Reports
                new MenuRole { Id = 8, FormId = 1, RoleId = 2 }, // Dashboard
                new MenuRole { Id = 9, FormId = 2, RoleId = 2 }, // Accounts
                new MenuRole { Id = 10, FormId = 6, RoleId = 2 }, // Reports

                // Storekeeper - Inventory + Reports
                new MenuRole { Id = 11, FormId = 1, RoleId = 3 }, // Dashboard
                new MenuRole { Id = 12, FormId = 3, RoleId = 3 }, // Inventory
                new MenuRole { Id = 13, FormId = 6, RoleId = 3 }, // Reports

                // Sales Clerk - Sales + Reports
                new MenuRole { Id = 14, FormId = 1, RoleId = 4 }, // Dashboard
                new MenuRole { Id = 15, FormId = 4, RoleId = 4 }, // Sales
                new MenuRole { Id = 16, FormId = 6, RoleId = 4 }, // Reports

                // Purchase Clerk - Purchases + Reports
                new MenuRole { Id = 17, FormId = 1, RoleId = 5 }, // Dashboard
                new MenuRole { Id = 18, FormId = 5, RoleId = 5 }, // Purchases
                new MenuRole { Id = 19, FormId = 6, RoleId = 5 }, // Reports

                // View Only - Dashboard + Reports (Read Only)
                new MenuRole { Id = 20, FormId = 1, RoleId = 6 }, // Dashboard
                new MenuRole { Id = 21, FormId = 6, RoleId = 6 }, // Reports
            };
        }

        public static List<UserPermission> GetDefaultUserPermissions()
        {
            return new List<UserPermission>
            {
                // System Administrator - Full permissions on all forms
                CreateUserPermission(1, 11, 1, 28, 1, true, true, true, true, true, true, true),
                CreateUserPermission(2, 11, 2, 28, 1, true, true, true, true, true, true, true),
                CreateUserPermission(3, 11, 3, 28, 1, true, true, true, true, true, true, true),
                CreateUserPermission(4, 11, 4, 28, 1, true, true, true, true, true, true, true),
                CreateUserPermission(5, 11, 5, 28, 1, true, true, true, true, true, true, true),
                CreateUserPermission(6, 11, 6, 28, 1, true, true, true, true, true, true, true),
                CreateUserPermission(7, 11, 7, 28, 1, true, true, true, true, true, true, true),

                //// Account Manager - Full permissions on accounts, view on others
                //CreateUserPermission(8, 2, 1, 1, 1, true, false, false, true, false, false, false),
                //CreateUserPermission(9, 2, 2, 1, 1, true, true, true, true, true, true, true),
                //CreateUserPermission(10, 2, 6, 1, 1, true, false, false, true, false, true, false),

                //// Storekeeper - Full permissions on inventory, view on others
                //CreateUserPermission(11, 3, 1, 1, 1, true, false, false, true, false, false, false),
                //CreateUserPermission(12, 3, 3, 1, 1, true, true, true, true, true, true, true),
                //CreateUserPermission(13, 3, 6, 1, 1, true, false, false, true, false, true, false),

                //// Sales Clerk - Full permissions on sales, view on others
                //CreateUserPermission(14, 4, 1, 1, 1, true, false, false, true, false, false, false),
                //CreateUserPermission(15, 4, 4, 1, 1, true, true, true, true, true, true, true),
                //CreateUserPermission(16, 4, 6, 1, 1, true, false, false, true, false, true, false),

                //// Purchase Clerk - Full permissions on purchases, view on others
                //CreateUserPermission(17, 5, 1, 1, 1, true, false, false, true, false, false, false),
                //CreateUserPermission(18, 5, 5, 1, 1, true, true, true, true, true, true, true),
                //CreateUserPermission(19, 5, 6, 1, 1, true, false, false, true, false, true, false),

                //// View Only - View permissions only
                //CreateUserPermission(20, 6, 1, 1, 1, false, false, false, true, false, false, false),
                //CreateUserPermission(21, 6, 6, 1, 1, false, false, false, true, false, true, false),
            };
        }

        private static UserPermission CreateUserPermission(int perId, int userId, int formId, int companyId, int branchId, 
            bool allowAdd, bool allowEdit, bool allowDelete, bool allowView, bool allowPost, bool allowPrint, bool allowRun)
        {
            return new UserPermission
            {
                
                UserId = userId,
                FormId = formId,
                CompanyId = companyId,
                BranchId = branchId,
                RoleId = userId, // Using userId as roleId for simplicity
                AllowAdd = allowAdd,
                AllowEdit = allowEdit,
                AllowDelete = allowDelete,
                AllowView = allowView,
                AllowPost = allowPost,
                AllowPrint = allowPrint,
                AllowRun = allowRun
            };
        }
    }
}
