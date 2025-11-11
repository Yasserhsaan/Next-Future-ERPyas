using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.PurchaseInvoices.Models;

namespace Next_Future_ERP.Features.PurchaseInvoices.Services
{
    public class PurchaseAPService : IPurchaseAPService
    {
        private readonly AppDbContext _db;

        public PurchaseAPService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<PurchaseAP>> GetAllAsync(string? searchText = null, int? supplierId = null, 
            DateTime? fromDate = null, DateTime? toDate = null, string? docType = null)
        {
            var query = _db.PurchaseAPs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var s = searchText.Trim();
                query = query.Where(x => x.DocNumber.Contains(s) || 
                                        (x.ReferenceNumber != null && x.ReferenceNumber.Contains(s)) ||
                                        (x.Remarks != null && x.Remarks.Contains(s)));
            }

            if (supplierId.HasValue)
                query = query.Where(x => x.SupplierId == supplierId.Value);

            if (fromDate.HasValue)
                query = query.Where(x => x.DocDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.DocDate <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(docType))
                query = query.Where(x => x.DocType == docType);

            var results = await query
                .OrderByDescending(x => x.DocDate)
                .ThenByDescending(x => x.APId)
                .ToListAsync();

            // تحميل أسماء الموردين
            foreach (var item in results)
            {
                var supplier = await _db.Suppliers
                    .Where(s => s.SupplierID == item.SupplierId)
                    .Select(s => s.SupplierName)
                    .FirstOrDefaultAsync();
                item.SupplierName = supplier;
            }

            return results;
        }

        public async Task<PurchaseAP?> GetByIdAsync(long apId)
        {
            return await _db.PurchaseAPs
                .Include(x => x.Details)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.APId == apId);
        }

        public async Task<long> AddAsync(PurchaseAP purchaseAP, IEnumerable<PurchaseAPDetail> details)
        {
            var strategy = _db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // تعيين القيم الافتراضية
                    await NormalizePurchaseAPAsync(purchaseAP);
                    
                    // حساب المجاميع
                    await CalculateTotalsAsync(purchaseAP, details);
                    
                    // إضافة الرأس
                    _db.PurchaseAPs.Add(purchaseAP);
                    await _db.SaveChangesAsync();
                    
                    // إضافة التفاصيل
                    foreach (var detail in details)
                    {
                        detail.APId = purchaseAP.APId;
                        _db.PurchaseAPDetails.Add(detail);
                    }
                    
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    return purchaseAP.APId;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task UpdateAsync(PurchaseAP purchaseAP, IEnumerable<PurchaseAPDetail> details)
        {
            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // حساب المجاميع
                    await CalculateTotalsAsync(purchaseAP, details);
                    
                    // تحديث الرأس
                    _db.PurchaseAPs.Update(purchaseAP);
                    
                    // حذف التفاصيل القديمة
                    var existingDetails = await _db.PurchaseAPDetails
                        .Where(d => d.APId == purchaseAP.APId)
                        .ToListAsync();
                    _db.PurchaseAPDetails.RemoveRange(existingDetails);
                    
                    // إضافة التفاصيل الجديدة
                    foreach (var detail in details)
                    {
                        detail.APId = purchaseAP.APId;
                        _db.PurchaseAPDetails.Add(detail);
                    }
                    
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task DeleteAsync(long apId)
        {
            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // حذف التفاصيل
                    var details = await _db.PurchaseAPDetails
                        .Where(d => d.APId == apId)
                        .ToListAsync();
                    _db.PurchaseAPDetails.RemoveRange(details);
                    
                    // حذف الرأس
                    var purchaseAP = await _db.PurchaseAPs.FindAsync(apId);
                    if (purchaseAP != null)
                    {
                        _db.PurchaseAPs.Remove(purchaseAP);
                    }
                    
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<string> GenerateNextNumberAsync(int companyId, int branchId, string docType)
        {
            var prefix = docType == "PI" ? "PI" : "PR";
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month.ToString("00");
            
            var lastNumber = await _db.PurchaseAPs
                .Where(x => x.CompanyId == companyId && 
                           x.BranchId == branchId && 
                           x.DocType == docType &&
                           x.DocNumber.StartsWith($"{prefix}-{year}-{month}"))
                .OrderByDescending(x => x.APId)
                .Select(x => x.DocNumber)
                .FirstOrDefaultAsync();
            
            int nextSequence = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var parts = lastNumber.Split('-');
                if (parts.Length >= 4 && int.TryParse(parts[3], out int lastSeq))
                {
                    nextSequence = lastSeq + 1;
                }
            }
            
            return $"{prefix}-{year}-{month}-{nextSequence:00000}";
        }

        public async Task<bool> PostAsync(long apId, int userId)
        {
            var strategy = _db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // قفل المستند لمنع التعديل المتزامن
                    var purchaseAP = await _db.PurchaseAPs
                        .Include(x => x.Details)
                        .FirstOrDefaultAsync(x => x.APId == apId);
                    
                    if (purchaseAP == null || purchaseAP.Status != 1) // يجب أن يكون محفوظ
                        return false;
                    
                    // التحقق من صحة البيانات قبل الترحيل
                    var isValid = await ValidateAsync(purchaseAP, purchaseAP.Details);
                    if (!isValid)
                        return false;
                    
                    // إنشاء القيد المحاسبي حسب السياسة (ب) - الأكثر شيوعاً
                    var journalEntryId = await CreateJournalEntryAsync(purchaseAP);
                    
                    // تحديث حالة المستند
                    purchaseAP.Status = 2; // Posted
                    purchaseAP.JournalEntryId = journalEntryId;
                    purchaseAP.ModifiedBy = userId;
                    purchaseAP.ModifiedAt = DateTime.UtcNow;
                    
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private async Task<long> CreateJournalEntryAsync(PurchaseAP purchaseAP)
        {
            // TODO: تنفيذ إنشاء القيد المحاسبي حسب السياسة (ب)
            // السياسة (ب): مدين GRNI، دائن المورد
            
            // 1. الحصول على حساب المورد
            var supplier = await _db.Suppliers.FindAsync(purchaseAP.SupplierId);
            if (supplier?.AccountID == null)
                throw new InvalidOperationException("المورد بدون حساب محاسبي");
            
            // 2. الحصول على حساب GRNI (من إعدادات الشركة أو افتراضي)
            var grniAccountId = await GetGRNIAccountIdAsync(purchaseAP.CompanyId);
            
            // 3. إنشاء القيد المحاسبي
            // TODO: تنفيذ إنشاء GeneralJournalEntry مع التفاصيل
            
            // مؤقتاً: إرجاع رقم وهمي
            return 999999;
        }

        private async Task<int> GetGRNIAccountIdAsync(int companyId)
        {
            // TODO: الحصول على حساب GRNI من إعدادات الشركة
            // مؤقتاً: إرجاع رقم افتراضي
            return 1001; // حساب GRNI افتراضي
        }

        public async Task<bool> UnpostAsync(long apId, int userId)
        {
            var strategy = _db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // قفل المستند لمنع التعديل المتزامن
                    var purchaseAP = await _db.PurchaseAPs.FindAsync(apId);
                    if (purchaseAP == null || purchaseAP.Status != 2) // يجب أن يكون مرحل
                        return false;
                    
                    // عكس القيد المحاسبي
                    if (purchaseAP.JournalEntryId.HasValue)
                    {
                        await ReverseJournalEntryAsync(purchaseAP.JournalEntryId.Value);
                    }
                    
                    // تحديث حالة المستند
                    purchaseAP.Status = 1; // Saved
                    purchaseAP.JournalEntryId = null;
                    purchaseAP.ModifiedBy = userId;
                    purchaseAP.ModifiedAt = DateTime.UtcNow;
                    
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private async Task ReverseJournalEntryAsync(long journalEntryId)
        {
            // TODO: تنفيذ عكس القيد المحاسبي
            // يمكن إنشاء قيد عكسي أو تعديل القيد الأصلي حسب سياسة الشركة
        }

        public async Task<bool> ValidateAsync(PurchaseAP purchaseAP, IEnumerable<PurchaseAPDetail> details)
        {
            // 1. التحقق من وجود المورد وحسابه
            var supplier = await _db.Suppliers.FindAsync(purchaseAP.SupplierId);
            if (supplier == null || supplier.AccountID == null)
                return false;
            
            // 2. التحقق من عدم تكرار رقم المستند الداخلي
            var existing = await _db.PurchaseAPs
                .AnyAsync(x => x.CompanyId == purchaseAP.CompanyId &&
                              x.BranchId == purchaseAP.BranchId &&
                              x.DocType == purchaseAP.DocType &&
                              x.DocNumber == purchaseAP.DocNumber &&
                              x.APId != purchaseAP.APId);
            if (existing)
                return false;
            
            // 3. التحقق من عدم تكرار رقم فاتورة المورد (للفواتير فقط)
            if (purchaseAP.DocType == "PI" && !string.IsNullOrWhiteSpace(purchaseAP.ReferenceNumber))
            {
                var existingRef = await _db.PurchaseAPs
                    .AnyAsync(x => x.CompanyId == purchaseAP.CompanyId &&
                                  x.SupplierId == purchaseAP.SupplierId &&
                                  x.ReferenceNumber == purchaseAP.ReferenceNumber &&
                                  x.APId != purchaseAP.APId);
                if (existingRef)
                    return false;
            }
            
            // 4. التحقق من التفاصيل الأساسية
            foreach (var detail in details)
            {
                // الكمية والسعر موجبة
                if (detail.Quantity <= 0 || detail.UnitPrice < 0)
                    return false;
                
                // نسبة الضريبة صحيحة
                if (detail.VATRate.HasValue && (detail.VATRate < 0 || detail.VATRate > 100))
                    return false;
            }
            
            // 5. التحقق من المطابقة مع سندات الاستلام (إذا كان مربوط)
            if (purchaseAP.RelatedReceiptId.HasValue)
            {
                var receipt = await _db.StoreReceipts
                    .Include(r => r.Details)
                    .FirstOrDefaultAsync(r => r.ReceiptId == purchaseAP.RelatedReceiptId.Value);
                
                if (receipt == null)
                    return false;
                
                // التحقق من عدم تجاوز الكميات المستلمة
                foreach (var detail in details.Where(d => d.ReceiptDetailId.HasValue))
                {
                    var receiptDetail = receipt.Details.FirstOrDefault(rd => rd.DetailId == detail.ReceiptDetailId);
                    if (receiptDetail == null)
                        return false;
                    
                    // حساب الكمية المفوترة مسبقاً لهذا السطر
                    var invoicedQty = await _db.PurchaseAPDetails
                        .Where(pd => pd.ReceiptDetailId == detail.ReceiptDetailId && 
                                    pd.APId != purchaseAP.APId)
                        .SumAsync(pd => pd.Quantity);
                    
                    if (detail.Quantity + invoicedQty > receiptDetail.Quantity)
                        return false;
                }
            }
            
            return true;
        }

        public async Task<List<string>> GetValidationErrorsAsync(PurchaseAP purchaseAP, IEnumerable<PurchaseAPDetail> details)
        {
            var errors = new List<string>();
            
            // 1. التحقق من وجود المورد وحسابه
            var supplier = await _db.Suppliers.FindAsync(purchaseAP.SupplierId);
            if (supplier == null)
                errors.Add("المورّد غير موجود");
            else if (supplier.AccountID == null)
                errors.Add("المورّد بدون حساب دائن/مدين");
            
            // 2. التحقق من عدم تكرار رقم المستند الداخلي
            var existing = await _db.PurchaseAPs
                .AnyAsync(x => x.CompanyId == purchaseAP.CompanyId &&
                              x.BranchId == purchaseAP.BranchId &&
                              x.DocType == purchaseAP.DocType &&
                              x.DocNumber == purchaseAP.DocNumber &&
                              x.APId != purchaseAP.APId);
            if (existing)
                errors.Add("رقم المستند الداخلي مكرر");
            
            // 3. التحقق من عدم تكرار رقم فاتورة المورد (للفواتير فقط)
            if (purchaseAP.DocType == "PI" && !string.IsNullOrWhiteSpace(purchaseAP.ReferenceNumber))
            {
                var existingRef = await _db.PurchaseAPs
                    .AnyAsync(x => x.CompanyId == purchaseAP.CompanyId &&
                                  x.SupplierId == purchaseAP.SupplierId &&
                                  x.ReferenceNumber == purchaseAP.ReferenceNumber &&
                                  x.APId != purchaseAP.APId);
                if (existingRef)
                    errors.Add("رقم فاتورة المورد مكرر");
            }
            
            // 4. التحقق من التفاصيل الأساسية
            foreach (var detail in details)
            {
                if (detail.Quantity <= 0)
                    errors.Add($"الكمية يجب أن تكون أكبر من صفر في السطر {detail.LineNo}");
                
                if (detail.UnitPrice < 0)
                    errors.Add($"السعر يجب أن يكون أكبر من أو يساوي صفر في السطر {detail.LineNo}");
                
                if (detail.VATRate.HasValue && (detail.VATRate < 0 || detail.VATRate > 100))
                    errors.Add($"نسبة الضريبة غير صحيحة في السطر {detail.LineNo}");
            }
            
            // 5. التحقق من المطابقة مع سندات الاستلام (إذا كان مربوط)
            if (purchaseAP.RelatedReceiptId.HasValue)
            {
                var receipt = await _db.StoreReceipts
                    .Include(r => r.Details)
                    .FirstOrDefaultAsync(r => r.ReceiptId == purchaseAP.RelatedReceiptId.Value);
                
                if (receipt == null)
                    errors.Add("سند الاستلام المرتبط غير موجود");
                else
                {
                    // التحقق من عدم تجاوز الكميات المستلمة
                    foreach (var detail in details.Where(d => d.ReceiptDetailId.HasValue))
                    {
                        var receiptDetail = receipt.Details.FirstOrDefault(rd => rd.DetailId == detail.ReceiptDetailId);
                        if (receiptDetail == null)
                        {
                            errors.Add($"سطر الاستلام غير موجود في السطر {detail.LineNo}");
                            continue;
                        }
                        
                        // حساب الكمية المفوترة مسبقاً لهذا السطر
                        var invoicedQty = await _db.PurchaseAPDetails
                            .Where(pd => pd.ReceiptDetailId == detail.ReceiptDetailId && 
                                        pd.APId != purchaseAP.APId)
                            .SumAsync(pd => pd.Quantity);
                        
                        if (detail.Quantity + invoicedQty > receiptDetail.Quantity)
                            errors.Add($"كمية الفاتورة تتجاوز الكمية المستلمة في السطر {detail.LineNo}");
                    }
                }
            }
            
            return errors;
        }

        public async Task CalculateTotalsAsync(PurchaseAP purchaseAP, IEnumerable<PurchaseAPDetail> details)
        {
            decimal subTotal = 0;
            decimal taxAmount = 0;
            decimal totalAmount = 0;
            
            foreach (var detail in details)
            {
                await CalculateDetailTotalsAsync(detail);
                
                subTotal += detail.TaxableAmount ?? 0;
                taxAmount += detail.VATAmount ?? 0;
                totalAmount += detail.LineTotal ?? 0;
            }
            
            purchaseAP.SubTotal = Math.Round(subTotal, 4);
            purchaseAP.TaxAmount = Math.Round(taxAmount, 4);
            purchaseAP.TotalAmount = Math.Round(totalAmount, 4);
        }

        private async Task CalculateDetailTotalsAsync(PurchaseAPDetail detail)
        {
            // حساب الضريبة بدقة حسب الدليل التنفيذي
            var baseAmount = detail.Quantity * detail.UnitPrice * detail.ExchangeRate;
            
            if (detail.PriceIncludesTax && detail.VATRate.HasValue && detail.VATRate > 0)
            {
                // السعر شامل الضريبة - تقسيم الإجمالي على (1 + النسبة/100) مع تقريب إلى 4 منازل
                var rate = detail.VATRate.Value / 100m;
                detail.TaxableAmount = Math.Round(baseAmount / (1 + rate), 4);
                // الضريبة = الفرق بين الإجمالي والأساس، تُقرّب لخانتين
                detail.VATAmount = Math.Round(baseAmount - detail.TaxableAmount.Value, 2);
            }
            else
            {
                // السعر غير شامل الضريبة
                detail.TaxableAmount = Math.Round(baseAmount, 4);
                if (detail.VATRate.HasValue && detail.VATRate > 0)
                {
                    // الضريبة = تقريب إلى خانتين عشريتين (0.01) من (الأساس × النسبة/100)
                    detail.VATAmount = Math.Round(detail.TaxableAmount.Value * (detail.VATRate.Value / 100m), 2);
                }
                else
                {
                    detail.VATAmount = 0;
                }
            }
            
            detail.LineTotal = Math.Round(detail.TaxableAmount.Value + detail.VATAmount.Value, 4);
        }

        private async Task NormalizePurchaseAPAsync(PurchaseAP purchaseAP)
        {
            if (purchaseAP.CompanyId == 0) purchaseAP.CompanyId = 1;
            if (purchaseAP.BranchId == 0) purchaseAP.BranchId = 1;
            if (purchaseAP.CurrencyId == 0) purchaseAP.CurrencyId = 1;
            if (purchaseAP.ExchangeRate == 0) purchaseAP.ExchangeRate = 1;
            
            if (string.IsNullOrWhiteSpace(purchaseAP.DocNumber))
            {
                purchaseAP.DocNumber = await GenerateNextNumberAsync(purchaseAP.CompanyId, purchaseAP.BranchId, purchaseAP.DocType);
            }
            
            if (purchaseAP.DocDate == default)
                purchaseAP.DocDate = DateTime.Today;
            
            if (purchaseAP.CreatedAt == default)
                purchaseAP.CreatedAt = DateTime.UtcNow;
        }

        public async Task<List<PurchaseAP>> GetByReceiptIdAsync(long receiptId)
        {
            return await _db.PurchaseAPs
                .Where(x => x.RelatedReceiptId == receiptId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<PurchaseAP>> GetByPurchaseOrderIdAsync(int purchaseOrderId)
        {
            return await _db.PurchaseAPs
                .Where(x => x.RelatedPOId == purchaseOrderId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<decimal> GetRemainingQuantityAsync(long receiptDetailId)
        {
            // حساب الكمية المتبقية من سطر الاستلام
            var receiptDetail = await _db.StoreReceiptsDetailed
                .FirstOrDefaultAsync(rd => rd.DetailId == receiptDetailId);
            
            if (receiptDetail == null)
                return 0;
            
            // حساب الكمية المفوترة مسبقاً
            var invoicedQty = await _db.PurchaseAPDetails
                .Where(pd => pd.ReceiptDetailId == receiptDetailId)
                .SumAsync(pd => pd.Quantity);
            
            return receiptDetail.Quantity - invoicedQty;
        }
    }
}
