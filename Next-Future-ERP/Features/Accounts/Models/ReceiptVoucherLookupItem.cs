using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Models
{
    public class ReceiptVoucherLookupItem
    {
        public int VoucherID { get; set; }
        public string DocumentNumber { get; set; } = "";
        public System.DateTime DocumentDate { get; set; }
        public string BranchName { get; set; } = "";
        public string VoucherType { get; set; } = ""; // Cash / Cheque
        public string SourceName { get; set; } = "";  // صندوق أو بنك
        public string Beneficiary { get; set; } = "";
        public decimal LocalAmount { get; set; }
    }
}
