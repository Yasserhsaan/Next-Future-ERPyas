using Next_Future_ERP.Features.Suppliers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Payments.Models
{
    public class PaymentTerm
    {
        public int TermId { get; set; }          // Identity
        public string TermCode { get; set; } = null!; // varchar(20) UNIQUE
        public string TermName { get; set; } = null!; // nvarchar(100)
        public int NetDays { get; set; }
        public decimal? DiscountPercent { get; set; }          // decimal(5,2)
        public int? DiscountDays { get; set; }
        public decimal? LateFeePercent { get; set; }          // decimal(5,2)
        public bool IsActive { get; set; }
    }
}
