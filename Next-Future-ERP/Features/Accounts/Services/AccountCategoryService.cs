using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public class AccountCategoryService : IAccountCategoryService
    {
        private readonly AppDbContext _db;
        public AccountCategoryService(AppDbContext db) => _db = db;

        //public Task<List<AccountCategoryRoll>> GetAllAsync(CancellationToken ct = default)
        //    => _db.AccountCategoryRolls.AsNoTracking()
        //          .OrderBy(c => c.CategoryNameAr)
        //          .ToListAsync(ct);

        //public Task<AccountCategoryRoll?> GetByKeyAsync(string key, CancellationToken ct = default)
        //    => _db.AccountCategoryRolls.AsNoTracking()
        //          .FirstOrDefaultAsync(c => c.CategoryKey == key, ct);
    }

}
