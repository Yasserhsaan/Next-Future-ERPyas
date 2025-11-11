using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Features.StoreIssues.Models
{
    [Table("StoreIssues")]
    public class StoreIssue
    {
        [Key]
        public long IssueId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Required]
        [StringLength(50)]
        public string IssueNumber { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "date")]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        [Required]
        public int IssueDestinationID { get; set; }

        public int? DefaultWarehouseId { get; set; }

        public long? CustomerId { get; set; }
        public long? SalesOrderId { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Description { get; set; }

        [Required]
        public int CurrencyId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,6)")]
        public decimal ExchangeRate { get; set; } = 1.0m;

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalAmount { get; set; } = 0;

        [Required]
        public byte Status { get; set; } = 0; // 0=Draft, 1=Posted, 2=Canceled

        [Required]
        public int CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }

        // Navigation Properties
        [ForeignKey("IssueDestinationID")]
        public virtual IssueDestination? Destination { get; set; }

        [ForeignKey("DefaultWarehouseId")]
        public virtual Warehouse? DefaultWarehouse { get; set; }

        [ForeignKey("CurrencyId")]
        public virtual NextCurrency? Currency { get; set; }

        public virtual ICollection<StoreIssueDetail> Details { get; set; } = new List<StoreIssueDetail>();

        // NotMapped properties for UI display
        [NotMapped]
        public string DestinationName => Destination?.DestinationName ?? "غير محدد";

        [NotMapped]
        public string CurrencyName => Currency?.CurrencyNameAr ?? "غير محدد";

        [NotMapped]
        public string DefaultWarehouseName => DefaultWarehouse?.WarehouseName ?? "غير محدد";

        [NotMapped]
        public string StatusText => Status switch
        {
            0 => "مسودة",
            1 => "مرحل",
            2 => "ملغي",
            _ => "غير معروف"
        };

        [NotMapped]
        public string StatusColor => Status switch
        {
            0 => "#6B7280", // Gray for Draft
            1 => "#10B981", // Green for Posted
            2 => "#EF4444", // Red for Canceled
            _ => "#6B7280"
        };
    }
}