using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Accounts.Models
{
    public class AccountLevelPrivlige
    {
        [Key]                                          // إن كان هذا هو الـPK
        public int AccountPrivligeId { get; set; }
        public string AccountPrivligeAname { get; set; }
        public string AccountPrivligeEname { get; set; }
        public byte LevelId { get; set; }

     
    }

}
