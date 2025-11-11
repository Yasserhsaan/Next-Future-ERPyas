using System.ComponentModel.DataAnnotations;

namespace Next_Future_ERP.Features.PrintManagement.Models
{
    /// <summary>
    /// محركات التصيير المدعومة
    /// </summary>
    public static class PrintEngine
    {
        public const string Html = "html";
        public const string Jrxml = "jrxml";
        public const string Fr3 = "fr3";
    }
    
    /// <summary>
    /// أحجام الورق الشائعة
    /// </summary>
    public static class PaperSize
    {
        public const string A4 = "A4";
        public const string A5 = "A5";
        public const string Letter = "Letter";
        public const string Legal = "Legal";
        public const string Receipt80mm = "80mm";
        public const string Receipt57mm = "57mm";
    }
    
    /// <summary>
    /// اتجاهات الورق
    /// </summary>
    public static class PaperOrientation
    {
        public const string Portrait = "P";
        public const string Landscape = "L";
    }
    
    /// <summary>
    /// معلومات القالب للعرض في القوائم
    /// </summary>
    public class TemplateInfo
    {
        public int TemplateId { get; set; }
        public int? CompanyId { get; set; }
        public int? BranchId { get; set; }           // قد تكون null
        public int? DocumentTypeId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string DocumentTypeName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? BranchName { get; set; }
        public string Engine { get; set; } = string.Empty;
        public string? Locale { get; set; }
        public bool IsDefault { get; set; }
        public bool Active { get; set; }
        public int ActiveVersionNo { get; set; }
        public DateTime? LastActivatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// معلومات الإصدار للعرض
    /// </summary>
    public class VersionInfo
    {
        public int TemplateVersionId { get; set; }
        public int VersionNo { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public int ContentCount { get; set; }
        public int DataSourceCount { get; set; }
        public bool HasMainDataSource { get; set; }
    }
    
    /// <summary>
    /// معلومات مصدر البيانات مع حالة الاختبار
    /// </summary>
    public class DataSourceInfo
    {
        public int DataSourceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int TimeoutSec { get; set; }
        public bool? ExistsInDatabase { get; set; }
        public bool? TestPassed { get; set; }
        public string? TestError { get; set; }
        public DateTime? LastTested { get; set; }
    }
}
