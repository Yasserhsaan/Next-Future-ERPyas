using Next_Future_ERP.Features.PrintManagement.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.PrintManagement.Services
{
    /// <summary>
    /// واجهة خدمة معاينة القوالب - عرض معاينة للقوالب قبل الطباعة
    /// </summary>
    public interface IRenderPreviewService
    {
        /// <summary>
        /// إنشاء معاينة للقالب مع بيانات تجريبية
        /// </summary>
        /// <param name="templateVersionId">معرف إصدار القالب</param>
        /// <param name="sampleData">البيانات التجريبية (اختيارية)</param>
        /// <returns>معاينة القالب</returns>
        Task<PreviewResult> RenderPreviewAsync(int templateVersionId, Dictionary<string, object>? sampleData = null);

        /// <summary>
        /// إنشاء معاينة للقالب مع بيانات مستند محدد
        /// </summary>
        /// <param name="templateVersionId">معرف إصدار القالب</param>
        /// <param name="documentTypeId">نوع المستند</param>
        /// <param name="documentId">معرف المستند</param>
        /// <returns>معاينة القالب</returns>
        Task<PreviewResult> RenderPreviewWithDocumentAsync(int templateVersionId, int documentTypeId, long documentId);

        /// <summary>
        /// إنشاء معاينة HTML للقالب
        /// </summary>
        /// <param name="templateVersionId">معرف إصدار القالب</param>
        /// <param name="sampleData">البيانات التجريبية</param>
        /// <returns>معاينة HTML</returns>
        Task<string> RenderHtmlPreviewAsync(int templateVersionId, Dictionary<string, object>? sampleData = null);

        /// <summary>
        /// إنشاء معاينة PDF للقالب
        /// </summary>
        /// <param name="templateVersionId">معرف إصدار القالب</param>
        /// <param name="sampleData">البيانات التجريبية</param>
        /// <returns>بيانات PDF</returns>
        Task<byte[]> RenderPdfPreviewAsync(int templateVersionId, Dictionary<string, object>? sampleData = null);

        /// <summary>
        /// الحصول على البيانات التجريبية لنوع مستند محدد
        /// </summary>
        /// <param name="documentTypeId">نوع المستند</param>
        /// <returns>البيانات التجريبية</returns>
        Task<Dictionary<string, object>> GetSampleDataAsync(int documentTypeId);

        /// <summary>
        /// التحقق من صحة القالب
        /// </summary>
        /// <param name="templateVersionId">معرف إصدار القالب</param>
        /// <returns>نتيجة التحقق</returns>
        Task<ValidationResult> ValidateTemplateAsync(int templateVersionId);
    }

    /// <summary>
    /// نتيجة المعاينة
    /// </summary>
    public class PreviewResult
    {
        public bool Success { get; set; }
        public string? HtmlContent { get; set; }
        public byte[]? PdfContent { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan RenderTime { get; set; }
        public Dictionary<string, object>? UsedData { get; set; }
    }

    /// <summary>
    /// نتيجة التحقق من صحة القالب
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
}