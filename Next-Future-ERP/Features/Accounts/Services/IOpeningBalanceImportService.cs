using Next_Future_ERP.Features.Accounts.Models;
using System.IO;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public interface IOpeningBalanceImportService
    {
        /// <summary>
        /// تحليل ملف Excel وإرجاع البيانات المستخرجة
        /// </summary>
        Task<OpeningBalanceImportDto> ParseExcelAsync(Stream fileStream, string fileName);

        /// <summary>
        /// إنشاء نموذج Excel للرفع
        /// </summary>
        byte[] CreateExcelTemplate();

        /// <summary>
        /// التحقق من صحة البيانات المستوردة
        /// </summary>
        Task<ValidationResult> ValidateImportDataAsync(OpeningBalanceImportDto importData, int companyId, int branchId);

        /// <summary>
        /// معالجة البيانات المستوردة وإنشاء دفعة
        /// </summary>
        Task<ImportResult> ProcessImportAsync(OpeningBalanceImportDto importData, int userId);

        /// <summary>
        /// تصدير تقرير الأخطاء إلى Excel
        /// </summary>
        byte[] ExportErrorReport(List<string> errors);
    }
}
