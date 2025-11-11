using System;
using System.ComponentModel.DataAnnotations;

namespace Next_Future_ERP.Features.PrintManagement.Models
{
    /// <summary>
    /// سجل الطباعة - تتبّع الطباعة، مخرجات، حالة
    /// </summary>
    public class PrintJob
    {
        public long JobId { get; set; }
        
        [Required]
        public int TemplateVersionId { get; set; }
        
        [Required]
        public int DocumentTypeId { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        public int? BranchId { get; set; }
        
        [Required]
        public long DocumentId { get; set; }
        
        [StringLength(60)]
        public string? DocumentNumber { get; set; }
        
        [StringLength(10)]
        public string? Locale { get; set; }
        
        public int Copies { get; set; } = 1;
        
        [Required]
        public OutputFormatType OutputFormat { get; set; } = OutputFormatType.Pdf;
        
        /// <summary>
        /// حالة المهمة: queued, rendering, done, error
        /// </summary>
        [Required]
        [StringLength(12)]
        public string Status { get; set; } = "done";
        
        [StringLength(500)]
        public string? FileUrl { get; set; }
        
        /// <summary>
        /// محتوى الملف المُصدّر (اختياري - قد يُحفظ في FileUrl بدلاً من ذلك)
        /// </summary>
        public byte[]? FileBytes { get; set; }
        
        [StringLength(1000)]
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// وقت التصيير بالميلي ثانية
        /// </summary>
        public int? RenderMs { get; set; }
        
        public int? PrintedBy { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public virtual TemplateVersion? TemplateVersion { get; set; }
        public virtual Next_Future_ERP.Models.DocumentType? DocumentType { get; set; }
    }
    
    /// <summary>
    /// حالات مهمة الطباعة
    /// </summary>
    public static class PrintJobStatus
    {
        public const string Queued = "queued";
        public const string Rendering = "rendering";
        public const string Done = "done";
        public const string Error = "error";
    }
    
    /// <summary>
    /// تنسيقات الإخراج
    /// </summary>
    public enum OutputFormatType
    {
        Pdf,
        Png,
        Html,
        Excel
    }
}
