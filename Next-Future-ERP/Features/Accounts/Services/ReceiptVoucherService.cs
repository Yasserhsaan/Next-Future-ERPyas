using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.Accounts.Models;
using Next_Future_ERP.Features.InitialSystem.Models;
using Next_Future_ERP.Models;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public class ReceiptVoucherService
    {
        private readonly AppDbContext _db;

        public ReceiptVoucherService()
        {
            _db = DbContextFactory.Create();
        }

        // ======= قوائم أساسية =======
        public async Task<List<BranchModel>> GetBranchesAsync()
            => await _db.Branches
                        .OrderBy(b => b.BranchName)
                        .AsNoTracking()
                        .ToListAsync();

        // صناديق (قبض فقط أو الاثنين)
        public async Task<List<Fund>> GetCashBoxesAsync()
            => await _db.Funds
                        .Where(f => f.IsActive == true &&
                                    (f.FundType == FundType.ReceiptOnly || f.FundType == FundType.Both))
                        .OrderBy(f => f.FundName)
                        .AsNoTracking()
                        .ToListAsync();

        public async Task<List<Fund>> GetCashBoxesByBranchAsync(int branchId)
            => await _db.Funds
                        .Where(f => f.IsActive == true &&
                                    (f.FundType == FundType.ReceiptOnly || f.FundType == FundType.Both) &&
                                    f.BranchId == branchId)
                        .OrderBy(f => f.FundName)
                        .AsNoTracking()
                        .ToListAsync();

        public async Task<List<Bank>> GetBanksAsync()
            => await _db.Banks
                        .OrderBy(b => b.BankName)
                        .AsNoTracking()
                        .ToListAsync();

        public async Task<List<Bank>> GetBanksByBranchAsync(int branchId)
            => await _db.Banks
                        .Where(b => b.BranchId == branchId)
                        .OrderBy(b => b.BankName)
                        .AsNoTracking()
                        .ToListAsync();

        public async Task<List<NextCurrency>> GetCurrenciesAsync()
            => await _db.NextCurrencies
                        .OrderBy(c => c.CurrencyNameAr)
                        .AsNoTracking()
                        .ToListAsync();

        public async Task<decimal> GetExchangeRateAsync(int currencyId, DateTime? onDate)
        {
            try
            {
                var q = _db.CurrencyExchangeRates.Where(x => x.CurrencyId == currencyId);
                var rate = await q.Select(x => (decimal?)x.ExchangeRate).FirstOrDefaultAsync();
                return rate ?? 1m;
            }
            catch { return 1m; }
        }

        // نوع المستند: RV فقط
        public async Task<DocumentType?> GetRVTypeAsync()
            => await _db.DocumentTypes
                        .Where(d => d.DocumentCode == "RV" && d.IsActive == true)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();

        // ترقيم: بادئة من DocumentTypes ثم 8 أرقام، مع تخزين آخر رقم في DocumentSequences
        public async Task<string> GenerateNextVoucherNumberAsync(int documentTypeId, int branchId)
        {
            // 1) تحديد البادئة من نوع المستند (أولوية: SequencePrefix ثم DocumentCode ثم "RV")
            var docType = await _db.DocumentTypes
                                   .Where(d => d.DocumentTypeId == documentTypeId && d.IsActive == true)
                                   .Select(d => new { d.SequencePrefix, d.DocumentCode })
                                   .FirstOrDefaultAsync()
                           ?? throw new InvalidOperationException("نوع المستند غير موجود أو غير مُفعّل.");

            var prefix = !string.IsNullOrWhiteSpace(docType.SequencePrefix)
                        ? docType.SequencePrefix
                        : (!string.IsNullOrWhiteSpace(docType.DocumentCode) ? docType.DocumentCode : "RV");

            // 2) استخدام المعاملة الجارية إن وُجدت، وإلا فتح معاملة محلّية
            var hasAmbientTx = _db.Database.CurrentTransaction != null;
            IDbContextTransaction? localTx = null;

            try
            {
                if (!hasAmbientTx)
                    localTx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                // 3) جلب/إنشاء صف التسلسل ثم زيادة CurrentNo
                var seq = await _db.DocumentSequences
                                   .SingleOrDefaultAsync(x => x.DocumentTypeId == documentTypeId &&
                                                              x.BranchId == branchId);

                if (seq == null)
                {
                    seq = new DocumentSequence
                    {
                        DocumentTypeId = documentTypeId,
                        BranchId = branchId,
                        CurrentNo = 0
                    };
                    _db.DocumentSequences.Add(seq);
                    await _db.SaveChangesAsync();
                }

                seq.CurrentNo += 1;
                await _db.SaveChangesAsync();

                if (localTx != null) await localTx.CommitAsync();

                // 4) إرجاع الرقم بصيغة PREFIX + 8 أرقام
                return $"{prefix}{seq.CurrentNo:D8}";
            }
            catch
            {
                if (localTx != null) await localTx.RollbackAsync();
                throw;
            }
        }

        // العملات حسب الصندوق/البنك
        public async Task<List<NextCurrency>> GetCurrenciesForCashBoxAsync(int fundId)
        {
            var q = from d in _db.FundCurrencyLimits
                    join c in _db.NextCurrencies on d.CurrencyId equals c.CurrencyId
                    where d.FundId == fundId
                    select c;
            return await q.AsNoTracking().Distinct().OrderBy(c => c.CurrencyNameAr).ToListAsync();
        }

        public async Task<List<NextCurrency>> GetCurrenciesForBankAsync(int bankId)
        {
            var q = from d in _db.BankCurrencyDetails
                    join c in _db.NextCurrencies on d.CurrencyId equals c.CurrencyId
                    where d.BankId == bankId
                    select c;
            return await q.AsNoTracking().Distinct().OrderBy(c => c.CurrencyNameAr).ToListAsync();
        }

        public async Task<List<DocumentType>> GetDocumentTypesAsync()
            => await _db.DocumentTypes.OrderBy(d => d.DocumentTypeId).AsNoTracking().ToListAsync();

        public async Task<List<CostCenter>> GetCostCentersAsync()
            => await _db.CostCenter.OrderBy(c => c.CostCenterName).AsNoTracking().ToListAsync();

      
        public async Task<List<Account>> GetAccountsAsync()
           => await _db.Accounts
           .Where(a => a.AccountType == 2 && a.IsActive == true)
                       .OrderBy(a => a.AccountNameAr)
                       .AsNoTracking()
                       .ToListAsync();

        // ======= CRUD =======
        public async Task<ReceiptVoucher?> GetByIdAsync(int id)
            => await _db.ReceiptVouchers
                        .Include(v => v.Details)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(v => v.VoucherID == id);

        // إنشاء مع ترقيم "آمن" وإعادة محاولة على تعارض DocumentNumber (لو لديك Unique Index)
        public async Task<ReceiptVoucher> CreateAsync(ReceiptVoucher v)
        {
            const int maxRetries = 3;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                using var tx = await _db.Database.BeginTransactionAsync();
                try
                {
                    if (string.IsNullOrWhiteSpace(v.DocumentNumber))
                    {
                        if (v.DocumentTypeID <= 0 || v.BranchID <= 0)
                            throw new InvalidOperationException("DocumentType/Branch مطلوبان لتوليد رقم المستند.");

                        v.DocumentNumber = await GenerateNextVoucherNumberAsync(v.DocumentTypeID, v.BranchID);
                    }

                    v.CreatedAt = DateTime.Now;
                    _db.ReceiptVouchers.Add(v);
                    await _db.SaveChangesAsync();

                    await tx.CommitAsync();
                    return v;
                }
                catch (DbUpdateException ex)
                {
                    await tx.RollbackAsync();

                    // لو السبب تعارض فريد على DocumentNumber — أعد المحاولة بتوليد رقم جديد
                    var msg = ex.InnerException?.Message ?? ex.Message;
                    var looksLikeUnique = msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
                                       || msg.Contains("unique", StringComparison.OrdinalIgnoreCase)
                                       || msg.Contains("IX_", StringComparison.OrdinalIgnoreCase);
                    if (!looksLikeUnique || attempt == maxRetries)
                    {
                        MessageBox.Show($"خطأ أثناء الإضافة:\n{ex.Message}");
                        throw;
                    }

                    // جرّب مرة أخرى
                    v.DocumentNumber = ""; // لنجبر التوليد في الدورة القادمة
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    MessageBox.Show($"خطأ أثناء الإضافة:\n{ex.Message}");
                    throw;
                }
            }

            // نظريًا لن نصل هنا
            throw new InvalidOperationException("تعذّر إنشاء السند بعد عدة محاولات.");
        }

        public async Task UpdateAsync(ReceiptVoucher v)
        {
            try
            {
                var existing = await _db.ReceiptVouchers
                                        .Include(x => x.Details)
                                        .FirstOrDefaultAsync(x => x.VoucherID == v.VoucherID);

                if (existing == null)
                    throw new InvalidOperationException("السند غير موجود.");

                _db.Entry(existing).CurrentValues.SetValues(v);

                foreach (var d in existing.Details.ToList())
                    if (!v.Details.Any(x => x.DetailID == d.DetailID))
                        _db.ReceiptVoucherDetails.Remove(d);

                foreach (var d in v.Details)
                {
                    var target = existing.Details.FirstOrDefault(x => x.DetailID == d.DetailID);
                    if (target == null)
                    {
                        d.VoucherID = existing.VoucherID;
                        _db.ReceiptVoucherDetails.Add(d);
                    }
                    else
                    {
                        _db.Entry(target).CurrentValues.SetValues(d);
                    }
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء التعديل:\n{ex.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(int voucherId)
        {
            try
            {
                var v = await _db.ReceiptVouchers
                                 .Include(x => x.Details)
                                 .FirstOrDefaultAsync(x => x.VoucherID == voucherId);
                if (v == null) return;

                _db.ReceiptVoucherDetails.RemoveRange(v.Details);
                _db.ReceiptVouchers.Remove(v);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء الحذف:\n{ex.Message}");
                throw;
            }
        }

        // ======= البحث / الاستعراض =======
        public async Task<(List<ReceiptVoucherLookupItem> Items, int Total)> SearchAsync(
             int? branchId,
    string? voucherType,   // "Cash" أو "Cheque" أو null
    int? sourceId,      // FundId إذا Cash أو BankId إذا Cheque
    DateTime? dateFrom,
    DateTime? dateTo,
    string? docNo,
    string? beneficiary,
    int skip,
    int take)
        {
            using var db = DbContextFactory.Create();
            var q = db.ReceiptVouchers.AsNoTracking().AsQueryable();

            if (branchId.HasValue) q = q.Where(v => v.BranchID == branchId.Value);
            if (!string.IsNullOrWhiteSpace(voucherType))
                q = q.Where(v => v.VoucherType == voucherType);

            if (sourceId.HasValue)
            {
                if (voucherType == "Cash")
                    q = q.Where(v => v.CashBoxID == sourceId);
                else if (voucherType == "Cheque")
                    q = q.Where(v => v.BankID == sourceId);
            }

            if (dateFrom.HasValue) q = q.Where(v => v.DocumentDate >= dateFrom.Value);
            if (dateTo.HasValue) q = q.Where(v => v.DocumentDate <= dateTo.Value);
            if (!string.IsNullOrWhiteSpace(docNo))
                q = q.Where(v => v.DocumentNumber.Contains(docNo));
            if (!string.IsNullOrWhiteSpace(beneficiary))
                q = q.Where(v => v.Beneficiary.Contains(beneficiary));

            var total = await q.CountAsync();

            var page = await (from v in q
                              join b in db.Branches on v.BranchID equals b.BranchId
                              from f in db.Funds.Where(x => x.FundId == v.CashBoxID).DefaultIfEmpty()
                              from k in db.Banks.Where(x => x.BankId == v.BankID).DefaultIfEmpty()
                              orderby v.DocumentDate descending, v.VoucherID descending
                              select new ReceiptVoucherLookupItem
                              {
                                  VoucherID = v.VoucherID,
                                  DocumentNumber = v.DocumentNumber,
                                  DocumentDate = v.DocumentDate,
                                 
                                  BranchName = b.BranchName,
                                  VoucherType = v.VoucherType,
                                 
                                  SourceName = v.VoucherType == "Cash" ? f.FundName : k.BankName,
                                  Beneficiary = v.Beneficiary,
                                  LocalAmount = v.LocalAmount
                              })
                              .Skip(skip)
                              .Take(take)
                              .ToListAsync();

            return (page, total);
        }
    }
}
