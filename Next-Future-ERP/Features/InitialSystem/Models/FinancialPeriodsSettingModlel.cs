using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.InitialSystem.Models
{
   public class FinancialPeriodsSettingModlel
    {
        [Key]
        public int FinancialSettingId { get; set; }
        public int CompanyId { get; set; }
        public bool AllowFutureDateEntry { get; set; }
        public bool AllowBackDateEntry { get; set; }
        public bool AutoPeriodRollover { get; set; }
        public bool? LockedPeriods { get; set; }
        public int PeriodClosePolicy { get; set; }
        public List<PeriodModel>? GeneratedPeriods { get; set; }
    }
}
