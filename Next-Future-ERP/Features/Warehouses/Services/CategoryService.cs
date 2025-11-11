using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Warehouses.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.Warehouses.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _db;

        public CategoryService(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<List<CategoryModel>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                return await _db.Categories
                    .AsNoTracking()
                    .ToListAsync(ct);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ أثناء جلب الفئات: {ex.Message}");
                // إرجاع قائمة فارغة بدلاً من عرض MessageBox من خيط غير UI
                return new();
            }
        }

        public async Task<List<CategoryModel>> GetParentCategoriesAsync(CancellationToken ct = default)
        {
            try
            {
                return await _db.Categories
                    .Where(c => c.ParentCategoryID == null)
                    .AsNoTracking()
                    .ToListAsync(ct);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ أثناء جلب الفئات الرئيسية: {ex.Message}");
                // إرجاع قائمة فارغة بدلاً من عرض MessageBox من خيط غير UI
                return new();
            }
        }

        public async Task SaveAsync(CategoryModel category, CancellationToken ct = default)
        {
            try
            {
                if (category.CategoryID == 0)
                {
                    category.CreatedDate = DateTime.Now;
                    _db.Categories.Add(category);
                }
                else
                {
                    var existing = await _db.Categories.FindAsync(new object[] { category.CategoryID }, ct);
                    if (existing is null)
                    {
                        System.Diagnostics.Debug.WriteLine("لم يتم العثور على الفئة للتعديل.");
                        return;
                    }

                    existing.CategoryCode = category.CategoryCode;
                    existing.CategoryName = category.CategoryName;
                    existing.ParentCategoryID = category.ParentCategoryID;
                    existing.Description = category.Description;
                    existing.ModifiedDate = DateTime.Now;
                }

                await _db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"حدث خطأ أثناء حفظ الفئة: {ex.Message}");
            }
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var entity = await _db.Categories.FindAsync(new object[] { id }, ct);
                if (entity == null)
                {
                    System.Diagnostics.Debug.WriteLine("لم يتم العثور على الفئة للحذف.");
                    return;
                }

                // التأكد من عدم وجود فئات فرعية
                var hasChildren = await _db.Categories.AnyAsync(c => c.ParentCategoryID == id, ct);
                if (hasChildren)
                {
                    System.Diagnostics.Debug.WriteLine("لا يمكن حذف الفئة لأنها تحتوي على فئات فرعية.");
                    return;
                }

                _db.Categories.Remove(entity);
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"حدث خطأ أثناء حذف الفئة: {ex.Message}");
            }
        }
    }
}
