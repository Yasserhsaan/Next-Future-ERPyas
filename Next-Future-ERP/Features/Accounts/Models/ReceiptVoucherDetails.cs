using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Next_Future_ERP.Models
{
    public class ReceiptVoucherDetail
    {
        [Key]
        public int DetailID { get; set; }

        public int VoucherID { get; set; }
        public int AccountID { get; set; }
        public int? CostCenterID { get; set; }

        public decimal? DebitCurncy { get; set; }       // الأجنبي
        public decimal? CreditCurncy { get; set; }
        public decimal? DebitCompCurncy { get; set; }   // المحلّي
        public decimal? CrediComptCurncy { get; set; }  // (نفس تسمية الجدول)

        public string? Description { get; set; }
    }
}

