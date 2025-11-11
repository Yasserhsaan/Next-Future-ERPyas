
using Next_Future_ERP.Features.Items.Models;



using Next_Future_ERP.Features.Warehouses.Models;
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
    /// نموذج تفاصيل مستند الجرد الافتتاحي
    /// </summary>
    public class InventoryOpeningDetail
    {
        [Key]
        public int LineID { get; set; }

        /// <summary>
        /// معرف رأس المستند
        /// </summary>
        [Required]
        public int DocID { get; set; }

        /// <summary>
        /// العلاقة مع رأس المستند
        /// </summary>
        [ForeignKey("DocID")]
        public virtual InventoryOpeningHeader Header { get; set; } = null!;

        /// <summary>
        /// معرف الصنف
        /// </summary>
        [Required]
        public int ItemID { get; set; }

        /// <summary>
        /// العلاقة مع الصنف
        /// </summary>
        [ForeignKey("ItemID")]

        public virtual Item? Item { get; set; }

       


        /// <summary>
        /// معرف الوحدة الأساسية للصنف
        /// </summary>
        [Required]
        public int UnitID { get; set; }

        /// <summary>
        /// العلاقة مع الوحدة الأساسية
        /// </summary>
        [ForeignKey("UnitID")]
        public virtual UnitModel? Unit { get; set; }

        /// <summary>
        /// معرف الوحدة العددية (للأصناف المقاسة بالوزن/الطول/الحجم)
        /// </summary>
        public int? NumericUnitID { get; set; }

        /// <summary>
        /// العلاقة مع الوحدة العددية
        /// </summary>
        [ForeignKey("NumericUnitID")]
        public virtual UnitModel? NumericUnit { get; set; }

        /// <summary>
        /// الكمية العددية (وزن/طول/حجم)
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal? NumericQty { get; set; }

        /// <summary>
        /// الكمية الأساسية المخزنية (تُحتسب من NumericQty ومعامل التحويل)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,6)")]
        [Range(0, double.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من أو تساوي الصفر")]
        public decimal Qty { get; set; }

        /// <summary>
        /// معرف المخزن
        /// </summary>
        [Required]
        public int WarehouseId { get; set; }

        /// <summary>
        /// العلاقة مع المخزن
        /// </summary>
        [ForeignKey("WarehouseId")]
        public virtual Warehouse? Warehouse { get; set; }

        /// <summary>
        /// تاريخ الانتهاء (للأصناف ذات الصلاحية)
        /// </summary>
        [Column(TypeName = "date")]
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// رقم الدفعة
        /// </summary>
        [MaxLength(50)]
        public string? BatchNo { get; set; }

        /// <summary>
        /// الرقم التسلسلي
        /// </summary>
        [MaxLength(50)]
        public string? SerialNo { get; set; }

        /// <summary>
        /// تكلفة الوحدة الافتتاحية
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        [Range(0, double.MaxValue, ErrorMessage = "التكلفة يجب أن تكون أكبر من أو تساوي الصفر")]
        public decimal? InitialUnitCost { get; set; }

        /// <summary>
        /// متوسط التكلفة الحالي (للعرض فقط كمرجع)
        /// </summary>
        [Column(TypeName = "decimal(18,6)")]
        public decimal? AvgCostSnapshot { get; set; }

        /// <summary>
        /// معرف العملة
        /// </summary>
        public int? CurrencyId { get; set; }

        /// <summary>
        /// ملاحظات السطر
        /// </summary>
        [MaxLength(300)]
        public string? LineNotes { get; set; }

        /// <summary>
        /// إجمالي التكلفة للسطر (حقل محتسب)
        /// </summary>
        [NotMapped]
        public decimal? TotalCost => InitialUnitCost.HasValue ? Qty * InitialUnitCost.Value : null;

        /// <summary>
        /// التحقق من صحة البيانات
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            // التحقق من الكميات
            if (Qty < 0 || (NumericQty.HasValue && NumericQty.Value < 0))
                return false;

            // التحقق من تاريخ الانتهاء
            if (ExpiryDate.HasValue && ExpiryDate.Value < new DateTime(2000, 1, 1))
                return false;

            // التحقق من التكلفة
            if (InitialUnitCost.HasValue && InitialUnitCost.Value < 0)
                return false;

            return true;
        }

        /// <summary>
        /// خاصية لعرض معلومات الصنف
        /// </summary>
        [NotMapped]
        public string ItemInfo => Item != null ? $"{Item.ItemCode} - {Item.ItemName}" : $"صنف رقم {ItemID}";

        /// <summary>
        /// خاصية لعرض معلومات المخزن
        /// </summary>
        [NotMapped]
        public string WarehouseInfo => Warehouse != null ? $"{Warehouse.WarehouseCode} - {Warehouse.WarehouseName}" : $"مخزن رقم {WarehouseId}";

        /// <summary>
        /// خاصية لعرض معلومات الوحدة
        /// </summary>
        [NotMapped]
        public string UnitInfo => Unit?.UnitName ?? $"وحدة رقم {UnitID}";

        /// <summary>
        /// خاصية لعرض معلومات الوحدة العددية
        /// </summary>
        [NotMapped]
        public string? NumericUnitInfo => NumericUnit?.UnitName;
    }
}
    