using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Models
{
    public sealed class PaymentVoucherLookupItem
    {
        public int VoucherID { get; set; }
        public string DocumentNumber { get; set; } = "";
        public DateTime DocumentDate { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = "";
        public string VoucherType { get; set; } = "Cash";
        public int? SourceId { get; set; }
        public string? SourceName { get; set; }
        public string? Beneficiary { get; set; }
        public decimal LocalAmount { get; set; }
        public int? CashBoxID { get; set; }   // صندوق (FundId)
        public int? BankID { get; set; }      // بنك
        public int CurrencyID { get; set; }
        public decimal? ExchangeRate { get; set; } // decimal(18,6)
    }

}
