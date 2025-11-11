// CurrencyExchangeRate.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Models
{
    [Table("CurrencyExchangeRates")]
    public class CurrencyExchangeRate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CurrencyId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 6)")]
        public decimal ExchangeRate { get; set; }

        [Required]
        public DateTime DateExchangeStart { get; set; }

        [Required] public DateTime? DateExchangeEnd { get; set; }


        public bool? Status { get; set; }

        // Navigation property
        [ForeignKey("CurrencyId")]
        public virtual NextCurrency Currency { get; set; }
    }
}