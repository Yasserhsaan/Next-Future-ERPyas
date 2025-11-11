using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.InitialSystem.Models;
using System;
using System.Threading.Tasks;

namespace Next_Future_ERP.Data.Services
{
    public class FinancialPeriodsService
    {
        private readonly AppDbContext _context;

        public FinancialPeriodsService()
        {
            _context = DbContextFactory.Create();
        }

        public async Task SaveFinancialPeriodsAsync(FinancialPeriodsSettingModlel financialPeriods)
        {


         //   Check if financial periods already exists
           var existing = await _context.FinancialPeriods.FirstOrDefaultAsync();
            var existingCompany = await _context.CompanyInfo.FirstOrDefaultAsync();
            if (existing != null)
            {
                // Update existing
                existing.CompanyId = existingCompany!.CompId;
                existing.AllowFutureDateEntry = financialPeriods.AllowFutureDateEntry;
                existing.AutoPeriodRollover = financialPeriods.AutoPeriodRollover;
                existing.LockedPeriods = financialPeriods.LockedPeriods;
                existing.PeriodClosePolicy = financialPeriods.PeriodClosePolicy;
              
               
                existing.GeneratedPeriods = financialPeriods.GeneratedPeriods;
            }
            else
            {
                // Add new
                financialPeriods.CompanyId = existingCompany!.CompId;
                _context.FinancialPeriods.Add(financialPeriods);
                await _context.SaveChangesAsync();

            }

            
        }
    }
} 