using Microsoft.EntityFrameworkCore;

namespace Next_Future_ERP.Features.Accounts.Models
{
    [Keyless]
    public class CompanyOption
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = "";
    }
}
