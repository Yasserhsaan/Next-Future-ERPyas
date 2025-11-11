using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Next_Future_ERP.Models
{
    using global::Next_Future_ERP.Models;
    // GeneralJournalEntryDetail.cs
  
        [Table("GeneralJournalEntriesDetailed")]
        public class GeneralJournalEntryDetail
        {
            [Key]
            public long DetailId { get; set; }

            [Required]
            public long JournalEntryId { get; set; }

            [Required]
            [MaxLength(20)]
            public string AccountNumber { get; set; }

            public int? CostCenterId { get; set; }

            [MaxLength(500)]
            public string Statement { get; set; }

            [Column(TypeName = "decimal(18, 4)")]
            public decimal? DebitAmount { get; set; }

            [Column(TypeName = "decimal(18, 4)")]
            public decimal? CreditAmount { get; set; }

            [Required]
            public int CurrencyId { get; set; }

            [Required]
            [Column(TypeName = "decimal(18, 6)")]
            public decimal ExchangeRate { get; set; }

            public DateTime? CreatedAt { get; set; }

            // Navigation properties
            [ForeignKey("JournalEntryId")]
            public virtual GeneralJournalEntry JournalEntry { get; set; }

          
        }
}
