using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.Accounts.Models;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public class OpeningBalanceService : IOpeningBalanceService
    {
        private readonly AppDbContext _context;
        private readonly IReferenceDataService _referenceDataService;

        public OpeningBalanceService(IReferenceDataService referenceDataService)
        {
            _context = DbContextFactory.Create();
            _referenceDataService = referenceDataService;
        }

        public async Task<int> CreateOrUpdateDraftAsync(OpeningBalanceBatch batch, List<OpeningBalanceLine> lines)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                OpeningBalanceBatch savedBatch;

                if (batch.BatchId == 0) // إنشاء جديد
                {
                    batch.CreatedAt = DateTime.UtcNow;
                    batch.Status = 0; // مسودة
                    
                    if (string.IsNullOrEmpty(batch.DocNo))
                    {
                        batch.DocNo = await GenerateDocNumberAsync(batch.CompanyId, batch.BranchId, batch.FiscalYear);
                    }

                    _context.OpeningBalanceBatches.Add(batch);
                    await _context.SaveChangesAsync();
                    savedBatch = batch;
                }
                else // تحديث موجود
                {
                    savedBatch = await _context.OpeningBalanceBatches
                        .FirstOrDefaultAsync(b => b.BatchId == batch.BatchId);

                if (savedBatch == null || savedBatch.IsPosted)
                    throw new InvalidOperationException("لا يمكن تعديل دفعة مُرحلة أو غير موجودة");

                    // تحديث بيانات الرأس
                    savedBatch.DocDate = batch.DocDate;
                    savedBatch.Description = batch.Description;
                    
                    // حذف السطور القديمة
                    var existingLines = await _context.OpeningBalanceLines
                        .Where(l => l.BatchId == batch.BatchId)
                        .ToListAsync();
                    _context.OpeningBalanceLines.RemoveRange(existingLines);
                }

                // إضافة السطور الجديدة
                foreach (var line in lines)
                {
                    line.BatchId = savedBatch.BatchId;
                    line.LineId = 0; // سيتم توليد رقم جديد
                    _context.OpeningBalanceLines.Add(line);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return savedBatch.BatchId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                MessageBox.Show($"❌ خطأ في حفظ دفعة الأرصدة الافتتاحية:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public async Task<bool> PostBatchAsync(int batchId, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // استرجاع الدفعة والسطور
                var batch = await GetBatchAsync(batchId);
                if (batch == null || batch.Status == 1)
                    throw new InvalidOperationException("الدفعة غير موجودة أو مُرحلة مسبقاً");

                var lines = await GetBatchLinesAsync(batchId);
                if (!lines.Any())
                    throw new InvalidOperationException("لا توجد سطور للترحيل");

                // التحقق من صحة البيانات
                var validation = await ValidateBatchForPostingAsync(batchId);
                if (!validation.IsValid)
                    throw new InvalidOperationException($"بيانات غير صحيحة للترحيل:\n{string.Join("\n", validation.Errors)}");

                // ربط العملات بالحسابات تلقائياً
                await LinkCurrenciesToAccountsAsync(lines);

                // تحديث AccountBalances
                await UpdateAccountBalancesAsync(batch, lines);

                // تحديث حالة الدفعة
                var batchEntity = await _context.OpeningBalanceBatches
                    .FirstOrDefaultAsync(b => b.BatchId == batchId);
                
                batchEntity!.Status = 1; // مُرحل
                batchEntity.PostedBy = userId;
                batchEntity.PostedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                MessageBox.Show($"❌ خطأ في ترحيل دفعة الأرصدة الافتتاحية:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<OpeningBalanceBatch?> GetBatchAsync(int batchId)
        {
            try
            {
                return await _context.OpeningBalanceBatches
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.BatchId == batchId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع الدفعة:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<List<OpeningBalanceLine>> GetBatchLinesAsync(int batchId)
        {
            try
            {
                var query = from line in _context.OpeningBalanceLines
                           join account in _context.Accounts on line.AccountId equals account.AccountId into accGroup
                           from acc in accGroup.DefaultIfEmpty()
                           join costCenter in _context.CostCenter on line.CostCenterId equals costCenter.CostCenterId into ccGroup
                           from cc in ccGroup.DefaultIfEmpty()
                           join txnCurrency in _context.NextCurrencies on line.TransactionCurrencyId equals txnCurrency.CurrencyId into tcGroup
                           from tc in tcGroup.DefaultIfEmpty()
                           join compCurrency in _context.NextCurrencies on line.CompanyCurrencyId equals compCurrency.CurrencyId into ccurrGroup
                           from ccurr in ccurrGroup.DefaultIfEmpty()
                           where line.BatchId == batchId
                           select new OpeningBalanceLine
                           {
                               LineId = line.LineId,
                               BatchId = line.BatchId,
                               AccountId = line.AccountId,
                               TransactionCurrencyId = line.TransactionCurrencyId,
                               TransactionDebit = line.TransactionDebit,
                               TransactionCredit = line.TransactionCredit,
                               CompanyCurrencyId = line.CompanyCurrencyId,
                               CompanyDebit = line.CompanyDebit,
                               CompanyCredit = line.CompanyCredit,
                               ExchangeRate = line.ExchangeRate,
                               Note = line.Note,
                               CostCenterId = line.CostCenterId,
                               // بيانات إضافية للعرض
                               AccountCode = acc.AccountCode,
                               AccountNameAr = acc.AccountNameAr,
                               UsesCostCenter = acc.UsesCostCenter ?? false,
                               CostCenterName = cc.CostCenterName,
                               TransactionCurrencyName = tc.CurrencyNameAr,
                               CompanyCurrencyName = ccurr.CurrencyNameAr
                           };

                return await query.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع سطور الدفعة:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<OpeningBalanceLine>();
            }
        }

        public async Task<List<OpeningBalanceBatch>> SearchBatchesAsync(OpeningBalanceSearchFilter filter)
        {
            try
            {
                var query = _context.OpeningBalanceBatches.AsQueryable();

                if (filter.CompanyId.HasValue)
                    query = query.Where(b => b.CompanyId == filter.CompanyId.Value);

                if (filter.BranchId.HasValue)
                    query = query.Where(b => b.BranchId == filter.BranchId.Value);

                if (filter.FiscalYear.HasValue)
                    query = query.Where(b => b.FiscalYear == filter.FiscalYear.Value);

                if (filter.Status.HasValue)
                    query = query.Where(b => b.Status == filter.Status.Value);

                if (!string.IsNullOrEmpty(filter.DocNo))
                    query = query.Where(b => b.DocNo!.Contains(filter.DocNo));

                if (filter.DateFrom.HasValue)
                    query = query.Where(b => b.DocDate >= filter.DateFrom.Value);

                if (filter.DateTo.HasValue)
                    query = query.Where(b => b.DocDate <= filter.DateTo.Value);

                if (filter.CreatedBy.HasValue)
                    query = query.Where(b => b.CreatedBy == filter.CreatedBy.Value);

                return await query
                    .OrderByDescending(b => b.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في البحث عن الدفعات:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<OpeningBalanceBatch>();
            }
        }

        public async Task<bool> DeleteDraftAsync(int batchId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var batch = await _context.OpeningBalanceBatches
                    .FirstOrDefaultAsync(b => b.BatchId == batchId);

                if (batch == null)
                    return false;

                if (batch.Status == 1)
                    throw new InvalidOperationException("لا يمكن حذف دفعة مُرحلة");

                // حذف السطور أولاً
                var lines = await _context.OpeningBalanceLines
                    .Where(l => l.BatchId == batchId)
                    .ToListAsync();
                _context.OpeningBalanceLines.RemoveRange(lines);

                // حذف الدفعة
                _context.OpeningBalanceBatches.Remove(batch);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                MessageBox.Show($"❌ خطأ في حذف الدفعة:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<ValidationResult> ValidateBatchForPostingAsync(int batchId)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                var batch = await GetBatchAsync(batchId);
                var lines = await GetBatchLinesAsync(batchId);

                if (batch == null)
                {
                    result.Errors.Add("الدفعة غير موجودة");
                    result.IsValid = false;
                    return result;
                }

                if (batch.Status == 1)
                {
                    result.Errors.Add("الدفعة مُرحلة مسبقاً");
                    result.IsValid = false;
                    return result;
                }

                if (!lines.Any())
                {
                    result.Errors.Add("لا توجد سطور في الدفعة");
                    result.IsValid = false;
                    return result;
                }

                // التحقق من توازن المدين والدائن
                var totalCompanyDebit = lines.Sum(l => l.CompanyDebit);
                var totalCompanyCredit = lines.Sum(l => l.CompanyCredit);

                if (Math.Abs(totalCompanyDebit - totalCompanyCredit) > 0.01m)
                {
                    result.Errors.Add($"عدم توازن في المبالغ: المدين = {totalCompanyDebit:N4}, الدائن = {totalCompanyCredit:N4}");
                    result.IsValid = false;
                }

                // التحقق من صحة كل سطر
                foreach (var line in lines)
                {
                    if (!line.IsValid)
                    {
                        result.Errors.Add($"بيانات غير صحيحة في السطر {lines.IndexOf(line) + 1}");
                        result.IsValid = false;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"خطأ في التحقق: {ex.Message}");
                result.IsValid = false;
            }

            return result;
        }

        public async Task<string> GenerateDocNumberAsync(int companyId, int branchId, short fiscalYear)
        {
            try
            {
                var lastDoc = await _context.OpeningBalanceBatches
                    .Where(b => b.CompanyId == companyId && 
                               b.BranchId == branchId && 
                               b.FiscalYear == fiscalYear)
                    .OrderByDescending(b => b.BatchId)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                var nextNumber = 1;
                if (lastDoc != null && !string.IsNullOrEmpty(lastDoc.DocNo))
                {
                    // محاولة استخراج الرقم من آخر مستند
                    var parts = lastDoc.DocNo.Split('-');
                    if (parts.Length > 0 && int.TryParse(parts.Last(), out var number))
                    {
                        nextNumber = number + 1;
                    }
                }

                return $"OB-{fiscalYear}-{nextNumber:D4}";
            }
            catch
            {
                return $"OB-{fiscalYear}-{DateTime.Now.Ticks.ToString().Substring(10)}";
            }
        }

        public async Task<bool> HasOpeningBalancesForYearAsync(int companyId, int branchId, short fiscalYear)
        {
            try
            {
                return await _context.OpeningBalanceBatches
                    .AnyAsync(b => b.CompanyId == companyId && 
                                  b.BranchId == branchId && 
                                  b.FiscalYear == fiscalYear &&
                                  b.Status == 1); // مُرحل فقط
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في التحقق من الأرصدة الافتتاحية:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async Task LinkCurrenciesToAccountsAsync(List<OpeningBalanceLine> lines)
        {
            foreach (var line in lines)
            {
                // ربط عملة المعاملة بالحساب إذا لم تكن مربوطة
                if (!await _referenceDataService.IsAccountCurrencyLinkedAsync(line.AccountId, line.TransactionCurrencyId))
                {
                    await _referenceDataService.LinkAccountCurrencyAsync(line.AccountId, line.TransactionCurrencyId);
                }

                // ربط عملة الشركة بالحساب إذا لم تكن مربوطة
                if (line.TransactionCurrencyId != line.CompanyCurrencyId &&
                    !await _referenceDataService.IsAccountCurrencyLinkedAsync(line.AccountId, line.CompanyCurrencyId))
                {
                    await _referenceDataService.LinkAccountCurrencyAsync(line.AccountId, line.CompanyCurrencyId);
                }
            }
        }

        private async Task UpdateAccountBalancesAsync(OpeningBalanceBatch batch, List<OpeningBalanceLine> lines)
        {
            // تجميع السطور حسب الحساب ومركز التكلفة
            var groupedLines = lines
                .GroupBy(l => new { l.AccountId, l.CostCenterId })
                .ToList();

            foreach (var group in groupedLines)
            {
                var totalCompanyDebit = group.Sum(l => l.CompanyDebit);
                var totalCompanyCredit = group.Sum(l => l.CompanyCredit);

                // البحث عن رصيد موجود
                var existingBalance = await _context.AccountBalances
                    .FirstOrDefaultAsync(ab => ab.CompanyId == batch.CompanyId &&
                                              ab.BranchId == batch.BranchId &&
                                              ab.AccountId == group.Key.AccountId &&
                                              ab.CurrencyId == lines.First().CompanyCurrencyId &&
                                              ab.PeriodType == 2 && // سنوي
                                              ab.FiscalYear == batch.FiscalYear &&
                                              ab.CostCenterId == group.Key.CostCenterId);

                if (existingBalance != null)
                {
                    // تحديث الرصيد الموجود
                    existingBalance.OpeningDebit += totalCompanyDebit;
                    existingBalance.OpeningCredit += totalCompanyCredit;
                    existingBalance.ClosingDebit = existingBalance.OpeningDebit + existingBalance.PeriodDebit;
                    existingBalance.ClosingCredit = existingBalance.OpeningCredit + existingBalance.PeriodCredit;
                    existingBalance.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // إنشاء رصيد جديد
                    var newBalance = new Models.AccountBalance
                    {
                        CompanyId = batch.CompanyId,
                        BranchId = batch.BranchId,
                        AccountId = group.Key.AccountId,
                        CurrencyId = lines.First().CompanyCurrencyId,
                        PeriodType = 2, // سنوي
                        FiscalYear = batch.FiscalYear,
                        FiscalMonth = null,
                        FiscalDay = null,
                        OpeningDebit = totalCompanyDebit,
                        OpeningCredit = totalCompanyCredit,
                        PeriodDebit = 0,
                        PeriodCredit = 0,
                        ClosingDebit = totalCompanyDebit,
                        ClosingCredit = totalCompanyCredit,
                        UpdatedAt = DateTime.UtcNow,
                        CostCenterId = group.Key.CostCenterId
                    };

                    _context.AccountBalances.Add(newBalance);
                }
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
