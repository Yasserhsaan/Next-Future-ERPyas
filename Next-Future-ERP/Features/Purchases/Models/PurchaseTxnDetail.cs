using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Purchases.Models
{
    public class PurchaseTxnDetail
    {
        public int DetailID { get; set; }
        public int TxnID { get; set; }
        public int CompanyID { get; set; }
        public int BranchID { get; set; }
        public int ItemID { get; set; }
        public decimal Quantity { get; set; }
        public int UnitID { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal VATRate { get; set; }
        public decimal VATAmount { get; set; }
        public decimal LineTotal { get; set; }
        public decimal? ReceivedQuantity { get; set; }
        public bool? IsClosed { get; set; }
        public bool IsSynced { get; set; }
    }
}
