using Next_Future_ERP.Features.Accounts.Models;
using Next_Future_ERP.Features.InitialSystem.Models;
using Next_Future_ERP.Models;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public interface IReferenceDataService
    {
        /// <summary>
        /// استرجاع الحسابات الفرعية فقط (AccountType = 2)
        /// </summary>
        Task<List<Account>> GetLeafAccountsAsync(int companyId, int branchId);

        /// <summary>
        /// استرجاع مراكز التكلفة
        /// </summary>
        Task<List<CostCenter>> GetCostCentersAsync(int companyId, int branchId);

        /// <summary>
        /// استرجاع العملات
        /// </summary>
        Task<List<NextCurrency>> GetCurrenciesAsync(int companyId);

        /// <summary>
        /// استرجاع عملة الشركة الرئيسية
        /// </summary>
        Task<NextCurrency?> GetCompanyCurrencyAsync(int companyId);

        /// <summary>
        /// التحقق من ربط الحساب بالعملة
        /// </summary>
        Task<bool> IsAccountCurrencyLinkedAsync(int accountId, int currencyId);

        /// <summary>
        /// ربط الحساب بالعملة
        /// </summary>
        Task<bool> LinkAccountCurrencyAsync(int accountId, int currencyId);

        /// <summary>
        /// استرجاع معلومات الحساب بالتفصيل
        /// </summary>
        Task<Account?> GetAccountDetailsAsync(int accountId);

        /// <summary>
        /// البحث عن حساب بالكود
        /// </summary>
        Task<Account?> FindAccountByCodeAsync(string accountCode, int companyId, int branchId);

        /// <summary>
        /// استرجاع جميع الشركات
        /// </summary>
        Task<List<CompanyInfoModel>> GetCompaniesAsync();

        /// <summary>
        /// استرجاع الفروع للشركة المحددة
        /// </summary>
        Task<List<BranchModel>> GetBranchesForCompanyAsync(int companyId);

        /// <summary>
        /// استرجاع الصناديق للفرع المحدد
        /// </summary>
        Task<List<Fund>> GetFundsForBranchAsync(int companyId, int branchId);

        /// <summary>
        /// استرجاع البنوك للفرع المحدد
        /// </summary>
        Task<List<Bank>> GetBanksForBranchAsync(int companyId, int branchId);
    }
}
