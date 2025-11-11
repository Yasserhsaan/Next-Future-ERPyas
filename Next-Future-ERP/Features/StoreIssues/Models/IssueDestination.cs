using Next_Future_ERP.Models;
using Next_Future_ERP.Features.Accounts.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Features.StoreIssues.Models
{
    [Table("IssueDestinations")]
    public class IssueDestination
    {
        [Key]
        public int DestinationID { get; set; }

        [Required]
        [Column("CompanyId")]
        public int CompanyID { get; set; }

        [Required]
        [Column("BranchId")]
        public int BranchID { get; set; }

        [Required]
        [StringLength(20)]
        public string DestinationCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string DestinationName { get; set; } = string.Empty;

        [Required]
        [StringLength(1)]
        public char DestinationType { get; set; } // E=Expense, P=Production, C=COGS, S=Shrinkage, A=Adjustment, O=Other

        public int? AccountID { get; set; }

        public int? CostCenterID { get; set; }

        public bool UsesCostCenter { get; set; } = false;

        public bool AllowAccountOverride { get; set; } = false;

        public bool AllowLineOverride { get; set; } = false;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [Column("CreatedDate")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? CreatedBy { get; set; }

        [Column("ModifiedDate")]
        public DateTime? ModifiedAt { get; set; }

        public int? ModifiedBy { get; set; }

        // Navigation Properties
        [ForeignKey("AccountID")]
        public virtual Account? Account { get; set; }

        [ForeignKey("CostCenterID")]
        public virtual CostCenter? CostCenter { get; set; }

        // Computed Properties
        [NotMapped]
        public string DestinationTypeText => DestinationType switch
        {
            'E' => "مصروف",
            'P' => "تشغيل",
            'C' => "تكلفة مبيعات",
            'S' => "هالك",
            'A' => "تسوية",
            'O' => "أخرى",
            _ => "غير محدد"
        };

        [NotMapped]
        public string AccountName => Account?.AccountNameAr ?? "غير محدد";

        [NotMapped]
        public string CostCenterName => CostCenter?.CostCenterName ?? "غير محدد";

        [NotMapped]
        public string StatusText => IsActive ? "نشط" : "غير نشط";

        [NotMapped]
        public string PoliciesText
        {
            get
            {
                var policies = new List<string>();
                if (UsesCostCenter) policies.Add("مركز كلفة");
                if (AllowAccountOverride) policies.Add("تجاوز حساب");
                if (AllowLineOverride) policies.Add("تجاوز سطر");
                return policies.Count > 0 ? string.Join(", ", policies) : "لا توجد";
            }
        }
    }
}
