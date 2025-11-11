using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // من أجل [Index]

namespace Next_Future_ERP.Features.Accounts.Models
{
    [Table("Company_Tax_Profile")]
    [Index(nameof(CompanyId))]
    [Index(nameof(BranchId))]
    [Index(nameof(VATRegistrationNumber))]
    [Index(nameof(CreatedAt))]
    public class CompanyTaxProfile
    {
        [Key]
        public int ProfileId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required, StringLength(30)]
        public string VATRegistrationNumber { get; set; } = string.Empty;

        [StringLength(30)]
        public string? BranchVATNumber { get; set; }

        [Column(TypeName = "varbinary(max)")]
        public byte[]? TaxCertificate { get; set; }

        [StringLength(100)]
        public string? TaxOffice { get; set; }

        public byte? TaxpayerType { get; set; }   // tinyint

        [StringLength(20)]
        public string? ActivityCode { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int? BranchId { get; set; }        // جديد
    }
}
