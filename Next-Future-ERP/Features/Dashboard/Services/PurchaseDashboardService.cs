using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Dashboard.Models;
using Next_Future_ERP.Features.PurchaseInvoices.Models;
using Next_Future_ERP.Features.Purchases.Models;
using Next_Future_ERP.Features.StoreReceipts.Models;
using Next_Future_ERP.Features.Suppliers.Models;
using Next_Future_ERP.Features.Items.Models;

namespace Next_Future_ERP.Features.Dashboard.Services
{
    public interface IPurchaseDashboardService
    {
        Task<PurchaseDashboardData> GetDashboardDataAsync();
        Task<List<RecentItem>> GetRecentItemsAsync(int count = 10);
        Task<List<AlertItem>> GetAlertsAsync();
    }

    public class PurchaseDashboardService : IPurchaseDashboardService
    {
        private readonly AppDbContext _db;

        public PurchaseDashboardService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PurchaseDashboardData> GetDashboardDataAsync()
        {
            try
            {
                var data = new PurchaseDashboardData();

                // إحصائيات أوامر الشراء
                var purchaseOrders = await _db.PurchaseTxns.ToListAsync();
                data.TotalPurchaseOrders = purchaseOrders.Count;
                data.PendingPurchaseOrders = purchaseOrders.Count(x => x.Status == 0 || x.Status == 1);
                data.CompletedPurchaseOrders = purchaseOrders.Count(x => x.Status == 2);
                data.TotalPurchaseAmount = purchaseOrders.Sum(x => x.TotalAmount ?? 0m);
                data.PendingAmount = purchaseOrders.Where(x => x.Status == 0 || x.Status == 1).Sum(x => x.TotalAmount ?? 0m);

            // إحصائيات سندات الاستلام
            var receipts = await _db.StoreReceipts.ToListAsync();
            data.TotalReceipts = receipts.Count;
            data.PendingReceipts = receipts.Count(x => x.Status == 0 || x.Status == 1);
            data.ApprovedReceipts = receipts.Count(x => x.Status == 2);
            data.TotalReceiptAmount = receipts.Sum(x => x.TotalAmount);

            // إحصائيات فواتير المشتريات
            var invoices = await _db.PurchaseAPs.ToListAsync();
            data.TotalInvoices = invoices.Count;
            data.DraftInvoices = invoices.Count(x => x.Status == 0);
            data.PostedInvoices = invoices.Count(x => x.Status == 2);
            data.TotalInvoiceAmount = invoices.Sum(x => x.TotalAmount);
            data.UnpaidAmount = invoices.Where(x => x.Status == 2).Sum(x => x.TotalAmount); // مؤقتاً

            // إحصائيات الموردين
            var suppliers = await _db.Suppliers.ToListAsync();
            data.TotalSuppliers = suppliers.Count;
            data.ActiveSuppliers = suppliers.Count(x => !(x.IsBlacklisted ?? false));
            data.BlacklistedSuppliers = suppliers.Count(x => x.IsBlacklisted ?? false);

            // إحصائيات المخزون (مؤقتة - تحتاج إلى جدول المخزون)
            var items = await _db.Items.ToListAsync();
            data.TotalItems = items.Count;
            data.LowStockItems = 0; // TODO: حساب من جدول المخزون
            data.OutOfStockItems = 0; // TODO: حساب من جدول المخزون
            data.TotalInventoryValue = 0; // TODO: حساب من جدول المخزون

                return data;
            }
            catch (Exception ex)
            {
                // إرجاع بيانات افتراضية في حالة الخطأ
                return new PurchaseDashboardData
                {
                    TotalPurchaseOrders = 0,
                    PendingPurchaseOrders = 0,
                    CompletedPurchaseOrders = 0,
                    TotalPurchaseAmount = 0,
                    PendingAmount = 0,
                    TotalReceipts = 0,
                    PendingReceipts = 0,
                    ApprovedReceipts = 0,
                    TotalReceiptAmount = 0,
                    TotalInvoices = 0,
                    DraftInvoices = 0,
                    PostedInvoices = 0,
                    TotalInvoiceAmount = 0,
                    UnpaidAmount = 0,
                    TotalSuppliers = 0,
                    ActiveSuppliers = 0,
                    BlacklistedSuppliers = 0,
                    TotalItems = 0,
                    LowStockItems = 0,
                    OutOfStockItems = 0,
                    TotalInventoryValue = 0
                };
            }
        }

        public async Task<List<RecentItem>> GetRecentItemsAsync(int count = 10)
        {
            try
            {
                var recentItems = new List<RecentItem>();

            // أوامر الشراء الحديثة
            var recentPOs = await _db.PurchaseTxns
                .OrderByDescending(x => x.CreatedAt)
                .Take(count / 4)
                .ToListAsync();

            foreach (var po in recentPOs)
            {
                recentItems.Add(new RecentItem
                {
                    Id = po.TxnID.ToString(),
                    Title = $"أمر شراء {po.TxnNumber}",
                    Description = $"المورد: {await GetSupplierNameAsync(po.SupplierID)}",
                    Status = GetStatusText((byte)(po.Status)),
                    StatusColor = GetStatusColor((byte)(po.Status)),
                    Date = po.TxnDate,
                    Amount = po.TotalAmount ?? 0,
                    Type = "PO"
                });
            }

            // سندات الاستلام الحديثة
            var recentReceipts = await _db.StoreReceipts
                .OrderByDescending(x => x.CreatedAt)
                .Take(count / 4)
                .ToListAsync();

            foreach (var receipt in recentReceipts)
            {
                recentItems.Add(new RecentItem
                {
                    Id = receipt.ReceiptId.ToString(),
                    Title = $"سند استلام {receipt.ReceiptNumber}",
                    Description = $"المورد: {await GetSupplierNameAsync(receipt.SupplierId ?? 0)}",
                    Status = GetReceiptStatusText((byte)(receipt.Status)),
                    StatusColor = GetReceiptStatusColor((byte)receipt.Status),
                    Date = receipt.ReceiptDate,
                    Amount = receipt.TotalAmount,
                    Type = "GRN"
                });
            }

            // فواتير المشتريات الحديثة
            var recentInvoices = await _db.PurchaseAPs
                .OrderByDescending(x => x.CreatedAt)
                .Take(count / 4)
                .ToListAsync();

            foreach (var invoice in recentInvoices)
            {
                recentItems.Add(new RecentItem
                {
                    Id = invoice.APId.ToString(),
                    Title = $"فاتورة {invoice.DocNumber}",
                    Description = $"المورد: {await GetSupplierNameAsync(invoice.SupplierId)}",
                    Status = GetInvoiceStatusText(invoice.Status),
                    StatusColor = GetInvoiceStatusColor(invoice.Status),
                    Date = invoice.CreatedAt,
                    Amount = invoice.TotalAmount,
                    Type = invoice.DocType
                });
            }

                return recentItems.OrderByDescending(x => x.Date).Take(count).ToList();
            }
            catch (Exception ex)
            {
                // إرجاع قائمة فارغة في حالة الخطأ
                return new List<RecentItem>();
            }
        }

        public async Task<List<AlertItem>> GetAlertsAsync()
        {
            try
            {
                var alerts = new List<AlertItem>();

            // تنبيهات أوامر الشراء المعلقة
            var pendingPOs = await _db.PurchaseTxns
                .Where(x => x.Status == 0 || x.Status == 1)
                .CountAsync();

            if (pendingPOs > 0)
            {
                alerts.Add(new AlertItem
                {
                    Id = "pending_po",
                    Title = "أوامر شراء معلقة",
                    Message = $"يوجد {pendingPOs} أمر شراء في انتظار المعالجة",
                    Type = "Warning",
                    Date = DateTime.Now,
                    IsRead = false,
                    ActionText = "عرض الأوامر",
                    ActionCommand = "ViewPendingPOs"
                });
            }

            // تنبيهات سندات الاستلام المعلقة
            var pendingReceipts = await _db.StoreReceipts
                .Where(x => x.Status == 0 || x.Status == 1)
                .CountAsync();

            if (pendingReceipts > 0)
            {
                alerts.Add(new AlertItem
                {
                    Id = "pending_receipts",
                    Title = "سندات استلام معلقة",
                    Message = $"يوجد {pendingReceipts} سند استلام في انتظار الاعتماد",
                    Type = "Warning",
                    Date = DateTime.Now,
                    IsRead = false,
                    ActionText = "عرض السندات",
                    ActionCommand = "ViewPendingReceipts"
                });
            }

            // تنبيهات فواتير المشتريات المسودة
            var draftInvoices = await _db.PurchaseAPs
                .Where(x => x.Status == 0)
                .CountAsync();

            if (draftInvoices > 0)
            {
                alerts.Add(new AlertItem
                {
                    Id = "draft_invoices",
                    Title = "فواتير مسودة",
                    Message = $"يوجد {draftInvoices} فاتورة في حالة مسودة",
                    Type = "Info",
                    Date = DateTime.Now,
                    IsRead = false,
                    ActionText = "عرض الفواتير",
                    ActionCommand = "ViewDraftInvoices"
                });
            }

                return alerts;
            }
            catch (Exception ex)
            {
                // إرجاع قائمة فارغة في حالة الخطأ
                return new List<AlertItem>();
            }
        }

        private async Task<string> GetSupplierNameAsync(int supplierId)
        {
            var supplier = await _db.Suppliers.FindAsync(supplierId);
            return supplier?.SupplierName ?? "غير محدد";
        }

        private string GetStatusText(byte status)
        {
            return status switch
            {
                0 => "مسودة",
                1 => "مرحل",
                2 => "مكتمل",
                9 => "ملغي",
                _ => "غير محدد"
            };
        }

        private string GetStatusColor(byte status)
        {
            return status switch
            {
                0 => "#6B7280", // رمادي
                1 => "#0A6ED1", // أزرق
                2 => "#0E9F6E", // أخضر
                9 => "#DC2626", // أحمر
                _ => "#6B7280"
            };
        }

        private string GetReceiptStatusText(byte status)
        {
            return status switch
            {
                0 => "مسودة",
                1 => "مرحل",
                2 => "معتمد",
                9 => "ملغي",
                _ => "غير محدد"
            };
        }

        private string GetReceiptStatusColor(byte status)
        {
            return status switch
            {
                0 => "#6B7280", // رمادي
                1 => "#0A6ED1", // أزرق
                2 => "#0E9F6E", // أخضر
                9 => "#DC2626", // أحمر
                _ => "#6B7280"
            };
        }

        private string GetInvoiceStatusText(byte status)
        {
            return status switch
            {
                0 => "مسودة",
                1 => "محفوظ",
                2 => "مرحل",
                8 => "معكوس",
                9 => "ملغي",
                _ => "غير محدد"
            };
        }

        private string GetInvoiceStatusColor(byte status)
        {
            return status switch
            {
                0 => "#6B7280", // رمادي
                1 => "#D97706", // برتقالي
                2 => "#0E9F6E", // أخضر
                8 => "#8B5CF6", // بنفسجي
                9 => "#DC2626", // أحمر
                _ => "#6B7280"
            };
        }
    }
}
