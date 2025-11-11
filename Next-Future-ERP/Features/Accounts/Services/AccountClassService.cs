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
    public class AccountClassService : IAccountClassService
    {
        private readonly AppDbContext _db;

        public AccountClassService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<AccountClass>> GetAllAsync(string? search = null)
        {
            var q = _db.AccountClasses.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(x =>
                    x.AccountClassAname.Contains(s) ||
                    x.AccountClassEname.Contains(s) ||
                    (x.CategoryKey ?? "").Contains(s));
            }

            return await q.OrderBy(x => x.AccountClassAname).ToListAsync();
        }

        public Task<AccountClass?> GetByIdAsync(int id) =>
            _db.AccountClasses.AsNoTracking().FirstOrDefaultAsync(x => x.AccountClassId == id);

        public async Task<int> AddAsync(AccountClass entity)
        {
            Normalize(entity, isNew: true);
            _db.ChangeTracker.Clear();

            await _db.AccountClasses.AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity.AccountClassId;
        }

        public async Task UpdateAsync(AccountClass entity)
        {
            if (entity.AccountClassId <= 0)
                throw new InvalidOperationException("لا يمكن التعديل بدون معرف التصنيف.");

            Normalize(entity, isNew: false);
            _db.ChangeTracker.Clear();

            var affected = await _db.AccountClasses
                .Where(x => x.AccountClassId == entity.AccountClassId)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(p => p.AccountClassAname, entity.AccountClassAname)
                    .SetProperty(p => p.AccountClassEname, entity.AccountClassEname)
                    .SetProperty(p => p.CategoryKey, entity.CategoryKey)
                );

            if (affected == 0)
                throw new InvalidOperationException("السجل المطلوب تعديله غير موجود.");
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) return;

            _db.ChangeTracker.Clear();

            await _db.AccountClasses
                     .Where(x => x.AccountClassId == id)
                     .ExecuteDeleteAsync();
        }

        public async Task<List<AccountCategoryOption>> GetAccountCategoryOptionsAsync(string? categoryKey = null)
        {
            if (!string.IsNullOrWhiteSpace(categoryKey))
            {
                // فلترة آمنة بالمعلّمات
                return await _db.Set<AccountCategoryOption>()
                    .FromSqlInterpolated($@"
                SELECT [CategoryKey],[CategoryNameEn],[CategoryNameAr],[CategoryType]
                FROM [dbo].[AccountCategoryRoll]
                WHERE [CategoryType] = {categoryKey.Trim()}
            ")
                    .AsNoTracking()
                    .OrderBy(x => x.CategoryKey) // الترتيب خارج SQL
                    .ToListAsync();
            }

            // الكل
            return await _db.Set<AccountCategoryOption>()
                .FromSqlRaw(@"
            SELECT [CategoryKey],[CategoryNameEn],[CategoryNameAr],[CategoryType]
            FROM [dbo].[AccountCategoryRoll]
        ")
                .AsNoTracking()
                .OrderBy(x => x.CategoryKey)
                .ToListAsync();
        }






        private static void Normalize(AccountClass entity, bool isNew)
        {
            entity.AccountClassAname = (entity.AccountClassAname ?? "").Trim();
            entity.AccountClassEname = (entity.AccountClassEname ?? "").Trim();
            entity.CategoryKey = entity.CategoryKey?.Trim();

            if (entity.AccountClassAname.Length == 0) 
                throw new InvalidOperationException("الاسم العربي مطلوب.");
            if (entity.AccountClassEname.Length == 0) 
                throw new InvalidOperationException("الاسم الإنجليزي مطلوب.");
        }
    }
}
