using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.InitialSystem.Models; // BranchModel
using Next_Future_ERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Next_Future_ERP.Features.Accounts.Models;

namespace Next_Future_ERP.Features.Accounts.Services
{
    // عنصر مختصر لنتائج البحث/الاستعراض
   

    public class PaymentVoucherService
    {
        // إزالة الـ DbContext المشترك لتجنب مشاكل threading
        // private readonly AppDbContext _db;
        // private readonly INumberingService _numbering;

        public PaymentVoucherService()
        {
            // لا نحتاج إنشاء DbContext مشترك الآن
        }
      



        // ================= لوائح أساسية =================

        public async Task<List<BranchModel>> GetBranchesAsync()
        {
            using var db = DbContextFactory.Create();
            return await db.Branches
                          .OrderBy(b => b.BranchName)
                          .AsNoTracking()
                          .ToListAsync();
        }

        // صناديق الفرع (نشطة وتدعم الصرف)
        public async Task<List<Fund>> GetCashBoxesByBranchAsync(int branchId)
        {
            using var db = DbContextFactory.Create();
            return await db.Funds
                          .Where(f => f.BranchId == branchId
                                   && f.IsActive == true
                                   && (f.FundType == FundType.PaymentOnly || f.FundType == FundType.Both))
                          .OrderBy(f => f.FundName)
                          .AsNoTracking()
                          .ToListAsync();
        }

        // بنوك الفرع (نشطة)
        public async Task<List<Bank>> GetBanksByBranchAsync(int branchId)
        {
            using var db = DbContextFactory.Create();
            return await db.Banks
                          .Where(b => b.BranchId == branchId && b.IsActive.HasValue && b.IsActive.Value)
                          .OrderBy(b => b.BankName)
                          .AsNoTracking()
                          .ToListAsync();
        }

        // (قوائم غير مصفّاة عند الحاجة)
        public async Task<List<Fund>> GetCashBoxesAsync()
        {
            using var db = DbContextFactory.Create();
            return await db.Funds
                          .Where(f => f.IsActive.HasValue && f.IsActive.Value
                                   && (f.FundType == FundType.PaymentOnly || f.FundType == FundType.Both))
                          .OrderBy(f => f.FundName)
                          .AsNoTracking()
                          .ToListAsync();
        }

        public async Task<List<Bank>> GetBanksAsync()
        {
            using var db = DbContextFactory.Create();
            return await db.Banks
                          .OrderBy(b => b.BankName)
                          .AsNoTracking()
                          .ToListAsync();
        }

        public async Task<List<NextCurrency>> GetCurrenciesAsync()
        {
            using var db = DbContextFactory.Create();
            return await db.NextCurrencies
                          .OrderBy(c => c.CurrencyNameAr)
                          .AsNoTracking()
                          .ToListAsync();
        }

        public async Task<decimal> GetExchangeRateAsync(int currencyId, DateTime? onDate)
        {
            try
            {
                using var db = DbContextFactory.Create();
                var q = db.CurrencyExchangeRates
                           .Where(x => x.CurrencyId == currencyId);

                var rate = await q
                    .Select(x => (decimal?)x.ExchangeRate)
                    .FirstOrDefaultAsync();

                return rate ?? 1m;
            }
            catch
            {
                return 1m;
            }
        }

        public async Task<List<DocumentType>> GetDocumentTypesAsync()
        {
            using var db = DbContextFactory.Create();
            return await db.DocumentTypes
                          .OrderBy(d => d.DocumentTypeId)
                          .AsNoTracking()
                          .ToListAsync();
        }

        public async Task<DocumentType?> GetPVTypeAsync()
        {
            using var db = DbContextFactory.Create();
            return await db.DocumentTypes
                          .Where(d => d.DocumentCode == "PV" && d.IsActive == true)
                          .AsNoTracking()
                          .FirstOrDefaultAsync();
        }

        public async Task<List<CostCenter>> GetCostCentersAsync()
        {
            using var db = DbContextFactory.Create();
            return await db.CostCenter
                          .OrderBy(c => c.CostCenterName)
                          .AsNoTracking()
                          .ToListAsync();
        }

        public async Task<List<Account>> GetAccountsAsync()
        {
            using var db = DbContextFactory.Create();
            return await db.Accounts
                          .Where(a => a.AccountType == 2 && a.IsActive == true)
                          .OrderBy(a => a.AccountNameAr)
                          .AsNoTracking()
                          .ToListAsync();
        }

        // عملات الصندوق
        public async Task<List<NextCurrency>> GetCurrenciesForCashBoxAsync(int fundId)
        {
            using var db = DbContextFactory.Create();
            var q = from d in db.FundCurrencyLimits
                    join c in db.NextCurrencies on d.CurrencyId equals c.CurrencyId
                    where d.FundId == fundId
                    select c;

            return await q.AsNoTracking()
                          .Distinct()
                          .OrderBy(c => c.CurrencyNameAr)
                          .ToListAsync();
        }

        // عملات البنك
        public async Task<List<NextCurrency>> GetCurrenciesForBankAsync(int bankId)
        {
            using var db = DbContextFactory.Create();
            var q = from d in db.BankCurrencyDetails
                    join c in db.NextCurrencies on d.CurrencyId equals c.CurrencyId
                    where d.BankId == bankId
                    select c;

            return await q.AsNoTracking()
                          .Distinct()
                          .OrderBy(c => c.CurrencyNameAr)
                          .ToListAsync();
        }

        // ================= البحث/الاستعراض =================
        // ترجع نتائج مختصرة قابلة للعرض (مع التصفية والصفحة)
        public async Task<(List<PaymentVoucherLookupItem> Items, int TotalCount)> SearchAsync(
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
            var q = db.PaymentVouchers.AsNoTracking().AsQueryable();

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
                              select new PaymentVoucherLookupItem
                              {
                                  VoucherID = v.VoucherID,
                                  DocumentNumber = v.DocumentNumber,
                                  DocumentDate = v.DocumentDate,
                                  BranchId = v.BranchID,
                                  BranchName = b.BranchName,
                                  VoucherType = v.VoucherType,
                                  SourceId = v.VoucherType == "Cash" ? v.CashBoxID : v.BankID,
                                  SourceName = v.VoucherType == "Cash" ? f.FundName : k.BankName,
                                  Beneficiary = v.Beneficiary,
                                  LocalAmount = v.LocalAmount
                              })
                              .Skip(skip)
                              .Take(take)
                              .ToListAsync();

            return (page, total);
        }



        // ================= CRUD =================

        public async Task<PaymentVoucher?> GetByIdAsync(int id)
        {
            using var db = DbContextFactory.Create();
            return await db.PaymentVouchers
                          .Include(v => v.Details)
                          .AsNoTracking()
                          .FirstOrDefaultAsync(v => v.VoucherID == id);
        }

        // ملاحظة: هنا فقط يتم توليد رقم السند داخل Transaction
        public async Task<PaymentVoucher> CreateAsync(PaymentVoucher v)
        {
            using var db = DbContextFactory.Create();
            var numbering = new NumberingService(db);
            
            await using var tx = await db.Database.BeginTransactionAsync();
            try
            {
                if (v.DocumentTypeID <= 0) throw new InvalidOperationException("نوع المستند مطلوب.");
                if (v.BranchID <= 0) throw new InvalidOperationException("الفرع مطلوب.");

                // الترقيم الموحّد (PV افتراضيًا)
                v.DocumentNumber = await numbering.GenerateNextAsync(v.DocumentTypeID, v.BranchID, "PV");
                v.CreatedAt = DateTime.Now;

                // رأس ثم تفاصيل
                var details = v.Details?.ToList() ?? new List<PaymentVoucherDetail>();
                v.Details = new List<PaymentVoucherDetail>();

                db.PaymentVouchers.Add(v);
                await db.SaveChangesAsync();

                foreach (var d in details)
                {
                    d.VoucherID = v.VoucherID;
                    db.PaymentVoucherDetails.Add(d);
                }
                if (details.Count > 0)
                    await db.SaveChangesAsync();

                await tx.CommitAsync();
                v.Details = details;
                return v;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                System.Windows.MessageBox.Show($"خطأ أثناء الإضافة:\n{ex.Message}");
                throw;
            }
        }

        public async Task UpdateAsync(PaymentVoucher v)
        {
            using var db = DbContextFactory.Create();
            try
            {
                var existing = await db.PaymentVouchers
                                        .Include(x => x.Details)
                                        .FirstOrDefaultAsync(x => x.VoucherID == v.VoucherID);

                if (existing == null)
                    throw new InvalidOperationException("السند غير موجود.");

                // تحديث الرأس
                db.Entry(existing).CurrentValues.SetValues(v);

                // حذف التفاصيل المحذوفة
                foreach (var d in existing.Details.ToList())
                    if (!v.Details.Any(x => x.DetailID == d.DetailID))
                        db.PaymentVoucherDetails.Remove(d);

                // إضافة/تحديث التفاصيل
                foreach (var d in v.Details)
                {
                    var target = existing.Details.FirstOrDefault(x => x.DetailID == d.DetailID);
                    if (target == null)
                    {
                        d.VoucherID = existing.VoucherID;
                        db.PaymentVoucherDetails.Add(d);
                    }
                    else
                    {
                        db.Entry(target).CurrentValues.SetValues(d);
                    }
                }

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء التعديل:\n{ex.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(int voucherId)
        {
            using var db = DbContextFactory.Create();
            try
            {
                var v = await db.PaymentVouchers
                                 .Include(x => x.Details)
                                 .FirstOrDefaultAsync(x => x.VoucherID == voucherId);
                if (v == null) return;

                db.PaymentVoucherDetails.RemoveRange(v.Details);
                db.PaymentVouchers.Remove(v);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء الحذف:\n{ex.Message}");
                throw;
            }
        }
    }
}
