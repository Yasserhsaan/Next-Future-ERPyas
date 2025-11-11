using Next_Future_ERP.Features.Warehouses.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Features.Items.Models
{
    public class Item
    {
        public int ItemID { get; set; }
        public string ItemCode { get; set; } = "";
        public string ItemName { get; set; } = "";
        public string? ItemBarcode { get; set; }

        public int? CategoryID { get; set; }
        public CategoryModel? Category { get; set; }

        // S = Simple, C = Composite, W = Weighted
        public string ItemType { get; set; } = "S";

        // Navigation property for ItemType
        [NotMapped]
        public ItemType? ItemTypeInfo { get; set; }

        public bool? IsBatchTracked { get; set; }
        public bool? IsSerialTracked { get; set; }
        public bool? HasExpiryDate { get; set; }

        public int? UnitID { get; set; }               // الوحدة الأساسية
        public UnitModel? Unit { get; set; }

        public decimal? Weight { get; set; }
        public decimal? Volume { get; set; }
        public decimal? MinStockLevel { get; set; }
        public decimal? MaxStockLevel { get; set; }
        public decimal? ReorderLevel { get; set; }
        public decimal? StandardCost { get; set; }
        public decimal? LastPurchasePrice { get; set; }

        public int? ValuationGroup { get; set; }       // لو عندك ValuationGroupId
        public bool? IsActive { get; set; } = true;

        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public ICollection<ItemUnit> Units { get; set; } = new List<ItemUnit>();
    }
}
