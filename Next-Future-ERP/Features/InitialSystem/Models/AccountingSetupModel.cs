using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.InitialSystem.Models
{
    public class AccountingSetupModel
    {

        [Key]
        public int ComiId { get; set; }
        public int BranchId { get; set; }
        public int FiscalYear { get; set; }
        public int CurrentMonth { get; set; }
        public bool? AutoPosting { get; set; }
        public byte? ChartType { get; set; }
        public string? DefaultCashAccount { get; set; }
        public string? DefaultBankAccount { get; set; }
        public string? DefaultInventoryAccount { get; set; }
        public string? ProfitLossAccount { get; set; }
        public string? LinkedAccounts { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? CreatedAt { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? UpdatedAt { get; set; }
    }
}
