using Next_Future_ERP.Features.Purchases.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Purchases.Services
{
    public interface IPurchaseTxnsService
    {
        Task<List<PurchaseTxn>> GetAllAsync(char txnType, string? q = null, int? supplierId = null, DateTime? from = null, DateTime? to = null);
        Task<PurchaseTxn?> GetByIdAsync(int id);
        Task<int> AddAsync(PurchaseTxn head, IEnumerable<PurchaseTxnDetail> details);
        Task UpdateAsync(PurchaseTxn head, IEnumerable<PurchaseTxnDetail> details);
        Task DeleteAsync(int id);

        Task<string> GenerateNextNumberAsync(int companyId, int branchId, char txnType);

        Task<List<PurchaseTxn>> GetApprovedOrdersAsync();

        // تغيير حالة المستند
        Task<bool> ChangeStatusAsync(int txnId, byte newStatus);
        Task<bool> PostAsync(int txnId);      // ترحيل: 0 -> 1
        Task<bool> ApproveAsync(int txnId);   // اعتماد: 1 -> 2
        Task<bool> CancelAsync(int txnId);    // إلغاء: أي حالة -> 9

    }
}
