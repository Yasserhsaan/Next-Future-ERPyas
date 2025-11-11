using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Next_Future_ERP.Features.PrintManagement.Models
{
    /// <summary>
    /// قالب الطباعة - يعبّر عن القالب: شركة/فرع/نوع مستند/لغة/محرك/افتراضي/نشط
    /// </summary>
    public class PrintTemplate
    {
        public int TemplateId { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        /// <summary>
        /// إن NULL يعني عام على كل الفروع
        /// </summary>
        public int? BranchId { get; set; }
        
        [Required]
        public int DocumentTypeId { get; set; }
        
        [Required]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// محرك التصيير: html, jrxml, fr3
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Engine { get; set; } = "html";
        
        /// <summary>
        /// حجم الورق: A4/A5/80mm
        /// </summary>
        [StringLength(10)]
        public string? PaperSize { get; set; }
        
        /// <summary>
        /// اتجاه الورق: P (Portrait) / L (Landscape)
        /// </summary>
        [StringLength(1)]
        public string? Orientation { get; set; }
        
        /// <summary>
        /// اللغة: ar-SA, en
        /// </summary>
        [StringLength(10)]
        public string? Locale { get; set; }
        
        /// <summary>
        /// هل هو القالب الافتراضي لهذا النوع
        /// </summary>
        public bool IsDefault { get; set; } = false;
        
        /// <summary>
        /// هل القالب نشط
        /// </summary>
        public bool Active { get; set; } = true;
        
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation Properties
        public virtual ICollection<TemplateVersion> Versions { get; set; } = new List<TemplateVersion>();
        
        // Reference to DocumentType (from existing models)
        public virtual Next_Future_ERP.Models.DocumentType? DocumentType { get; set; }
    }
}
