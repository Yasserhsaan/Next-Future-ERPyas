using Next_Future_ERP.Features.PurchaseInvoices.Models;

namespace Next_Future_ERP.Features.PurchaseInvoices.Services
{
    public interface IPurchaseAPService
    {
        Task<List<PurchaseAP>> GetAllAsync(string? searchText = null, int? supplierId = null, 
            DateTime? fromDate = null, DateTime? toDate = null, string? docType = null);
        
        Task<PurchaseAP?> GetByIdAsync(long apId);
        
        Task<long> AddAsync(PurchaseAP purchaseAP, IEnumerable<PurchaseAPDetail> details);
        
        Task UpdateAsync(PurchaseAP purchaseAP, IEnumerable<PurchaseAPDetail> details);
        
        Task DeleteAsync(long apId);
        
        Task<string> GenerateNextNumberAsync(int companyId, int branchId, string docType);
        
        Task<bool> PostAsync(long apId, int userId);
        
        Task<bool> UnpostAsync(long apId, int userId);
        
        Task<bool> ValidateAsync(PurchaseAP purchaseAP, IEnumerable<PurchaseAPDetail> details);
        
        Task<List<string>> GetValidationErrorsAsync(PurchaseAP purchaseAP, IEnumerable<PurchaseAPDetail> details);
        
        Task CalculateTotalsAsync(PurchaseAP purchaseAP, IEnumerable<PurchaseAPDetail> details);
        
        Task<List<PurchaseAP>> GetByReceiptIdAsync(long receiptId);
        
        Task<List<PurchaseAP>> GetByPurchaseOrderIdAsync(int purchaseOrderId);
        
        Task<decimal> GetRemainingQuantityAsync(long receiptDetailId);
    }
}
