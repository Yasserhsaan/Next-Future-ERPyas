// SalesSetting.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Models
{
   
    public class SalesSetting
    {
        [Key]
        public int SalesSettingId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int BranchId { get; set; }

        [MaxLength(20)]
        public string? InvoiceNumbering { get; set; }

        [MaxLength(100)]
        public string? DefaultSalesRep { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? DefaultTaxRate { get; set; }

        [Required]
        public bool? AutoPostInvoice { get; set; } = false;

      
        [Required]
        public bool AllowDiscount { get; set; } = false;

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? MaxDiscount { get; set; }

        [Required]
        public bool PosEnabled { get; set; } = false;

        [Required]
        public bool PosAutoPrint { get; set; } = false;

        [MaxLength(200)]
        public string PosPaymentMethods { get; set; }
    }
}