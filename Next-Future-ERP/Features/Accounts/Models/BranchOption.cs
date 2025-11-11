using Microsoft.EntityFrameworkCore;

namespace Next_Future_ERP.Features.Accounts.Models
{
    [Keyless]
    public class BranchOption
    {
        public int BranchId { get; set; }
        public int CompanyId { get; set; }
        public string BranchName { get; set; } = "";
    }
}
