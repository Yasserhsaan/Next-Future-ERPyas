using Next_Future_ERP.Features.Suppliers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Payments.Models
{
    using Next_Future_ERP.Features.Payments.Models;

    namespace Next_Future_ERP.Features.Payments.Models
    {
        public class PaymentMethod
        {
            public int MethodId { get; set; }        // Identity
            public string MethodName { get; set; } = null!;
            public string GLAccount { get; set; } = null!;
            public byte PaymentTypeId { get; set; }        // FK -> Payment_Types.Type_ID
            public int? ProviderId { get; set; }
            public bool? RequiresApproval { get; set; }
            public bool? IsActive { get; set; }
            public bool? SupportsSplit { get; set; }

            // Navigation (اختياري للعرض)
            public PaymentType? Type { get; set; }
        }
    }

}
