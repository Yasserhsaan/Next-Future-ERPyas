using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Models
{
    public class V_AccountStructureSettingsRow
    {
        public int CompId { get; set; }
        public string CategoryNameAr { get; set; } = "";
        public string CategoryNameEn { get; set; } = "";
        public int StartNumber { get; set; }
        public int AccountNumberLength { get; set; }
        public int SubAccountLevel { get; set; }
        public int NumberLever { get; set; }
        public int AccountNature { get; set; }
    }

}
