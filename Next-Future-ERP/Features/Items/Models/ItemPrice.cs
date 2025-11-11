using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Features.Items.Models
{
    [Table("ItemPrices")]
    public class ItemPrice
    {
        [Key]
        public int PriceID { get; set; }
        
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int ItemID { get; set; }
        public int UnitID { get; set; }
        public int PriceLevelId { get; set; }
        public int CurrencyId { get; set; }
        
        public byte PriceType { get; set; } = 1;
        public byte Method { get; set; } = 1;
        public byte Base { get; set; } = 1;
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal? PriceAmount { get; set; }
        
        [Column(TypeName = "decimal(7,4)")]
        public decimal? PricePercent { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal SellPrice { get; set; }
        
        public byte? RoundingRule { get; set; }
        
        [Column(TypeName = "date")]
        public DateTime EffectiveFrom { get; set; }
        
        [Column(TypeName = "date")]
        public DateTime? EffectiveTo { get; set; }
        
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public byte Status { get; set; } = 1;
        
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        
        [MaxLength(400)]
        public string? Notes { get; set; }
        
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
