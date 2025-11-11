using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.StoreReceipts.Models
{
    public class StoreReceiptDetailed
    {
        public long DetailId { get; set; }
        public long ReceiptId { get; set; }
        public int ItemId { get; set; }
        public int UnitId { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? VatRate { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal? TotalPrice { get; set; }
        public int? CostCenterId { get; set; }
        public int WarehouseId { get; set; }
        
        // خاصية للكمية المتبقية من أمر الشراء (للعرض فقط)
        [NotMapped]
        public decimal RemainingQuantity { get; set; }
        public int CurrencyId { get; set; }
        public decimal ExchangeRate { get; set; }
        public string? DebitAccount { get; set; }
        public string? CreditAccount { get; set; }

        // Navigation Properties
        public StoreReceipt Receipt { get; set; } = null!;
    }
}
