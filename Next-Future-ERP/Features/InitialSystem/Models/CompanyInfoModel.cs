using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media.Imaging;

namespace Next_Future_ERP.Features.InitialSystem.Models
{
    [Table("SystemSettings")]
    public class CompanyInfoModel
    {

        [Key]
        public int CompId { get; set; }
        [Required]
        public string CompName { get; set; } = string.Empty;

        public BitmapImage? LogoUrl { get; set; }
     
        public string? Language { get; set; }
        [Required]
        public string Currency { get; set; } = string.Empty;

        public string? Timezone { get; set; }

        public string? DateFormat { get; set; }

        public String FiscalYearStart { get; set; }

        public bool? MultiDeviceLogin { get; set; }

        public int? MinPasswordLength { get; set; }

        public bool? UseVat { get; set; }

        public DateTime? VatActivationDate { get; set; }

        public int? AccountNumberLength { get; set; }

        public int? SubAccountLevel { get; set; }
        public bool? EnableForeignCurrency { get; set; }
        public bool? EnableCostCenters { get; set; }
        public bool? HijriSupport { get; set; }

        public int? HijriAdjustment { get; set; }
        public int? AssetsStart { get; set; }
        public int? LiabilitiesStart { get; set; }
        public int? EquityStart { get; set; }
        public int? RevenueStart { get; set; }
        public int? ExpenseStart { get; set; }

        public bool? ArabicLanguage { get; set; }

       
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? CreatedAt { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? UpdatedAt { get; set; }

    }
}
