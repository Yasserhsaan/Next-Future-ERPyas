// Features/Accounts/Services/AccountsService.cs
using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.Accounts.Models;
using Next_Future_ERP.Models;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public class AccountsService
    {
        private readonly AppDbContext _context;

        public AccountsService()
        {
            _context = DbContextFactory.Create();
        }
        // استرجاع الحسابات التي نوعها = 2 (فرعية)
        public async Task<List<Account>> GetAccountsOfType2Async()
        {
            try
            {
                return await _context.Accounts
                    .Where(a => a.AccountType == 2)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ حدث خطأ أثناء جلب الحسابات من النوع 2:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new();
            }
        }

        // استرجاع كل الحسابات
        public async Task<List<Account>> GetAllAsync()
        {
            try
            {
                return await _context.Accounts.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء جلب البيانات:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new(); // إرجاع قائمة فارغة
            }
        }

        // بناء شجرة الحسابات
        public async Task<List<Account>> GetAccountsTreeAsync()
        {
            try
            {
                var accounts = await _context.Accounts
                    .AsNoTracking()
                    .ToListAsync();

                var lookup = accounts.ToLookup(a => a.ParentAccountCode);

                foreach (var acc in accounts)
                    acc.Children = lookup[acc.AccountCode].ToList();

                return accounts.Where(a => a.ParentAccountCode == null).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ حدث خطأ أثناء تحميل شجرة الحسابات:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new(); // قائمة فارغة في حال الخطأ
            }
        }

        // إضافة حساب جديد
        public async Task AddAsync(Account acc)
        {
            try
            {
                acc.CreatedAt = DateTime.Now;
                _context.Accounts.Add(acc);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ حدث خطأ أثناء الإضافة:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // تحديث حساب موجود
        public async Task UpdateAsync(Account acc)
        {
            try
            {
                var existing = await _context.Accounts.FindAsync(acc.AccountId);
                if (existing is null)
                {
                    MessageBox.Show("⚠️ لم يتم العثور على الحساب للتعديل.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // تحديث الحقول النصية والحالة
                existing.AccountNameAr = acc.AccountNameAr;
                existing.AccountNameEn = acc.AccountNameEn;
                existing.Notes = acc.Notes;
                existing.IsActive = acc.IsActive;

                // الحقول التصنيفية والطبيعة ونوع الحساب والتدفق النقدي
                existing.AccountClassification = acc.AccountClassification;
                existing.Nature = acc.Nature;
                existing.AccountType = acc.AccountType;
                existing.TypeOfCashFlow = acc.TypeOfCashFlow;

                // استخدام مركز تكلفة وصلاحية المستوى والفئة
                existing.UsesCostCenter = acc.UsesCostCenter;
                existing.AccountLevelPrivlige = acc.AccountLevelPrivlige;
                existing.AccountCategoryKey = acc.AccountCategoryKey;

                existing.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ حدث خطأ أثناء التعديل:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // حذف حساب
        public async Task DeleteAsync(int id)
        {
            try
            {
                var entity = await _context.Accounts.FindAsync(id);
                if (entity == null)
                {
                    MessageBox.Show("⚠️ لم يتم العثور على الحساب للحذف.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _context.Accounts.Remove(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ حدث خطأ أثناء الحذف:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public async Task<AccountLevelInfo?> GetAccountLevelInfoAsync(string accountCode)
        {
            // استدعاء TVF
            return await _context.Set<AccountLevelInfo>()
                .FromSqlInterpolated($"SELECT * FROM fn_GetAccountLevelInfo({accountCode})")
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        // AccountsService.cs (إضافة)
        public async Task<List<Account>> GetByCategoryKeyAsync(string categoryKey)
        {
            try
            {
                return await _context.Accounts
                    .Where(a => a.AccountType == 2 && a.AccountCategoryKey == categoryKey)
                    .OrderBy(a => a.AccountCode)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء جلب حسابات {categoryKey}:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new();
            }
        }

    }
}
