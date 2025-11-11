// DocumentType.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Models
{
    
    public class DocumentType
    {
        [Key]
        public int DocumentTypeId { get; set; }

        [Required]
        [MaxLength(10)]
        public string DocumentCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string DocumentNameAr { get; set; }

        [MaxLength(100)]
        public string DocumentNameEn { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public int? ModuleId { get; set; }

        [MaxLength(5)]
        public string SequencePrefix { get; set; }

        [Required]
        public bool IsSystem { get; set; } = false;

        public DateTime? CreatedAt { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public int? ModifiedBy { get; set; }
    }
}