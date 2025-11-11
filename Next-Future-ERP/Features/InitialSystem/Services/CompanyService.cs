using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.InitialSystem.Models;
using System;
using System.Threading.Tasks;

namespace Next_Future_ERP.Data.Services
{
    public class CompanyService
    {
        private readonly AppDbContext _context;

        public CompanyService()
        {
            _context = DbContextFactory.Create();
        }

        public async Task SaveCompanyInfoAsync(CompanyInfoModel companyInfo)
        {
            if (string.IsNullOrWhiteSpace(companyInfo.CompName))
                throw new ArgumentException("اسم الشركة لا يمكن أن يكون فارغاً");

            if (string.IsNullOrWhiteSpace(companyInfo.Currency))
                throw new ArgumentException("العملة لا يمكن أن تكون فارغة");

            companyInfo.CreatedAt = DateTime.Now;
            companyInfo.UpdatedAt = DateTime.Now;

          //  Check if company info already exists
           var existing = await _context.CompanyInfo.FirstOrDefaultAsync();
            if (existing != null)
            {
                // Update existing
                existing.CompName = companyInfo.CompName;
                existing.Currency = companyInfo.Currency;
                existing.Language = companyInfo.Language;
                existing.Timezone = companyInfo.Timezone;
                existing.DateFormat = companyInfo.DateFormat;
                existing.FiscalYearStart = companyInfo.FiscalYearStart;
                existing.MultiDeviceLogin = companyInfo.MultiDeviceLogin;
                existing.MinPasswordLength = companyInfo.MinPasswordLength;
                existing.UseVat = companyInfo.UseVat;
                existing.VatActivationDate = companyInfo.VatActivationDate;
                existing.AccountNumberLength = companyInfo.AccountNumberLength;
                existing.SubAccountLevel = companyInfo.SubAccountLevel;
                existing.EnableForeignCurrency = companyInfo.EnableForeignCurrency;
                existing.EnableCostCenters = companyInfo.EnableCostCenters;
                existing.HijriSupport = companyInfo.HijriSupport;
                existing.HijriAdjustment = companyInfo.HijriAdjustment;
                existing.ArabicLanguage = companyInfo.ArabicLanguage;
                existing.AssetsStart = companyInfo.AssetsStart;
                existing.LiabilitiesStart = companyInfo.LiabilitiesStart;
                existing.EquityStart = companyInfo.EquityStart;
                existing.RevenueStart = companyInfo.RevenueStart;
                existing.ExpenseStart = companyInfo.ExpenseStart;
                existing.UpdatedAt = DateTime.Now;
            }
            else
            {
                // Add new

                _context.CompanyInfo.Add(companyInfo);

                await _context.SaveChangesAsync();
            }
           
        }
    }
} 