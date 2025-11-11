using System.ComponentModel;

namespace Next_Future_ERP.Features.Dashboard.Models
{
    public class PurchaseDashboardData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // إحصائيات عامة
        private int _totalPurchaseOrders;
        private int _pendingPurchaseOrders;
        private int _completedPurchaseOrders;
        private decimal _totalPurchaseAmount;
        private decimal _pendingAmount;

        // إحصائيات سندات الاستلام
        private int _totalReceipts;
        private int _pendingReceipts;
        private int _approvedReceipts;
        private decimal _totalReceiptAmount;

        // إحصائيات فواتير المشتريات
        private int _totalInvoices;
        private int _draftInvoices;
        private int _postedInvoices;
        private decimal _totalInvoiceAmount;
        private decimal _unpaidAmount;

        // إحصائيات الموردين
        private int _totalSuppliers;
        private int _activeSuppliers;
        private int _blacklistedSuppliers;

        // إحصائيات المخزون
        private int _totalItems;
        private int _lowStockItems;
        private int _outOfStockItems;
        private decimal _totalInventoryValue;

        // خصائص إحصائيات أوامر الشراء
        public int TotalPurchaseOrders
        {
            get => _totalPurchaseOrders;
            set { _totalPurchaseOrders = value; OnPropertyChanged(); OnPropertyChanged(nameof(CompletionRate)); }
        }

        public int PendingPurchaseOrders
        {
            get => _pendingPurchaseOrders;
            set { _pendingPurchaseOrders = value; OnPropertyChanged(); }
        }

        public int CompletedPurchaseOrders
        {
            get => _completedPurchaseOrders;
            set { _completedPurchaseOrders = value; OnPropertyChanged(); OnPropertyChanged(nameof(CompletionRate)); }
        }

        public decimal TotalPurchaseAmount
        {
            get => _totalPurchaseAmount;
            set { _totalPurchaseAmount = value; OnPropertyChanged(); }
        }

        public decimal PendingAmount
        {
            get => _pendingAmount;
            set { _pendingAmount = value; OnPropertyChanged(); }
        }

        // خصائص إحصائيات سندات الاستلام
        public int TotalReceipts
        {
            get => _totalReceipts;
            set { _totalReceipts = value; OnPropertyChanged(); OnPropertyChanged(nameof(ApprovalRate)); }
        }

        public int PendingReceipts
        {
            get => _pendingReceipts;
            set { _pendingReceipts = value; OnPropertyChanged(); }
        }

        public int ApprovedReceipts
        {
            get => _approvedReceipts;
            set { _approvedReceipts = value; OnPropertyChanged(); OnPropertyChanged(nameof(ApprovalRate)); }
        }

        public decimal TotalReceiptAmount
        {
            get => _totalReceiptAmount;
            set { _totalReceiptAmount = value; OnPropertyChanged(); }
        }

        // خصائص إحصائيات فواتير المشتريات
        public int TotalInvoices
        {
            get => _totalInvoices;
            set { _totalInvoices = value; OnPropertyChanged(); }
        }

        public int DraftInvoices
        {
            get => _draftInvoices;
            set { _draftInvoices = value; OnPropertyChanged(); }
        }

        public int PostedInvoices
        {
            get => _postedInvoices;
            set { _postedInvoices = value; OnPropertyChanged(); }
        }

        public decimal TotalInvoiceAmount
        {
            get => _totalInvoiceAmount;
            set { _totalInvoiceAmount = value; OnPropertyChanged(); OnPropertyChanged(nameof(PaymentRate)); }
        }

        public decimal UnpaidAmount
        {
            get => _unpaidAmount;
            set { _unpaidAmount = value; OnPropertyChanged(); OnPropertyChanged(nameof(PaymentRate)); }
        }

        // خصائص إحصائيات الموردين
        public int TotalSuppliers
        {
            get => _totalSuppliers;
            set { _totalSuppliers = value; OnPropertyChanged(); }
        }

        public int ActiveSuppliers
        {
            get => _activeSuppliers;
            set { _activeSuppliers = value; OnPropertyChanged(); }
        }

        public int BlacklistedSuppliers
        {
            get => _blacklistedSuppliers;
            set { _blacklistedSuppliers = value; OnPropertyChanged(); }
        }

        // خصائص إحصائيات المخزون
        public int TotalItems
        {
            get => _totalItems;
            set { _totalItems = value; OnPropertyChanged(); }
        }

        public int LowStockItems
        {
            get => _lowStockItems;
            set { _lowStockItems = value; OnPropertyChanged(); }
        }

        public int OutOfStockItems
        {
            get => _outOfStockItems;
            set { _outOfStockItems = value; OnPropertyChanged(); }
        }

        public decimal TotalInventoryValue
        {
            get => _totalInventoryValue;
            set { _totalInventoryValue = value; OnPropertyChanged(); }
        }

        // خصائص محسوبة - للقراءة فقط
        public decimal CompletionRate => TotalPurchaseOrders > 0 ? (decimal)CompletedPurchaseOrders / TotalPurchaseOrders * 100 : 0;
        public decimal ApprovalRate => TotalReceipts > 0 ? (decimal)ApprovedReceipts / TotalReceipts * 100 : 0;
        public decimal PaymentRate => TotalInvoiceAmount > 0 ? (TotalInvoiceAmount - UnpaidAmount) / TotalInvoiceAmount * 100 : 0;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // نموذج للعناصر الحديثة
    public class RecentItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty; // PO, GRN, PI, PR
    }

    // نموذج للتنبيهات
    public class AlertItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Warning, Error, Info, Success
        public DateTime Date { get; set; }
        public bool IsRead { get; set; }
        public string ActionText { get; set; } = string.Empty;
        public string ActionCommand { get; set; } = string.Empty;
    }
}
