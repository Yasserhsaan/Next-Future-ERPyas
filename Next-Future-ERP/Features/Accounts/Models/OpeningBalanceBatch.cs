using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Features.Accounts.Models
{
    [Table("OpeningBalanceBatch")]
    public class OpeningBalanceBatch
    {
        [Key]
        public int BatchId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Required]
        public short FiscalYear { get; set; }

        [StringLength(30)]
        public string? DocNo { get; set; }

        [StringLength(50)]
        public string? 
            
            BatchCode { get; set; }

        [Required]
        public DateTime DocDate { get; set; } = DateTime.Today;

        [StringLength(200)]
        public string? Description { get; set; }

        [Required]
        public byte Status { get; set; } = 0; // 0=Draft, 1=Posted

        [Required]
        public int CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? PostedBy { get; set; }
        public DateTime? PostedAt { get; set; }

        // Navigation properties
        [NotMapped]
        public List<OpeningBalanceLine> Lines { get; set; } = new();

        // Display properties
        [NotMapped]
        public string StatusText => Status switch
        {
            0 => "مسودة",
            1 => "مُرحل",
            _ => "غير محدد"
        };

        [NotMapped]
        public bool IsPosted => Status == 1;

        [NotMapped]
        public bool IsDraft => Status == 0;
    }
}
