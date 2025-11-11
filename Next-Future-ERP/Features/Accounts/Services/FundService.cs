using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
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
    public class FundService

    {
        public async Task<List<Fund>> GetAllAsync()
        {
            try
            {
                using var db = DbContextFactory.Create();
                return await db.Funds
                    .Include(f => f.Company)
                    .Include(f => f.Branch)
                    .Include(f => f.CurrencyLimits)
                    .AsNoTracking()
                    .OrderBy(f => f.FundName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء جلب الصناديق:\n{ex.Message}");
                return new();
            }
        }

        public async Task<Fund?> GetByIdAsync(int id)
        {
            try
            {
                using var db = DbContextFactory.Create();
                return await db.Funds
                    .Include(f => f.Company)
                    .Include(f => f.Branch)
                    .Include(f => f.CurrencyLimits)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.FundId == id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء جلب الصندوق:\n{ex.Message}");
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

        public async Task<Fund> CreateAsync(Fund fund)
        {
            try
            {
                using var db = DbContextFactory.Create();
                fund.CreatedAt = DateTime.Now;
                db.Funds.Add(fund);
                await db.SaveChangesAsync();
                return fund;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء الإضافة:\n{ex.Message}");
                return fund;
            }
        }

        public async Task UpdateAsync(Fund fund)
        {
            try
            {
                using var db = DbContextFactory.Create();

                var dbFund = await db.Funds
                    .Include(f => f.CurrencyLimits)
                    .FirstOrDefaultAsync(f => f.FundId == fund.FundId);

                if (dbFund == null) return;

                db.Entry(dbFund).CurrentValues.SetValues(fund);

                foreach (var row in fund.CurrencyLimits)
                {
                    if (row.LimitId == 0) row.FundId = dbFund.FundId;

                    var existing = dbFund.CurrencyLimits.FirstOrDefault(x => x.LimitId == row.LimitId);
                    if (existing == null)
                        dbFund.CurrencyLimits.Add(row);
                    else
                        db.Entry(existing).CurrentValues.SetValues(row);
                }

                foreach (var existing in dbFund.CurrencyLimits.ToList())
                    if (!fund.CurrencyLimits.Any(x => x.LimitId == existing.LimitId))
                        db.Remove(existing);

                dbFund.UpdatedAt = DateTime.Now;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"❌ خطأ أثناء التعديل:\n{ex.Message}");
            }
        }



        public async Task DeleteAsync(int fundId)
        {
            try
            {
                using var db = DbContextFactory.Create();
                var entity = await db.Funds.FindAsync(fundId);
                if (entity == null)
                {
                    MessageBox.Show("⚠️ لم يتم العثور على الصندوق للحذف.");
                    return;
                }
                db.Funds.Remove(entity);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء الحذف:\n{ex.Message}");
            }
        }
    }
}
