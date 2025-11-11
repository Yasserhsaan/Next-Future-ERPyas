using Next_Future_ERP.Features.PrintManagement.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.PrintManagement.Services
{
    /// <summary>
    /// خدمة فهرس القوالب - استعلام القوالب بالتصفية، إنشاء/تحديث/تفعيل/أرشفة قالب
    /// </summary>
    public interface ITemplateCatalogService
    {
        /// <summary>
        /// استعلام القوالب بالتصفية
        /// </summary>
        Task<List<TemplateInfo>> GetTemplatesAsync(
            int? companyId = null,
            int? branchId = null,
            int? documentTypeId = null,
            string? locale = null,
            string? engine = null,
            bool? isActive = null,
            bool? isDefault = null);

        /// <summary>
        /// الحصول على قالب واحد بالتفاصيل
        /// </summary>
        Task<PrintTemplate?> GetTemplateByIdAsync(int templateId);

        /// <summary>
        /// إنشاء قالب جديد
        /// </summary>
        Task<PrintTemplate> CreateTemplateAsync(PrintTemplate template);

        /// <summary>
        /// تحديث قالب
        /// </summary>
        Task<PrintTemplate> UpdateTemplateAsync(PrintTemplate template);

        /// <summary>
        /// تفعيل/إيقاف قالب
        /// </summary>
        Task<bool> ToggleActiveAsync(int templateId, bool isActive);

        /// <summary>
        /// تعيين قالب كافتراضي
        /// </summary>
        Task<bool> SetDefaultAsync(int templateId);

        /// <summary>
        /// إلغاء تعيين قالب كافتراضي
        /// </summary>
        Task<bool> UnsetDefaultAsync(int templateId);

        /// <summary>
        /// التحقق من القيود - عدم تكرار الافتراضي
        /// </summary>
        Task<bool> CanSetDefaultAsync(int templateId);

        /// <summary>
        /// نسخ قالب
        /// </summary>
        Task<PrintTemplate> DuplicateTemplateAsync(int sourceTemplateId, string newName);

        /// <summary>
        /// حذف قالب (إذا لم يكن له إصدارات نشطة)
        /// </summary>
        Task<bool> DeleteTemplateAsync(int templateId);

        /// <summary>
        /// الحصول على القالب الافتراضي لنوع مستند معين
        /// </summary>
        Task<PrintTemplate?> GetDefaultTemplateAsync(
            int companyId,
            int? branchId,
            int documentTypeId,
            string? locale = null);

        /// <summary>
        /// الحصول على إحصائيات القوالب
        /// </summary>
        Task<Dictionary<string, int>> GetTemplateStatsAsync(int? companyId = null);
    }
}
