using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.StoreReceipts.Models;

namespace Next_Future_ERP.Features.StoreReceipts.Services
{
    public class StoreReceiptsService : IStoreReceiptsService
    {
        private readonly AppDbContext _db;

        public StoreReceiptsService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<StoreReceipt>> GetAllAsync(string? searchText = null, int? supplierId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _db.Set<StoreReceipt>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var search = searchText.Trim();
                query = query.Where(x => x.ReceiptNumber.Contains(search) || 
                                        (x.ReferenceNumber ?? "").Contains(search) ||
                                        (x.Description ?? "").Contains(search));
            }

            if (supplierId.HasValue)
                query = query.Where(x => x.SupplierId == supplierId.Value);

            if (fromDate.HasValue)
                query = query.Where(x => x.ReceiptDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.ReceiptDate <= toDate.Value);

            var receipts = await query
                .OrderByDescending(x => x.ReceiptDate)
                .ThenByDescending(x => x.ReceiptId)
                .ToListAsync();

            // تصحيح لرؤية السندات المحملة
            System.Diagnostics.Debug.WriteLine($"StoreReceiptsService.GetAllAsync - Parameters: searchText='{searchText}', supplierId={supplierId}, fromDate={fromDate}, toDate={toDate}");
            System.Diagnostics.Debug.WriteLine($"StoreReceiptsService.GetAllAsync - Total receipts: {receipts.Count}");
            foreach (var receipt in receipts)
            {
                System.Diagnostics.Debug.WriteLine($"Receipt: {receipt.ReceiptNumber}, Status: {receipt.Status}, SupplierId: {receipt.SupplierId}, Date: {receipt.ReceiptDate:yyyy-MM-dd}");
            }

            // تحميل أسماء الموردين
            foreach (var receipt in receipts.Where(r => r.SupplierId.HasValue))
            {
                var supplier = await _db.Suppliers
                    .Where(s => s.SupplierID == receipt.SupplierId.Value)
                    .Select(s => new { s.SupplierName })
                    .FirstOrDefaultAsync();
                
                if (supplier != null)
                    receipt.SupplierName = supplier.SupplierName;
            }

            return receipts;
        }

        public async Task<StoreReceipt?> GetByIdAsync(long id)
        {
            var receipt = await _db.Set<StoreReceipt>()
                .Include(x => x.Details)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ReceiptId == id);

            if (receipt != null && receipt.SupplierId.HasValue)
            {
                var supplier = await _db.Suppliers
                    .Where(s => s.SupplierID == receipt.SupplierId.Value)
                    .Select(s => new { s.SupplierName })
                    .FirstOrDefaultAsync();
                
                if (supplier != null)
                    receipt.SupplierName = supplier.SupplierName;
            }

            return receipt;
        }

        public async Task<long> AddAsync(StoreReceipt receipt, IEnumerable<StoreReceiptDetailed> details)
        {
            await NormalizeReceiptAsync(receipt);
            ValidateReceipt(receipt, details);
            CalculateTotals(receipt, details);

            var strategy = _db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    await _db.Set<StoreReceipt>().AddAsync(receipt);
                    await _db.SaveChangesAsync();

                    var detailList = details.ToList();
                    foreach (var detail in detailList)
                    {
                        detail.ReceiptId = receipt.ReceiptId;
                        CalculateDetailTotals(detail);
                    }

                    await _db.Set<StoreReceiptDetailed>().AddRangeAsync(detailList);
                    await _db.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return receipt.ReceiptId;
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    var root = ex.GetBaseException().Message;
                    throw new InvalidOperationException($"فشل الحفظ: {root}", ex);
                }
            });
        }

        public async Task UpdateAsync(StoreReceipt receipt, IEnumerable<StoreReceiptDetailed> details)
        {
            if (receipt.ReceiptId <= 0)
                throw new InvalidOperationException("لا يمكن التعديل بدون ReceiptId.");

            await NormalizeReceiptAsync(receipt);
            CalculateTotals(receipt, details);

            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // تحديث الرأس
                    await _db.Set<StoreReceipt>()
                        .Where(x => x.ReceiptId == receipt.ReceiptId)
                        .ExecuteUpdateAsync(set => set
                            .SetProperty(p => p.SupplierId, receipt.SupplierId)
                            .SetProperty(p => p.PurchaseOrderId, receipt.PurchaseOrderId)
                            .SetProperty(p => p.ReceiptDate, receipt.ReceiptDate)
                            .SetProperty(p => p.ReferenceNumber, receipt.ReferenceNumber)
                            .SetProperty(p => p.Description, receipt.Description)
                            .SetProperty(p => p.TotalAmount, receipt.TotalAmount)
                            .SetProperty(p => p.CurrencyId, receipt.CurrencyId)
                            .SetProperty(p => p.ExchangeRate, receipt.ExchangeRate)
                            .SetProperty(p => p.Status, receipt.Status)
                            .SetProperty(p => p.ModifiedAt, DateTime.Now)
                            .SetProperty(p => p.ModifiedBy, receipt.ModifiedBy)
                        );

                    // حذف التفاصيل القديمة وإضافة الجديدة
                    await _db.Set<StoreReceiptDetailed>()
                        .Where(d => d.ReceiptId == receipt.ReceiptId)
                        .ExecuteDeleteAsync();

                    var detailList = details.ToList();
                    foreach (var detail in detailList)
                    {
                        detail.ReceiptId = receipt.ReceiptId;
                        CalculateDetailTotals(detail);
                    }

                    await _db.Set<StoreReceiptDetailed>().AddRangeAsync(detailList);
                    await _db.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    var root = ex.GetBaseException().Message;
                    throw new InvalidOperationException($"فشل التحديث: {root}", ex);
                }
            });
        }

        public async Task DeleteAsync(long id)
        {
            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    await _db.Set<StoreReceiptDetailed>()
                        .Where(d => d.ReceiptId == id)
                        .ExecuteDeleteAsync();

                    await _db.Set<StoreReceipt>()
                        .Where(x => x.ReceiptId == id)
                        .ExecuteDeleteAsync();

                    await transaction.CommitAsync();
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    var root = ex.GetBaseException().Message;
                    throw new InvalidOperationException($"فشل الحذف: {root}", ex);
                }
            });
        }

        public async Task<string> GenerateNextNumberAsync(int companyId, int branchId)
        {
            var year = DateTime.Today.Year;
            var prefix = "GRN";

            var lastNumber = await _db.Set<StoreReceipt>()
                .Where(x => x.CompanyId == companyId && x.BranchId == branchId)
                .OrderByDescending(x => x.ReceiptId)
                .Select(x => x.ReceiptNumber)
                .FirstOrDefaultAsync();

            int nextSequence = 1;
            if (!string.IsNullOrWhiteSpace(lastNumber))
            {
                var parts = lastNumber.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[^1], out var sequence))
                    nextSequence = sequence + 1;
            }

            return $"{prefix}-{year}-{nextSequence:D5}";
        }

        public async Task<bool> PostAsync(long id, int userId)
        {
            try
            {
                var receipt = await _db.Set<StoreReceipt>()
                    .FirstOrDefaultAsync(x => x.ReceiptId == id);

                if (receipt == null)
                {
                    System.Diagnostics.Debug.WriteLine($"PostAsync: Receipt with ID {id} not found");
                    return false;
                }

                if (receipt.Status != 0) // يجب أن يكون في حالة مسودة
                {
                    System.Diagnostics.Debug.WriteLine($"PostAsync: Receipt {receipt.ReceiptNumber} status is {receipt.Status}, not 0");
                    return false;
                }

                // تحديث حالة السند إلى مرحل
                receipt.Status = 1;
                receipt.ModifiedBy = userId;
                receipt.ModifiedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                
                System.Diagnostics.Debug.WriteLine($"PostAsync: Successfully posted receipt {receipt.ReceiptNumber}, new status: {receipt.Status}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PostAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UnpostAsync(long id, int userId)
        {
            try
            {
                var receipt = await _db.Set<StoreReceipt>()
                    .FirstOrDefaultAsync(x => x.ReceiptId == id);

                if (receipt == null)
                    return false;

                if (receipt.Status != 1) // يجب أن يكون في حالة مرحل
                    return false;

                // تحديث حالة السند إلى مسودة
                receipt.Status = 0;
                receipt.ModifiedBy = userId;
                receipt.ModifiedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ApproveAsync(long id, int userId)
        {
            try
            {
                var receipt = await _db.Set<StoreReceipt>()
                    .FirstOrDefaultAsync(x => x.ReceiptId == id);

                if (receipt == null)
                {
                    System.Diagnostics.Debug.WriteLine($"ApproveAsync: Receipt with ID {id} not found");
                    return false;
                }

                if (receipt.Status != 1) // يجب أن يكون في حالة مرحل
                {
                    System.Diagnostics.Debug.WriteLine($"ApproveAsync: Receipt {receipt.ReceiptNumber} status is {receipt.Status}, not 1");
                    return false;
                }

                // تحديث حالة السند إلى معتمد
                receipt.Status = 2;
                receipt.ModifiedBy = userId;
                receipt.ModifiedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                
                System.Diagnostics.Debug.WriteLine($"ApproveAsync: Successfully approved receipt {receipt.ReceiptNumber}, new status: {receipt.Status}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApproveAsync error: {ex.Message}");
                return false;
            }
        }

        private async Task NormalizeReceiptAsync(StoreReceipt receipt)
        {
            if (receipt.CompanyId == 0) receipt.CompanyId = 1;
            if (receipt.BranchId == 0) receipt.BranchId = 1;
            if (receipt.CurrencyId == 0) receipt.CurrencyId = 1;
            if (receipt.ExchangeRate == 0) receipt.ExchangeRate = 1;

            if (string.IsNullOrWhiteSpace(receipt.ReceiptNumber))
            {
                receipt.ReceiptNumber = await GenerateNextNumberAsync(receipt.CompanyId, receipt.BranchId);
            }

            if (receipt.ReceiptDate == default)
                receipt.ReceiptDate = DateTime.Today;

            if (receipt.CreatedAt == null)
                receipt.CreatedAt = DateTime.Now;
        }

        private void ValidateReceipt(StoreReceipt receipt, IEnumerable<StoreReceiptDetailed> details)
        {
            if (receipt.CompanyId <= 0)
                throw new InvalidOperationException("CompanyId مطلوب.");

            if (receipt.BranchId <= 0)
                throw new InvalidOperationException("BranchId مطلوب.");

            if (string.IsNullOrWhiteSpace(receipt.ReceiptNumber))
                throw new InvalidOperationException("ReceiptNumber مطلوب.");

            if (receipt.ReceiptDate == default)
                throw new InvalidOperationException("ReceiptDate مطلوب.");

            if (receipt.CurrencyId <= 0)
                throw new InvalidOperationException("CurrencyId مطلوب.");

            if (receipt.ExchangeRate <= 0)
                throw new InvalidOperationException("ExchangeRate يجب أن يكون أكبر من صفر.");

            var detailList = details?.ToList() ?? new List<StoreReceiptDetailed>();
            if (detailList.Count == 0)
                throw new InvalidOperationException("لا توجد تفاصيل.");

            foreach (var detail in detailList)
            {
                if (detail.ItemId <= 0)
                    throw new InvalidOperationException("كل سطر يجب أن يحتوي صنفاً.");

                if (detail.UnitId <= 0)
                    throw new InvalidOperationException("كل سطر يجب أن يحتوي وحدة.");

                if (detail.Quantity <= 0)
                    throw new InvalidOperationException("الكمية يجب أن تكون موجبة.");

                if (detail.UnitPrice < 0)
                    throw new InvalidOperationException("سعر الوحدة لا يقبل السالب.");

                if (detail.WarehouseId <= 0)
                    throw new InvalidOperationException("المستودع مطلوب لكل سطر.");
            }
        }

        private void CalculateTotals(StoreReceipt receipt, IEnumerable<StoreReceiptDetailed> details)
        {
            // إعادة حساب المجاميع لكل سطر
            foreach (var detail in details)
            {
                CalculateDetailTotals(detail);
            }
            
            var total = details.Sum(d => d.TotalPrice ?? 0);
            receipt.TotalAmount = Math.Round(total, 4);
        }

        private void CalculateDetailTotals(StoreReceiptDetailed detail)
        {
            detail.SubTotal = Math.Round(detail.Quantity * detail.UnitPrice, 4);
            
            if (detail.VatRate.HasValue && detail.VatRate > 0)
            {
                detail.VatAmount = Math.Round(detail.SubTotal.Value * (detail.VatRate.Value / 100m), 4);
            }
            else
            {
                detail.VatAmount = 0;
            }

            detail.TotalPrice = Math.Round(detail.SubTotal.Value + detail.VatAmount.Value, 4);
        }
    }
}
