using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Features.Items.Models
{
    [Table("InventoryBalances")]
    public class InventoryBalance
    {
        [Key]
        public long BalanceID { get; set; }

        [Required]
        public int WarehouseID { get; set; }

        [Required]
        public int ItemID { get; set; }

        public int? BatchID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        [Required]
        public int UnitID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal AvgCost { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? LastCost { get; set; }

        public long? LastTransactionID { get; set; }

        public DateTime? LastUpdate { get; set; }

        public int? CompanyId { get; set; }

        public int? BranchID { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? FtCost { get; set; }

        // Navigation Properties
        public virtual Item Item { get; set; }
        public virtual Next_Future_ERP.Features.Warehouses.Models.Warehouse Warehouse { get; set; }
        public virtual ItemBatch Batch { get; set; }
        public virtual Next_Future_ERP.Features.Warehouses.Models.UnitModel Unit { get; set; }

        // Computed Properties for Display
        [NotMapped]
        public string WarehouseName => Warehouse?.WarehouseName ?? WarehouseID.ToString();

        [NotMapped]
        public string UnitName => Unit?.UnitName ?? UnitID.ToString();
    }
}
