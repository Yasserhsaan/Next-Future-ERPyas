using System.ComponentModel.DataAnnotations;

namespace Next_Future_ERP.Features.PrintManagement.Models
{
    /// <summary>
    /// مصدر بيانات القالب - "header/lines/company…"، النوع (view/proc)، الاسم، الرئيسي، المهلة
    /// </summary>
    public class TemplateDataSource
    {
        public int DataSourceId { get; set; }
        
        [Required]
        public int TemplateVersionId { get; set; }
        
        /// <summary>
        /// اسم مصدر البيانات: header, lines, company, etc.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// نوع المصدر: view أو proc
        /// </summary>
        [Required]
        [StringLength(10)]
        public string SourceType { get; set; } = "view";
        
        /// <summary>
        /// اسم الـ View أو Stored Procedure
        /// </summary>
        [Required]
        [StringLength(128)]
        public string SourceName { get; set; } = string.Empty;
        
        /// <summary>
        /// هل هو مصدر البيانات الرئيسي
        /// </summary>
        public bool IsMain { get; set; } = false;
        
        /// <summary>
        /// مهلة التنفيذ بالثواني
        /// </summary>
        public int TimeoutSec { get; set; } = 30;
        
        // Navigation Properties
        public virtual TemplateVersion? TemplateVersion { get; set; }
    }
    
    /// <summary>
    /// أنواع مصادر البيانات
    /// </summary>
    public static class DataSourceType
    {
        public const string View = "view";
        public const string Procedure = "proc";
    }
    
    /// <summary>
    /// أسماء مصادر البيانات الشائعة
    /// </summary>
    public static class DataSourceNames
    {
        public const string Header = "header";
        public const string Lines = "lines";
        public const string Company = "company";
        public const string Branch = "branch";
        public const string Summary = "summary";
        public const string Footer = "footer";
    }
}
