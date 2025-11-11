using Next_Future_ERP.Features.PrintManagement.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.PrintManagement.Services
{
    /// <summary>
    /// خدمة إدارة إصدارات القوالب - إنشاء إصدار جديد، تغيير الحالة، استرجاع
    /// </summary>
    public interface IVersioningService
    {
        /// <summary>
        /// الحصول على جميع إصدارات قالب
        /// </summary>
        Task<List<VersionInfo>> GetVersionsAsync(int templateId);

        /// <summary>
        /// الحصول على إصدار واحد بالتفاصيل
        /// </summary>
        Task<TemplateVersion?> GetVersionByIdAsync(int versionId);

        /// <summary>
        /// الحصول على الإصدار النشط
        /// </summary>
        Task<TemplateVersion?> GetActiveVersionAsync(int templateId);

        /// <summary>
        /// إنشاء إصدار جديد من الصفر
        /// </summary>
        Task<TemplateVersion> CreateNewVersionAsync(int templateId, string? notes = null);

        /// <summary>
        /// إنشاء إصدار جديد بنسخ من إصدار موجود
        /// </summary>
        Task<TemplateVersion> CreateVersionFromExistingAsync(int templateId, int sourceVersionId, string? notes = null);

        /// <summary>
        /// تفعيل إصدار (يصبح Active ويؤرشف الإصدار النشط السابق)
        /// </summary>
        Task<bool> ActivateVersionAsync(int versionId, string? notes = null);

        /// <summary>
        /// أرشفة إصدار
        /// </summary>
        Task<bool> ArchiveVersionAsync(int versionId, string? notes = null);

        /// <summary>
        /// استرجاع إلى إصدار سابق (تفعيله وأرشفة الحالي)
        /// </summary>
        Task<bool> RevertToVersionAsync(int versionId, string? notes = null);

        /// <summary>
        /// تحديث ملاحظات الإصدار
        /// </summary>
        Task<bool> UpdateVersionNotesAsync(int versionId, string notes);

        /// <summary>
        /// حذف إصدار (فقط إذا كان Draft ولا يحتوي على مهام طباعة)
        /// </summary>
        Task<bool> DeleteVersionAsync(int versionId);

        /// <summary>
        /// التحقق من صحة الإصدار للتفعيل
        /// </summary>
        Task<(bool IsValid, List<string> ValidationErrors)> ValidateVersionForActivationAsync(int versionId);

        /// <summary>
        /// الحصول على آخر رقم إصدار لقالب
        /// </summary>
        Task<int> GetNextVersionNumberAsync(int templateId);
    }
}
