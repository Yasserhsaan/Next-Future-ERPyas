using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Warehouses.Models
{
    public class ValuationGroup
    {
        public int ValuationGroupId { get; set; }        // maps to [ValuationGroup]
        public int CompanyId { get; set; }
        public string ValuationGroupCode { get; set; } = "";
        public string ValuationGroupName { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public int? CostCenterId { get; set; }
    }
}
