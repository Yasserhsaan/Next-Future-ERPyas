using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Next_Future_ERP.Features.PrintManagement.Models
{
    /// <summary>
    /// إصدار القالب - رقم الإصدار/الحالة (draft/active/archived)/تاريخ الإنشاء/التفعيل
    /// </summary>
    public class TemplateVersion
    {
        public int TemplateVersionId { get; set; }
        
        [Required]
        public int TemplateId { get; set; }
        
        [Required]
        public int VersionNo { get; set; }
        
        /// <summary>
        /// حالة الإصدار: draft, active, archived
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Status { get; set; } = "draft";
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ActivatedAt { get; set; }
        
        // Navigation Properties
        public virtual PrintTemplate? Template { get; set; }
        public virtual ICollection<TemplateContent> Contents { get; set; } = new List<TemplateContent>();
        public virtual ICollection<TemplateDataSource> DataSources { get; set; } = new List<TemplateDataSource>();
        public virtual ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();
    }
    
    /// <summary>
    /// حالات الإصدار
    /// </summary>
    public static class TemplateVersionStatus
    {
        public const string Draft = "draft";
        public const string Active = "active";
        public const string Archived = "archived";
    }
}
