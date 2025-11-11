using Next_Future_ERP.Features.Warehouses.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Features.Items.Models
{
    public class ItemUnit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BarcodeID { get; set; }          // المفتاح الأساسي
        public int ItemID { get; set; }
        public Item? Item { get; set; }
        public int UnitID { get; set; }
        public UnitModel? Unit { get; set; }
        public string UnitBarcode { get; set; } = "";
        public string? BarcodeType { get; set; }
        public bool? IsPrimary { get; set; }         // أساسية
        public bool? IsSalesUnit { get; set; }       // للبيع
        public bool? PurchaseUnit { get; set; }      // للشراء
        public bool? IsInventoryUnit { get; set; }   // للمخزون

        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
    }
}
