using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Features.PurchaseInvoices.Models
{
    public class PurchaseAP
    {
        [Key]
        public long APId { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        [Required]
        public int BranchId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string DocNumber { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(2)]
        public string DocType { get; set; } = string.Empty; // PI أو PR
        
        [Required]
        public DateTime DocDate { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        [Required]
        public int SupplierId { get; set; }
        
        [MaxLength(50)]
        public string? ReferenceNumber { get; set; } // رقم فاتورة المورد
        
        [Required]
        public int CurrencyId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,6)")]
        public decimal ExchangeRate { get; set; } = 1;
        
        public long? RelatedReceiptId { get; set; } // FK → StoreReceipts
        
        public int? RelatedPOId { get; set; } // FK → PurchaseTxn
        
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal SubTotal { get; set; } = 0;
        
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TaxAmount { get; set; } = 0;
        
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalAmount { get; set; } = 0;
        
        [Required]
        public bool PriceIncludesTax { get; set; } = false;
        
        [Required]
        public byte Status { get; set; } = 0; // 0 Draft, 1 Saved, 2 Posted, 8 Reversed, 9 Canceled
        
        public long? JournalEntryId { get; set; } // FK → GeneralJournalEntries
        
        [MaxLength(1000)]
        public string? Remarks { get; set; }
        
        [Required]
        public int CreatedBy { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public int? ModifiedBy { get; set; }
        
        public DateTime? ModifiedAt { get; set; }
        
        // خصائص محسوبة
        [NotMapped]
        public int TxYear => DocDate.Year;
        
        [NotMapped]
        public int TxMonth => DocDate.Month;
        
        [NotMapped]
        public int DocSign => DocType == "PR" ? -1 : 1;
        
        [NotMapped]
        public string StatusText => Status switch
        {
            0 => "مسودة",
            1 => "محفوظ",
            2 => "مرحل",
            8 => "معكوس",
            9 => "ملغي",
            _ => "غير محدد"
        };
        
        [NotMapped]
        public string? SupplierName { get; set; }
        
        [NotMapped]
        public string DocTypeText => DocType switch
        {
            "PI" => "فاتورة مشتريات",
            "PR" => "مرتجع مشتريات",
            _ => "غير محدد"
        };

        // Navigation Properties
        public ICollection<PurchaseAPDetail> Details { get; set; } = new List<PurchaseAPDetail>();
    }
}
