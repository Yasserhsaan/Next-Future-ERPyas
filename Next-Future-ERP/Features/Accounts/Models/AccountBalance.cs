using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Features.Accounts.Models
{
    [Table("AccountBalances")]
    public class AccountBalance
    {
        [Key]
        public int BalanceId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Required]
        public int AccountId { get; set; }

        [Required]
        public int CurrencyId { get; set; }

        [Required]
        public byte PeriodType { get; set; } // 0=Daily, 1=Monthly, 2=Yearly

        [Required]
        public short FiscalYear { get; set; }

        public byte? FiscalMonth { get; set; }

        public byte? FiscalDay { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        [Required]
        public decimal OpeningDebit { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        [Required]
        public decimal OpeningCredit { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        [Required]
        public decimal PeriodDebit { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        [Required]
        public decimal PeriodCredit { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        [Required]
        public decimal ClosingDebit { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        [Required]
        public decimal ClosingCredit { get; set; }

        public DateTime? LastMovementAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int? CostCenterId { get; set; }

        // Computed columns (Persisted in DB but calculated here for display)
        [NotMapped]
        public decimal OpeningNet => OpeningDebit - OpeningCredit;

        [NotMapped]
        public decimal PeriodNet => PeriodDebit - PeriodCredit;

        [NotMapped]
        public decimal ClosingNet => ClosingDebit - ClosingCredit;

        // Display properties
        [NotMapped]
        public string PeriodTypeText => PeriodType switch
        {
            0 => "يومي",
            1 => "شهري", 
            2 => "سنوي",
            _ => "غير محدد"
        };

        [NotMapped]
        public string? AccountCode { get; set; }

        [NotMapped]
        public string? AccountNameAr { get; set; }

        [NotMapped]
        public string? CurrencyNameAr { get; set; }

        [NotMapped]
        public string? CostCenterName { get; set; }
    }
}
