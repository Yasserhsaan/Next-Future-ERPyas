using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Purchases.Models
{
    public class PurchaseTxn
    {
        public int TxnID { get; set; }
        public int CompanyID { get; set; }
        public int BranchID { get; set; }
        public string TxnNumber { get; set; } = null!;
        public char TxnType { get; set; } // 'P' أو 'R'
        public int SupplierID { get; set; }
        public DateTime TxnDate { get; set; }
        public DateTime? ExpectedDelivery { get; set; }
        public byte? Status { get; set; } // 0=مسودة, 1=مرحل, 2=معتمد, 9=ملغي (مثال)
        public decimal? SubTotal { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public int? ParentTxnID { get; set; }
        public bool IsSynced { get; set; }

        // Navigation properties for display (not mapped to database)
        [NotMapped]
        public string? SupplierName { get; set; }
        
        [NotMapped]
        public string? Description { get; set; }

        // Computed properties for UI
        [NotMapped]
        public string StatusText => Status switch
        {
            0 => "مسودة",
            1 => "مرحل",
            2 => "معتمد",
            9 => "ملغي",
            _ => "غير محدد"
        };

        public ICollection<PurchaseTxnDetail> Details { get; set; } = new List<PurchaseTxnDetail>();
    }
}
