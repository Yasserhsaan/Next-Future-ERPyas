using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Features.StoreIssues.Models
{
    [Table("StoreIssuesDetailed")]
    public class StoreIssueDetail
    {
        [Key]
        public long DetailId { get; set; }

        [Required]
        public long IssueId { get; set; }

        [Required]
        public int LineNo { get; set; } = 1;

        [Required]
        public int ItemId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        // تتبع الدُفعات/الصلاحية
        public int? BatchID { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }

        // الكمية والتكلفة الدفترية
        public int? UnitId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitCost { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalCost => Quantity * UnitCost;

        // تحليل محاسبي
        public int? CostCenterId { get; set; }
        public string? DebitAccount { get; set; }
        public string? CreditAccount { get; set; }

        // عملة/سعر صرف
        [Required]
        public int CurrencyId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,6)")]
        public decimal ExchangeRate { get; set; }

        // طوابع
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }

        // Navigation Properties
        [ForeignKey("IssueId")]
        public virtual StoreIssue? StoreIssue { get; set; }

        [ForeignKey("ItemId")]
        public virtual Item? Item { get; set; }

        [ForeignKey("WarehouseId")]
        public virtual Warehouse? Warehouse { get; set; }

        [ForeignKey("BatchID")]
        public virtual ItemBatch? Batch { get; set; }

        [ForeignKey("UnitId")]
        public virtual UnitModel? Unit { get; set; }

        [ForeignKey("CostCenterId")]
        public virtual CostCenter? CostCenter { get; set; }

        [ForeignKey("CurrencyId")]
        public virtual NextCurrency? Currency { get; set; }

        // NotMapped properties for UI display
        [NotMapped]
        public string ItemName => Item?.ItemName ?? "غير محدد";

        [NotMapped]
        public string ItemCode => Item?.ItemCode ?? "غير محدد";

        [NotMapped]
        public string WarehouseName => Warehouse?.WarehouseName ?? "غير محدد";

        [NotMapped]
        public string BatchNumberDisplay => BatchNumber ?? "غير محدد";

        [NotMapped]
        public string UnitName => Unit?.UnitName ?? "غير محدد";

        [NotMapped]
        public string CostCenterName => CostCenter?.CostCenterName ?? "غير محدد";

        [NotMapped]
        public string CurrencyName => Currency?.CurrencyNameAr ?? "غير محدد";

        [NotMapped]
        public string ExpiryDateText => ExpiryDate?.ToString("yyyy-MM-dd") ?? "غير محدد";

        [NotMapped]
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Today;

        [NotMapped]
        public string ExpiryStatus => IsExpired ? "منتهي الصلاحية" : "صالح";

        // مؤشرات التحذير البصرية
        [NotMapped]
        public bool HasInventoryWarning
        {
            get
            {
                if (Item == null || Warehouse == null) return false;
                
                // تحذير إذا كان المخزون لا يسمح بالسالب والكمية المتاحة غير كافية
                if (Warehouse.AllowNegativeStock != true)
                {
                    // هذا يحتاج إلى حساب الكمية المتاحة - سيتم تطبيقه في الخدمة
                    return false; // مؤقتاً
                }
                
                return false;
            }
        }

        [NotMapped]
        public bool HasCostCenterWarning
        {
            get
            {
                // تحذير إذا كان مركز الكلفة مطلوباً ولم يكن محدداً
                // هذا يحتاج إلى معرفة سياسة الجهة - سيتم تطبيقه في الخدمة
                return false; // مؤقتاً
            }
        }

        [NotMapped]
        public bool HasAccountWarning
        {
            get
            {
                // تحذير إذا كان الحساب المدين أو الدائن غير محدد
                return string.IsNullOrEmpty(DebitAccount) || string.IsNullOrEmpty(CreditAccount);
            }
        }

        [NotMapped]
        public bool HasBatchWarning
        {
            get
            {
                // تحذير إذا كان الصنف يتطلب دفعة ولم تكن محددة
                return Item?.IsBatchTracked == true && (BatchID == null || string.IsNullOrEmpty(BatchNumber));
            }
        }

        [NotMapped]
        public bool HasExpiryWarning
        {
            get
            {
                // تحذير إذا كان الصنف له صلاحية وكانت منتهية
                return Item?.HasExpiryDate == true && IsExpired;
            }
        }

        [NotMapped]
        public bool HasUnitCostWarning
        {
            get
            {
                // تحذير إذا كانت تكلفة الوحدة صفر أو سالبة
                return UnitCost <= 0;
            }
        }

        [NotMapped]
        public string WarningText
        {
            get
            {
                var warnings = new List<string>();
                
                if (HasInventoryWarning) warnings.Add("تحذير مخزون");
                if (HasCostCenterWarning) warnings.Add("مركز كلفة مطلوب");
                if (HasAccountWarning) warnings.Add("حساب مفقود");
                if (HasBatchWarning) warnings.Add("دفعة مطلوبة");
                if (HasExpiryWarning) warnings.Add("منتهي الصلاحية");
                if (HasUnitCostWarning) warnings.Add("تكلفة غير صحيحة");
                
                return warnings.Count > 0 ? string.Join(", ", warnings) : "";
            }
        }

        [NotMapped]
        public bool HasAnyWarning => HasInventoryWarning || HasCostCenterWarning || HasAccountWarning || 
                                   HasBatchWarning || HasExpiryWarning || HasUnitCostWarning;
    }
}