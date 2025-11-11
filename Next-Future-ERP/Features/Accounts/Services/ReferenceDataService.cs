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
    public class ReferenceDataService : IReferenceDataService
    {
        private readonly AppDbContext _context;

        public ReferenceDataService()
        {
            _context = DbContextFactory.Create();
        }

        public async Task<List<Account>> GetLeafAccountsAsync(int companyId, int branchId)
        {
            try
            {
                return await _context.Accounts
                    .Where(a => a.CompanyId == companyId && 
                               a.BranchId == branchId && 
                               a.AccountType == 2 && // الحسابات الفرعية فقط
                               a.IsActive == true)
                    .OrderBy(a => a.AccountCode)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع الحسابات الفرعية:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Account>();
            }
        }

        public async Task<List<CostCenter>> GetCostCentersAsync(int companyId, int branchId)
        {
            try
            {
                return await _context.CostCenter
                    .Where(cc => cc.IsActive)
                    .OrderBy(cc => cc.CostCenterName)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع مراكز التكلفة:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<CostCenter>();
            }
        }

        public async Task<List<NextCurrency>> GetCurrenciesAsync(int companyId)
        {
            try
            {
                return await _context.NextCurrencies
                    .Where(c => c.CompanyId == companyId)
                    .OrderBy(c => c.CurrencyNameAr)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع العملات:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<NextCurrency>();
            }
        }

        public async Task<NextCurrency?> GetCompanyCurrencyAsync(int companyId)
        {
            try
            {
                return await _context.NextCurrencies
                    .Where(c => c.CompanyId == companyId && c.IsCompanyCurrency == true)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع عملة الشركة:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<bool> IsAccountCurrencyLinkedAsync(int accountId, int currencyId)
        {
            try
            {
                return await _context.AccountCurrencies
                    .AnyAsync(ac => ac.AccountId == accountId && ac.CurrencyId == currencyId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في التحقق من ربط الحساب بالعملة:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> LinkAccountCurrencyAsync(int accountId, int currencyId)
        {
            try
            {
                // التحقق من وجود الربط مسبقاً
                if (await IsAccountCurrencyLinkedAsync(accountId, currencyId))
                    return true;

                var accountCurrency = new AccountCurrency
                {
                    AccountId = accountId,
                    CurrencyId = currencyId,
                    IsStopped = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AccountCurrencies.Add(accountCurrency);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في ربط الحساب بالعملة:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<Account?> GetAccountDetailsAsync(int accountId)
        {
            try
            {
                return await _context.Accounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AccountId == accountId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع تفاصيل الحساب:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<Account?> FindAccountByCodeAsync(string accountCode, int companyId, int branchId)
        {
            try
            {
                return await _context.Accounts
                    .Where(a => a.AccountCode == accountCode && 
                               a.CompanyId == companyId && 
                               a.BranchId == branchId &&
                               a.AccountType == 2) // فرعي فقط
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في البحث عن الحساب بالكود:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }


        public async Task<List<CompanyInfoModel>> GetCompaniesAsync()
        {
            try
            {
                return await _context.CompanyInfo
                    .OrderBy(c => c.CompName)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع الشركات:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<CompanyInfoModel>();
            }
        }

        public async Task<List<BranchModel>> GetBranchesForCompanyAsync(int companyId)
        {
            try
            {
                return await _context.Branches
                    .Where(b => b.ComiId == companyId)
                    .OrderBy(b => b.BranchName)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع الفروع:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<BranchModel>();
            }
        }

        public async Task<List<Fund>> GetFundsForBranchAsync(int companyId, int branchId)
        {
            try
            {
                return await _context.Funds
                    .Where(f => f.CompanyId == companyId && 
                               f.BranchId == branchId && 
                               f.IsActive == true)
                    .OrderBy(f => f.FundName)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع الصناديق:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Fund>();
            }
        }

        public async Task<List<Bank>> GetBanksForBranchAsync(int companyId, int branchId)
        {
            try
            {
                return await _context.Banks
                    .Where(b => b.CompanyId == companyId && 
                               b.BranchId == branchId && 
                               b.IsActive == true)
                    .OrderBy(b => b.BankName)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع البنوك:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Bank>();
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
