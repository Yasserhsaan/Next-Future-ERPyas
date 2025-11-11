using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.InitialSystem.Models;
using System;
using System.Threading.Tasks;

namespace Next_Future_ERP.Data.Services
{
    public class AccountingSetupService
    {
        private readonly AppDbContext _context;

        public AccountingSetupService()
        {
            _context = DbContextFactory.Create();
        }

        public async Task SaveAccountingSetupAsync(AccountingSetupModel accountingSetup)
        {
            if (string.IsNullOrWhiteSpace(accountingSetup.DefaultCashAccount))
                throw new ArgumentException("الحساب النقدي الافتراضي لا يمكن أن يكون فارغاً");

            accountingSetup.CreatedAt = DateTime.Now;
            accountingSetup.UpdatedAt = DateTime.Now;

           // Check if accounting setup already exists
           var existing = await _context.AccountingSetup.FirstOrDefaultAsync();
            var existingCompany = await _context.CompanyInfo.FirstOrDefaultAsync();
            var existingBranch = await _context.Branches.FirstOrDefaultAsync();
            if (existing != null)
            {
                // Update existing
                existing.ComiId = existingCompany!.CompId;
                existing.BranchId = existingBranch!.BranchId;
                existing.FiscalYear = accountingSetup.FiscalYear;
                existing.CurrentMonth = accountingSetup.CurrentMonth;
                existing.AutoPosting = accountingSetup.AutoPosting;
                existing.ChartType = accountingSetup.ChartType;
                existing.DefaultCashAccount = accountingSetup.DefaultCashAccount;
                existing.DefaultBankAccount = accountingSetup.DefaultBankAccount;
                existing.DefaultInventoryAccount = accountingSetup.DefaultInventoryAccount;
                existing.ProfitLossAccount = accountingSetup.ProfitLossAccount;
                existing.LinkedAccounts = accountingSetup.LinkedAccounts;
                existing.UpdatedAt = DateTime.Now;
            }
            else
            {
                // Add new
                accountingSetup.ComiId = existingCompany!.CompId;
                accountingSetup.BranchId = existingBranch!.BranchId;
                _context.AccountingSetup.Add(accountingSetup);

                await _context.SaveChangesAsync();

            }
            
        }
    }
} 