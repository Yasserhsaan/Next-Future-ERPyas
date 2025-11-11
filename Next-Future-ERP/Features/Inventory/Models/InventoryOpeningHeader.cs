using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Inventory.Models
{
    /// <summary>
    /// نموذج رأس مستند الجرد الافتتاحي
    /// </summary>
    public class InventoryOpeningHeader
    {
        [Key]
        public int DocID { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Required]
        [MaxLength(30)]
        public string DocNo { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "date")]
        public DateTime DocDate { get; set; } = DateTime.Today;

        /// <summary>
        /// طريقة الإدخال: يدوي أو تلقائي
        /// </summary>
        public EntryMethod EntryMethod { get; set; } = EntryMethod.Manual;

        /// <summary>
        /// طريقة العرض: حسب الصنف أو حسب المخزن
        /// </summary>
        public ViewMode? ViewMode { get; set; }

        /// <summary>
        /// طريقة احتساب التكاليف
        /// </summary>
        [Required]
        public CostMethod CostMethod { get; set; }

        /// <summary>
        /// نطاق المتوسط المرجح (في حالة اختيار المتوسط المرجح)
        /// </summary>
        public WeightedAvgScope? WeightedAvgScope { get; set; }

        /// <summary>
        /// استخدام تتبع تاريخ الانتهاء
        /// </summary>
        public bool UseExpiry { get; set; } = false;

        /// <summary>
        /// استخدام تتبع أرقام الدفعات
        /// </summary>
        public bool UseBatch { get; set; } = false;

        /// <summary>
        /// استخدام تتبع الأرقام التسلسلية
        /// </summary>
        public bool UseSerial { get; set; } = false;

        /// <summary>
        /// حالة المستند
        /// </summary>
        public InventoryOpeningStatus Status { get; set; } = InventoryOpeningStatus.Draft;

        /// <summary>
        /// ملاحظات عامة على المستند
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// معرف المستخدم الذي اعتمد المستند
        /// </summary>
        public int? ApprovedBy { get; set; }

        /// <summary>
        /// تاريخ ووقت الاعتماد
        /// </summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// معرف المستخدم الذي أنشأ المستند
        /// </summary>
        [Required]
        public int CreatedBy { get; set; }

        /// <summary>
        /// تاريخ ووقت الإنشاء
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// معرف المستخدم الذي عدّل المستند آخر مرة
        /// </summary>
        public int? ModifiedBy { get; set; }

        /// <summary>
        /// تاريخ ووقت آخر تعديل
        /// </summary>
        public DateTime? ModifiedAt { get; set; }

        /// <summary>
        /// تفاصيل المستند (العلاقة مع جدول التفاصيل)
        /// </summary>
        public virtual ICollection<InventoryOpeningDetail> Details { get; set; } = new List<InventoryOpeningDetail>();

        /// <summary>
        /// خاصية للتحقق من إمكانية التعديل
        /// </summary>
        [NotMapped]
        public bool CanEdit => Status == InventoryOpeningStatus.Draft;

        /// <summary>
        /// خاصية للتحقق من إمكانية الاعتماد
        /// </summary>
        [NotMapped]
        public bool CanApprove => Status == InventoryOpeningStatus.Draft && Details.Any();

        /// <summary>
        /// خاصية لعرض نص حالة المستند
        /// </summary>
        [NotMapped]
        public string StatusText => Status switch
        {
            InventoryOpeningStatus.Draft => "مسودة",
            InventoryOpeningStatus.Approved => "معتمد",
            _ => "غير محدد"
        };
    }
}
