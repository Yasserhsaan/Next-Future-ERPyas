using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Models
{
    [Table("Accounts")]
    public partial class Account
    {
        [Key]
        public int AccountId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public string AccountCode { get; set; }
        public string AccountNameAr { get; set; }
        public string? AccountNameEn { get; set; }
        public string? ParentAccountCode { get; set; }
        public string AccountDisplay => $"{AccountCode} — {AccountNameAr}";
        public byte AccountLevel { get; set; }
        public byte AccountType { get; set; }
        public byte? AccountClassification { get; set; }
        public byte? Nature { get; set; }
        public byte? ClosingAccountType { get; set; }
        public byte? PaymentType { get; set; }
        public bool? UsesCostCenter { get; set; }
        public bool? UsesProject { get; set; }
        public bool? IsArchived { get; set; }
        public bool? IsActive { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? AnalyticalAccounting { get; set; }
        public int? AccountRepId { get; set; }
        public int? TypeOfCashFlow { get; set; }
        public int? AccountGroupId { get; set; }
        public int? AccountLevelPrivlige { get; set; }
        public string? AccountCategoryKey { get; set; }

        [NotMapped]
        public List<Account> Children { get; set; } = new();
    }
}
