using Next_Future_ERP.Features.PrintManagement.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.PrintManagement.Services
{
    /// <summary>
    /// خدمة استيراد وتصدير القوالب - حفظ واستيراد القوالب
    /// </summary>
    public interface ITemplateExportImportService
    {
        /// <summary>
        /// تصدير قالب واحد
        /// </summary>
        Task<byte[]> ExportTemplateAsync(int templateId, ExportOptions options);

        /// <summary>
        /// تصدير عدة قوالب
        /// </summary>
        Task<byte[]> ExportTemplatesAsync(List<int> templateIds, ExportOptions options);

        /// <summary>
        /// استيراد قالب من ملف
        /// </summary>
        Task<ImportResult> ImportTemplateAsync(Stream fileStream, ImportOptions options);

        /// <summary>
        /// معاينة استيراد القالب
        /// </summary>
        Task<ImportPreview> PreviewImportAsync(Stream fileStream);

        /// <summary>
        /// تصدير قالب كـ JSON
        /// </summary>
        Task<string> ExportTemplateAsJsonAsync(int templateId, bool includeAssets = false);

        /// <summary>
        /// استيراد قالب من JSON
        /// </summary>
        Task<ImportResult> ImportTemplateFromJsonAsync(string jsonContent, ImportOptions options);

        /// <summary>
        /// إنشاء نموذج قالب فارغ
        /// </summary>
        Task<byte[]> CreateTemplateTemplateAsync(string templateType = "invoice");

        /// <summary>
        /// التحقق من صحة ملف الاستيراد
        /// </summary>
        Task<(bool IsValid, List<string> Errors)> ValidateImportFileAsync(Stream fileStream);
    }

    /// <summary>
    /// خيارات التصدير
    /// </summary>
    public class ExportOptions
    {
        public bool IncludeAssets { get; set; } = true;
        public bool IncludeAllVersions { get; set; } = false;
        public bool IncludeInactiveVersions { get; set; } = false;
        public string Format { get; set; } = "zip"; // zip, json
        public bool CompressAssets { get; set; } = true;
    }

    /// <summary>
    /// خيارات الاستيراد
    /// </summary>
    public class ImportOptions
    {
        public int TargetCompanyId { get; set; }
        public int? TargetBranchId { get; set; }
        public bool OverwriteExisting { get; set; } = false;
        public string ConflictResolution { get; set; } = "rename"; // rename, skip, overwrite
        public bool ImportAssets { get; set; } = true;
        public bool SetAsDefault { get; set; } = false;
        public bool ActivateAfterImport { get; set; } = false;
        public string? NewTemplateName { get; set; }
    }

    /// <summary>
    /// نتيجة الاستيراد
    /// </summary>
    public class ImportResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<ImportedTemplate> ImportedTemplates { get; set; } = new();
        public List<ImportedAsset> ImportedAssets { get; set; } = new();
        public int TotalProcessed { get; set; }
        public int SuccessfulImports { get; set; }
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// معاينة الاستيراد
    /// </summary>
    public class ImportPreview
    {
        public bool IsValidFile { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<TemplatePreview> Templates { get; set; } = new();
        public List<AssetPreview> Assets { get; set; } = new();
        public string FileFormat { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }

    /// <summary>
    /// معاينة القالب
    /// </summary>
    public class TemplatePreview
    {
        public string Name { get; set; } = string.Empty;
        public string Engine { get; set; } = string.Empty;
        public int VersionCount { get; set; }
        public int ContentCount { get; set; }
        public int DataSourceCount { get; set; }
        public bool HasConflict { get; set; }
        public string? ConflictReason { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
    }

    /// <summary>
    /// معاينة الأصل
    /// </summary>
    public class AssetPreview
    {
        public string Name { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public long Size { get; set; }
        public bool HasConflict { get; set; }
        public string? ConflictReason { get; set; }
    }

    /// <summary>
    /// قالب مُستورد
    /// </summary>
    public class ImportedTemplate
    {
        public int TemplateId { get; set; }
        public string OriginalName { get; set; } = string.Empty;
        public string ImportedName { get; set; } = string.Empty;
        public bool WasRenamed { get; set; }
        public int ImportedVersions { get; set; }
        public string Status { get; set; } = string.Empty; // imported, skipped, failed
    }

    /// <summary>
    /// أصل مُستورد
    /// </summary>
    public class ImportedAsset
    {
        public int AssetId { get; set; }
        public string OriginalName { get; set; } = string.Empty;
        public string ImportedName { get; set; } = string.Empty;
        public bool WasCompressed { get; set; }
        public long OriginalSize { get; set; }
        public long FinalSize { get; set; }
        public string Status { get; set; } = string.Empty; // imported, skipped, failed
    }
}
