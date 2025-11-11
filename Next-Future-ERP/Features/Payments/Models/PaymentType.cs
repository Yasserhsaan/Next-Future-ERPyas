using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Payments.Models
{
    public class PaymentType
    {
        public byte TypeId { get; set; }        // Identity tinyint
        public string Code { get; set; } = null!; // char(2)
        public string? Description { get; set; }       // nvarchar(50)
    }
}
