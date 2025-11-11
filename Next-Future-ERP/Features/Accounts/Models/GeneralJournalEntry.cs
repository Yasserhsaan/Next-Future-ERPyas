using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Next_Future_ERP.Models
{
    // GeneralJournalEntry.cs
 
   
        [Table("GeneralJournalEntries")]
        public class GeneralJournalEntry
        {
            [Key]
            public long JournalEntryId { get; set; }

            [Required]
            public int CompanyId { get; set; }

            [Required]
            public int BranchId { get; set; }

            [Required]
            public int DocumentTypeId { get; set; }

            [Required]
            [MaxLength(50)]
            public string DocumentNumber { get; set; }

            [Required]
            public DateTime PostingDate { get; set; }

            [MaxLength(50)]
            public string ReferenceNumber { get; set; }

            [Required]
            [MaxLength(500)]
            public string Description { get; set; }

            [Required]
            [Column(TypeName = "decimal(18, 4)")]
            public decimal TotalDebit { get; set; }

            [Required]
            [Column(TypeName = "decimal(18, 4)")]
            public decimal TotalCredit { get; set; }

            [Required]
            public byte Status { get; set; }

            [Required]
            public int CreatedBy { get; set; }

            public DateTime? CreatedAt { get; set; }

            public int? ModifiedBy { get; set; }

            public DateTime? ModifiedAt { get; set; }

            // Navigation property
            public virtual ICollection<GeneralJournalEntryDetail> Details { get; set; } = new List<GeneralJournalEntryDetail>();
        }
}
