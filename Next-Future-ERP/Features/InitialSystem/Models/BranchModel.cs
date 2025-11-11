using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.InitialSystem.Models
{
    public class BranchModel
    {
        [Key]
        public int BranchId { get; set; }
        public int ComiId { get; set; }
        public string BranchName { get; set; }
        public string? Location { get; set; }
        public bool? IsActive { get; set; }

      
      
    }
    
}
