using Next_Future_ERP.Features.PrintManagement.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.PrintManagement.Services
{
    /// <summary>
    /// خدمة إدارة المحتوى - إضافة/تحديث محتوى، فحص سلامة، ربط أصول
    /// </summary>
    public interface IContentService
    {
        /// <summary>
        /// الحصول على محتويات إصدار
        /// </summary>
        Task<List<TemplateContent>> GetVersionContentsAsync(int versionId);

        /// <summary>
        /// إضافة محتوى نصي (HTML/CSS)
        /// </summary>
        Task<TemplateContent> AddTextContentAsync(int versionId, string contentType, string content);

        /// <summary>
        /// إضافة محتوى ثنائي (JRXML/FR3)
        /// </summary>
        Task<TemplateContent> AddBinaryContentAsync(int versionId, string contentType, byte[] content);

        /// <summary>
        /// تحديث محتوى موجود
        /// </summary>
        Task<bool> UpdateContentAsync(int contentId, string? textContent = null, byte[]? binaryContent = null);

        /// <summary>
        /// حذف محتوى
        /// </summary>
        Task<bool> RemoveContentAsync(int contentId);

        /// <summary>
        /// فحص سلامة المحتوى
        /// </summary>
        Task<(bool IsValid, List<string> ValidationErrors)> ValidateContentAsync(int contentId);

        /// <summary>
        /// الحصول على محتوى للتحميل
        /// </summary>
        Task<(byte[] Data, string MimeType, string FileName)?> GetContentForDownloadAsync(int contentId);

        /// <summary>
        /// رفع ملف محتوى
        /// </summary>
        Task<TemplateContent> UploadContentFileAsync(int versionId, string contentType, Stream fileStream, string fileName);
    }
}
