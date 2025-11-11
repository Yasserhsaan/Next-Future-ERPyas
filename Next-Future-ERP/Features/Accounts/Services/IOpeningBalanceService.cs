using Next_Future_ERP.Features.Accounts.Models;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public interface IOpeningBalanceService
    {
        /// <summary>
        /// إنشاء أو تحديث دفعة أرصدة افتتاحية كمسودة
        /// </summary>
        Task<int> CreateOrUpdateDraftAsync(OpeningBalanceBatch batch, List<OpeningBalanceLine> lines);

        /// <summary>
        /// ترحيل دفعة الأرصدة الافتتاحية
        /// </summary>
        Task<bool> PostBatchAsync(int batchId, int userId);

        /// <summary>
        /// استرجاع دفعة بالتفصيل
        /// </summary>
        Task<OpeningBalanceBatch?> GetBatchAsync(int batchId);

        /// <summary>
        /// استرجاع سطور الدفعة
        /// </summary>
        Task<List<OpeningBalanceLine>> GetBatchLinesAsync(int batchId);

        /// <summary>
        /// البحث عن الدفعات بالفلاتر
        /// </summary>
        Task<List<OpeningBalanceBatch>> SearchBatchesAsync(OpeningBalanceSearchFilter filter);

        /// <summary>
        /// حذف مسودة (إذا لم تُرحل)
        /// </summary>
        Task<bool> DeleteDraftAsync(int batchId);

        /// <summary>
        /// التحقق من صحة دفعة قبل الترحيل
        /// </summary>
        Task<ValidationResult> ValidateBatchForPostingAsync(int batchId);

        /// <summary>
        /// توليد رقم مستند جديد
        /// </summary>
        Task<string> GenerateDocNumberAsync(int companyId, int branchId, short fiscalYear);

        /// <summary>
        /// التحقق من وجود أرصدة افتتاحية للسنة المالية
        /// </summary>
        Task<bool> HasOpeningBalancesForYearAsync(int companyId, int branchId, short fiscalYear);
    }

    public class OpeningBalanceSearchFilter
    {
        public int? CompanyId { get; set; }
        public int? BranchId { get; set; }
        public short? FiscalYear { get; set; }
        public byte? Status { get; set; } // 0=Draft, 1=Posted
        public string? DocNo { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? CreatedBy { get; set; }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}
