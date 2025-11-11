using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Features.PurchaseInvoices.Models
{
    public class PurchaseAPDetail
    {
        [Key]
        public long DetailId { get; set; }
        
        [Required]
        public long APId { get; set; }
        
        [Required]
        [Column("LinNo")]
        public int LineNo { get; set; }
        
        [Required]
        public int ItemId { get; set; }
        
        [Required]
        public int UnitId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; }
        
        [Required]
        public bool PriceIncludesTax { get; set; } = false;
        
        public int? VATCodeID { get; set; }
        
        [Column(TypeName = "decimal(7,4)")]
        public decimal? VATRate { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal? TaxableAmount { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal? VATAmount { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal? LineTotal { get; set; }
        
        public int? WarehouseId { get; set; }
        
        public int? BatchID { get; set; }
        
        public int? PurchaseDetailId { get; set; } // FK → PurchaseTxnDetails
        
        public long? ReceiptDetailId { get; set; } // FK → StoreReceiptsDetailed
        
        public int? CostCenterId { get; set; }
        
        [Required]
        public int CurrencyId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,6)")]
        public decimal ExchangeRate { get; set; } = 1;
        
        [MaxLength(500)]
        public string? Remarks { get; set; }
        
        // خصائص محسوبة
        [NotMapped]
        public string? ItemName { get; set; }
        
        [NotMapped]
        public string? UnitName { get; set; }
        
        [NotMapped]
        public string? WarehouseName { get; set; }
        
        [NotMapped]
        public string? VATCodeName { get; set; }

        // Navigation Properties
        public PurchaseAP PurchaseAP { get; set; } = null!;
    }
}
