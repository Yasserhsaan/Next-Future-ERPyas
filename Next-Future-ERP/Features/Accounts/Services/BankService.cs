// Features/Accounts/Services/BankService.cs
using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.InitialSystem.Models;
using Next_Future_ERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public class BankService
    {
        public async Task<List<Bank>> GetAllAsync()
        {
            try
            {
                using var db = DbContextFactory.Create();
                return await db.Set<Bank>()
                    .Include(b => b.Company)
                    .Include(b => b.Branch)
                    .Include(b => b.CurrencyDetails)
                    .AsNoTracking()
                    .OrderBy(b => b.BankName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء جلب البنوك:\n{ex.Message}");
                return new();
            }
        }

        public async Task<Bank?> GetByIdAsync(int id)
        {
            try
            {
                using var db = DbContextFactory.Create();
                return await db.Set<Bank>()
                    .Include(b => b.Company)
                    .Include(b => b.Branch)
                    .Include(b => b.CurrencyDetails)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.BankId == id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء جلب البنك:\n{ex.Message}");
                return null;
            }
        }

        public async Task<List<NextCurrency>> GetCurrenciesAsync()
        {
            try
            {
                using var db = DbContextFactory.Create();
                return await db.NextCurrencies
                    .AsNoTracking()
                    .OrderBy(c => c.CurrencyNameAr)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء جلب العملات:\n{ex.Message}");
                return new();
            }
        }

        public async Task<List<CompanyInfoModel>> GetCompaniesAsync()
        {
            try
            {
                using var db = DbContextFactory.Create();
                return await db.CompanyInfo
                    .AsNoTracking()
                    .OrderBy(c => c.CompName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء جلب الشركات:\n{ex.Message}");
                return new();
            }
        }

        public async Task<List<BranchModel>> GetBranchesByCompanyAsync(int companyId)
        {
            try
            {
                using var db = DbContextFactory.Create();
                return await db.Branches
                    .AsNoTracking()
                    .Where(b => b.ComiId == companyId)
                    .OrderBy(b => b.BranchName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء جلب الفروع:\n{ex.Message}");
                return new();
            }
        }

        public async Task<Bank> CreateAsync(Bank bank)
        {
            try
            {
                using var db = DbContextFactory.Create();
                bank.CreatedAt = DateTime.Now;
                db.Set<Bank>().Add(bank);
                await db.SaveChangesAsync();
                return bank;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء الإضافة:\n{ex.Message}");
                return bank;
            }
        }

        public async Task UpdateAsync(Bank bank)
        {
            try
            {
                using var db = DbContextFactory.Create();

                var dbBank = await db.Set<Bank>()
                    .Include(b => b.CurrencyDetails)
                    .FirstOrDefaultAsync(b => b.BankId == bank.BankId);

                if (dbBank == null) return;

                db.Entry(dbBank).CurrentValues.SetValues(bank);

                // upsert للتفاصيل
                foreach (var row in bank.CurrencyDetails)
                {
                    if (row.DetailId == 0) row.BankId = dbBank.BankId;

                    var existing = dbBank.CurrencyDetails.FirstOrDefault(x => x.DetailId == row.DetailId);
                    if (existing == null)
                        dbBank.CurrencyDetails.Add(row);
                    else
                        db.Entry(existing).CurrentValues.SetValues(row);
                }

                // حذف ما تم إزالته من الواجهة
                foreach (var existing in dbBank.CurrencyDetails.ToList())
                    if (!bank.CurrencyDetails.Any(x => x.DetailId == existing.DetailId))
                        db.Remove(existing);

                dbBank.UpdatedAt = DateTime.Now;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء التحديث:\n{ex.Message}");
            }
        }

        public async Task DeleteAsync(int bankId)
        {
            try
            {
                using var db = DbContextFactory.Create();
                var entity = await db.Set<Bank>().FindAsync(bankId);
                if (entity == null)
                {
                    MessageBox.Show("⚠️ لم يتم العثور على البنك للحذف.");
                    return;
                }
                db.Set<Bank>().Remove(entity);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء الحذف:\n{ex.Message}");
            }
        }
    }
}
