using System.ComponentModel.DataAnnotations;

namespace Next_Future_ERP.Features.PrintManagement.Models
{
    /// <summary>
    /// محتوى القالب - نوع المحتوى (html/jrxml/fr3/css)/نصي أو ملف/بصمة
    /// </summary>
    public class TemplateContent
    {
        public int TemplateContentId { get; set; }
        
        [Required]
        public int TemplateVersionId { get; set; }
        
        /// <summary>
        /// نوع المحتوى: html, jrxml, fr3, css
        /// </summary>
        [Required]
        [StringLength(20)]
        public string ContentType { get; set; } = string.Empty;
        
        /// <summary>
        /// المحتوى النصي (للـ HTML/CSS)
        /// </summary>
        public string? ContentText { get; set; }
        
        /// <summary>
        /// المحتوى الثنائي (للملفات مثل JRXML/FR3)
        /// </summary>
        public byte[]? ContentBinary { get; set; }
        
        /// <summary>
        /// بصمة المحتوى للتحقق من التغييرات
        /// </summary>
        public byte[]? ContentHash { get; set; }
        
        // Navigation Properties
        public virtual TemplateVersion? TemplateVersion { get; set; }
    }
    
    /// <summary>
    /// أنواع محتوى القالب
    /// </summary>
    public static class TemplateContentType
    {
        public const string Html = "html";
        public const string Jrxml = "jrxml";
        public const string Fr3 = "fr3";
        public const string Css = "css";
    }
}
