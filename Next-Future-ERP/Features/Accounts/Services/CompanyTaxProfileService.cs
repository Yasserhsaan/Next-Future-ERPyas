using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Accounts.Models;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Threading.Tasks;
using System;


namespace Next_Future_ERP.Features.Accounts.Services
{
    public class CompanyTaxProfileService
    {
        private readonly AppDbContext _db;

        // بدّل هذا بمنشئ DI لو تحب
        public CompanyTaxProfileService()
        {
            _db = DbContextFactory.Create();
        }

        public async Task<List<CompanyOption>> GetCompanyOptionsAsync()
        {
            var sql = @"SELECT CompanyId, CompanyName FROM dbo.Companies ORDER BY CompanyName";
            return await _db.Set<CompanyOption>().FromSqlRaw(sql).AsNoTracking().ToListAsync();
        }

        public async Task<List<BranchOption>> GetBranchOptionsAsync(int? companyId)
        {
            var sql = @"
SELECT BranchId, CompanyId, BranchName
FROM dbo.Branches
WHERE (@p0 IS NULL OR CompanyId = @p0)
ORDER BY BranchName";
            return await _db.Set<BranchOption>()
                .FromSqlRaw(sql, companyId ?? (object)DBNull.Value)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(List<CompanyTaxProfileLookup> Items, int Total)> SearchAsync(
            int? companyId, int? branchId, string? vat, string? activityCode, string? taxOffice,
            DateTime? from, DateTime? to, int page = 1, int pageSize = 20)
        {
            var sql = @"
SELECT p.ProfileId, p.CompanyId, c.CompanyName,
       p.BranchId, b.BranchName,
       p.VATRegistrationNumber, p.BranchVATNumber, p.TaxOffice,
       p.TaxpayerType, p.ActivityCode, p.CreatedAt, p.UpdatedAt
FROM dbo.Company_Tax_Profile p
JOIN dbo.Companies c ON c.CompanyId = p.CompanyId
LEFT JOIN dbo.Branches b ON b.BranchId = p.BranchId
WHERE (@CompanyId IS NULL OR p.CompanyId = @CompanyId)
  AND (@BranchId  IS NULL OR p.BranchId  = @BranchId)
  AND (@VAT IS NULL OR p.VATRegistrationNumber LIKE '%' + @VAT + '%')
  AND (@ActCode IS NULL OR p.ActivityCode LIKE '%' + @ActCode + '%')
  AND (@TaxOffice IS NULL OR p.TaxOffice LIKE '%' + @TaxOffice + '%')
  AND (@From IS NULL OR p.CreatedAt >= @From)
  AND (@To   IS NULL OR p.CreatedAt < DATEADD(DAY, 1, @To))
";
            var q = _db.Set<CompanyTaxProfileLookup>()
                .FromSqlRaw(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@CompanyId", companyId ?? (object)DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@BranchId", branchId ?? (object)DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@VAT", (object?)vat ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@ActCode", (object?)activityCode ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@TaxOffice", (object?)taxOffice ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@From", (object?)from ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@To", (object?)to ?? DBNull.Value))
                .AsNoTracking();

            var all = await q.ToListAsync();
            var total = all.Count;

            var items = all.OrderByDescending(x => x.ProfileId)
                           .Skip((page - 1) * pageSize)
                           .Take(pageSize)
                           .ToList();

            return (items, total);
        }

        public async Task<CompanyTaxProfile?> GetAsync(int id)
        {
            return await _db.Set<CompanyTaxProfile>()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.ProfileId == id);
        }

        public async Task<int> UpsertAsync(CompanyTaxProfile model)
        {
            // فحص خفيف لرقم VAT السعودي (عادة 15 رقم)
            if (!string.IsNullOrWhiteSpace(model.VATRegistrationNumber))
            {
                var digits = new string(model.VATRegistrationNumber.Where(char.IsDigit).ToArray());
                if (digits.Length != 15)
                    throw new InvalidOperationException("الرقم الضريبي السعودي عادة 15 رقم.");
            }

            if (model.ProfileId == 0)
            {
                model.CreatedAt ??= DateTime.Now;
                model.UpdatedAt = DateTime.Now;
                _db.Add(model);
            }
            else
            {
                var existing = await _db.Set<CompanyTaxProfile>().FirstOrDefaultAsync(x => x.ProfileId == model.ProfileId);
                if (existing == null) throw new InvalidOperationException("السجل غير موجود.");

                _db.Entry(existing).CurrentValues.SetValues(model);
                existing.UpdatedAt = DateTime.Now;
            }

            await _db.SaveChangesAsync();
            return model.ProfileId;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _db.Set<CompanyTaxProfile>().FirstOrDefaultAsync(x => x.ProfileId == id);
            if (entity != null)
            {
                _db.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }
    }
}
