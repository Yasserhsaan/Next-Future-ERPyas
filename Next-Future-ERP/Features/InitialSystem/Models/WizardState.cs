using Next_Future_ERP.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.InitialSystem.Models
{
    public class WizardState
{
        public string? SelectedConnectionType { get; set; }
        public DatabaseConnectionModel DatabaseConnection { get; set; } = new();
        public CompanyInfoModel CompanyInfo { get; set; } = new();
        public BranchModel Branches { get; set; } = new();
        public AccountingSetupModel AccountingSetup { get; set; } = new();
        public FinancialPeriodsSettingModlel FinancialPeriods { get; set; } = new();
        public Nextuser AdminUser { get; set; } = new();
}
}
