using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Models
{
    [Table("AccountCurrencies")]
    public class AccountCurrency
    {
        [Key]
        public int AccountCurrencyId { get; set; }

        [Required]
        public int AccountId { get; set; }

        [Required]
        public int CurrencyId { get; set; }

        public bool? IsStopped { get; set; }
        public DateTime? CreatedAt { get; set; }

        [ForeignKey(nameof(CurrencyId))]
        public virtual NextCurrency? Currency { get; set; }
    }
}

  
