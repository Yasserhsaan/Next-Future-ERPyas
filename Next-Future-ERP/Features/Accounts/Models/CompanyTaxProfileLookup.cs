using System;
using Microsoft.EntityFrameworkCore;

namespace Next_Future_ERP.Features.Accounts.Models
{
    [Keyless]
    public class CompanyTaxProfileLookup
    {
        public int ProfileId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = "";
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }

        public string VATRegistrationNumber { get; set; } = "";
        public string? BranchVATNumber { get; set; }
        public string? TaxOffice { get; set; }
        public byte? TaxpayerType { get; set; }
        public string? ActivityCode { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
