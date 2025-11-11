using System;
using System.ComponentModel.DataAnnotations;

namespace Next_Future_ERP.Features.PrintManagement.Models
{
    /// <summary>
    /// أصول الطباعة - اسم/نوع/رابط أو بيانات/بصمة (شعار، ختم، CSS…)
    /// </summary>
    public class PrintAsset
    {
        public int AssetId { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        /// <summary>
        /// إن NULL يعني عام على كل الفروع
        /// </summary>
        public int? BranchId { get; set; }
        
        [Required]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string MimeType { get; set; } = string.Empty;
        
        /// <summary>
        /// رابط خارجي للأصل (اختياري)
        /// </summary>
        [StringLength(500)]
        public string? Url { get; set; }
        
        /// <summary>
        /// بيانات الأصل المُخزنة في قاعدة البيانات
        /// </summary>
        public byte[]? Data { get; set; }
        
        /// <summary>
        /// بصمة المحتوى للتحقق من التغييرات
        /// </summary>
        public byte[]? ContentHash { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// أنواع أصول الطباعة الشائعة
    /// </summary>
    public static class AssetType
    {
        public const string Logo = "logo";
        public const string Stamp = "stamp";
        public const string Signature = "signature";
        public const string Css = "css";
        public const string Image = "image";
        public const string Font = "font";
    }
    
    /// <summary>
    /// أنواع MIME الشائعة
    /// </summary>
    public static class CommonMimeTypes
    {
        public const string ImagePng = "image/png";
        public const string ImageJpeg = "image/jpeg";
        public const string ImageGif = "image/gif";
        public const string ImageSvg = "image/svg+xml";
        public const string TextCss = "text/css";
        public const string FontWoff = "font/woff";
        public const string FontWoff2 = "font/woff2";
        public const string FontTtf = "font/ttf";
    }
}
