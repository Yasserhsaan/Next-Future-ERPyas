using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.StoreReceipts.Models
{
    public class StoreReceipt
    {
        public long ReceiptId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public string ReceiptNumber { get; set; } = null!;
        public DateTime ReceiptDate { get; set; }
        public int? SupplierId { get; set; }
        public int? PurchaseOrderId { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Description { get; set; }
        public decimal TotalAmount { get; set; }
        public int CurrencyId { get; set; }
        public decimal ExchangeRate { get; set; }
        public byte Status { get; set; } // 0=مسودة, 1=مرحل, 2=معتمد, 9=ملغي
        public int CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }

        // Computed Properties
        [NotMapped]
        public string SupplierName { get; set; } = string.Empty;

        [NotMapped]
        public string StatusText => Status switch
        {
            0 => "مسودة",
            1 => "مرحل",
            2 => "معتمد",
            9 => "ملغي",
            _ => "غير محدد"
        };

        // Navigation Properties
        public ICollection<StoreReceiptDetailed> Details { get; set; } = new List<StoreReceiptDetailed>();
    }
}
