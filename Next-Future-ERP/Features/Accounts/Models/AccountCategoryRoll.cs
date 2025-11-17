using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Models
{
   


        [Keyless]
        public class AccountCategoryRoll
        {
            public string CategoryKey { get; set; } = string.Empty;
            public string CategoryNameEn { get; set; } = string.Empty;
            public string CategoryNameAr { get; set; } = string.Empty;
            public string? CategoryType { get; set; }

            public string Display => $"{CategoryKey} - {CategoryNameAr} / {CategoryNameEn}";
        }
    
}
