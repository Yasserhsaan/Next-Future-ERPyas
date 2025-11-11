using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.InitialSystem.Models
{
  public  class PeriodModel
    {
        [Key]
        public int PeriodId { get; set; }
        public int Year { get; set; }
        public int IsLocked { get; set; }
        public DateTime StartDateOfYear { get; set; }
        public DateTime EndDateOfYear { get; set; }
    }
}
