// Features/Accounts/Services/DebitCreditNotificationService.cs
using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.Accounts.Models;
using Next_Future_ERP.Features.InitialSystem.Models;
using Next_Future_ERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public class DebitCreditNotificationService
    {
        private readonly AppDbContext _db;

        public DebitCreditNotificationService()
        {
            _db = DbContextFactory.Create();
        }

        // ===== قوائم أساسية =====
        public async Task<List<BranchModel>> GetBranchesAsync()
            => await _db.Branches.OrderBy(b => b.BranchName).AsNoTracking().ToListAsync();

        public async Task<List<Account>> GetAccountsAsync()
            => await _db.Accounts.OrderBy(a => a.AccountNameAr).AsNoTracking().ToListAsync();

        public async Task<List<NextCurrency>> GetCurrenciesAsync()
            => await _db.NextCurrencies.OrderBy(c => c.CurrencyNameAr).AsNoTracking().ToListAsync();

        public async Task<decimal> GetExchangeRateAsync(int currencyId, DateTime onDate)
        {
            try
            {
                var rate = await _db.CurrencyExchangeRates
                    .Where(x => x.CurrencyId == currencyId)
                    .OrderByDescending(x => x.DateExchangeEnd?? DateTime.MinValue)
                    .Select(x => (decimal?)x.ExchangeRate)
                    .FirstOrDefaultAsync();
                return rate ?? 1m;
            }
            catch { return 1m; }
        }

        // ===== CRUD =====
        public async Task<DebitCreditNotification?> GetByIdAsync(long id)
            => await _db.DebitCreditNotifications
                        .Include(v => v.Details)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(v => v.NotificationId == id);

        public async Task<DebitCreditNotification> CreateAsync(DebitCreditNotification n)
        {
            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                n.CreatedAt = DateTime.Now;
                _db.DebitCreditNotifications.Add(n);
                await _db.SaveChangesAsync();

                // تفاصيل
                foreach (var d in n.Details)
                {
                    d.NotificationId = n.NotificationId;
                    _db.DebitCreditNoteDetails.Add(d);
                }
                if (n.Details.Count > 0)
                    await _db.SaveChangesAsync();

                await tx.CommitAsync();
                return n;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                MessageBox.Show($"خطأ أثناء الإضافة:\n{ex.Message}");
                throw;
            }
        }

        public async Task UpdateAsync(DebitCreditNotification n)
        {
            try
            {
                var existing = await _db.DebitCreditNotifications
                                        .Include(x => x.Details)
                                        .FirstOrDefaultAsync(x => x.NotificationId == n.NotificationId);
                if (existing == null)
                    throw new InvalidOperationException("الإشعار غير موجود.");

                _db.Entry(existing).CurrentValues.SetValues(n);

                // حذف تفاصيل ملغاة
                foreach (var d in existing.Details.ToList())
                    if (!n.Details.Any(x => x.DetailId == d.DetailId))
                        _db.DebitCreditNoteDetails.Remove(d);

                // إضافة/تحديث تفاصيل
                foreach (var d in n.Details)
                {
                    var target = existing.Details.FirstOrDefault(x => x.DetailId == d.DetailId);
                    if (target == null)
                    {
                        d.NotificationId = existing.NotificationId;
                        _db.DebitCreditNoteDetails.Add(d);
                    }
                    else
                    {
                        _db.Entry(target).CurrentValues.SetValues(d);
                    }
                }

                existing.ModifiedAt = DateTime.Now;
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء التعديل:\n{ex.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                var n = await _db.DebitCreditNotifications
                                 .Include(x => x.Details)
                                 .FirstOrDefaultAsync(x => x.NotificationId == id);
                if (n == null) return;

                _db.DebitCreditNoteDetails.RemoveRange(n.Details);
                _db.DebitCreditNotifications.Remove(n);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء الحذف:\n{ex.Message}");
                throw;
            }
        }

        // ===== البحث (مع صفحات) =====
        public async Task<(List<DebitCreditNotificationLookupItem> items, int total)> SearchAsync(
            int? branchId,
            string? dcType,                 // "D" / "C" / null
            string? accountNumber,
            DateTime? dateFrom,
            DateTime? dateTo,
            byte? status,
            int skip, int take)
        {
            var q = from n in _db.DebitCreditNotifications
                    join b in _db.Branches on n.BranchId equals b.BranchId
                    join c in _db.NextCurrencies on n.CurrencyId equals c.CurrencyId
                    select new { n, b.BranchName, c.CurrencyNameAr };

            if (branchId.HasValue) q = q.Where(x => x.n.BranchId == branchId.Value);
            if (!string.IsNullOrWhiteSpace(dcType)) q = q.Where(x => x.n.NotificationType == dcType);
            if (!string.IsNullOrWhiteSpace(accountNumber)) q = q.Where(x => x.n.AccountNumber.Contains(accountNumber));
            if (dateFrom.HasValue) q = q.Where(x => x.n.NotificationDate >= dateFrom.Value);
            if (dateTo.HasValue) q = q.Where(x => x.n.NotificationDate <= dateTo.Value);
            if (status.HasValue) q = q.Where(x => x.n.Status == status.Value);

            var total = await q.CountAsync();

            var page = await q.OrderByDescending(x => x.n.NotificationDate)
                              .ThenByDescending(x => x.n.NotificationId)
                              .Skip(skip).Take(take)
                              .Select(x => new DebitCreditNotificationLookupItem
                              {
                                  NotificationId = x.n.NotificationId,
                                  NotificationDate = x.n.NotificationDate,
                                  DCType = x.n.NotificationType,
                                  BranchName = x.BranchName!,
                                  AccountNumber = x.n.AccountNumber,
                                  CurrencyName = x.CurrencyNameAr!,
                                  TotalAmount = x.n.TotalAmount,
                                  Status = x.n.Status
                              })
                              .AsNoTracking()
                              .ToListAsync();

            return (page, total);
        }
    }
}
