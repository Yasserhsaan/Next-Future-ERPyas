using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Accounts.Models
{
    [Keyless] // EF Core
    public class AccountLevelInfo
    {
        public string AccountCategory { get; set; } = "";
        public int CurrentLevel { get; set; }
        public int MaxLevel { get; set; }
        public bool CanCreateChild { get; set; }
        public int DigitsPerLevel { get; set; }
        public string NextChildCode { get; set; } = "";
    }
}
