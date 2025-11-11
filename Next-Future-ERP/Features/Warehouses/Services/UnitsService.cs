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
    public class UnitsService : IUnitsService
    {
        private readonly AppDbContext _db;

        public UnitsService(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<List<UnitModel>> GetByUnitTypeAsync(char unitType, CancellationToken ct = default)
        {
            try
            {
                return await _db.Units
                    .Where(u => u.UnitType == unitType)
                    .AsNoTracking()
                    .ToListAsync(ct);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ أثناء جلب الوحدات حسب النوع: {ex.Message}");
                // إرجاع قائمة فارغة بدلاً من عرض MessageBox من خيط غير UI
                return new();
            }
        }

        public async Task<List<UnitModel>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                return await _db.Units
                    .AsNoTracking()
                    .ToListAsync(ct);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ أثناء جلب الوحدات: {ex.Message}");
                // إرجاع قائمة فارغة بدلاً من عرض MessageBox من خيط غير UI
                return new();
            }
        }

        public async Task SaveAsync(UnitModel unit, CancellationToken ct = default)
        {
            try
            {
                if (unit.UnitID == 0)
                {
                    unit.CreatedDate = DateTime.Now;
                    _db.Units.Add(unit);
                }
                else
                {
                    var existing = await _db.Units.FindAsync(new object[] { unit.UnitID }, ct);
                    if (existing is null)
                    {
                        System.Diagnostics.Debug.WriteLine("لم يتم العثور على الوحدة للتعديل.");
                        return;
                    }

                    existing.UnitCode = unit.UnitCode;
                    existing.UnitName = unit.UnitName;
                    existing.UnitType = unit.UnitType;
                    existing.UnitClass = unit.UnitClass;
                    existing.BaseUnitID = unit.BaseUnitID;
                    existing.ConversionFactor = unit.ConversionFactor;
                    existing.IsActive = unit.IsActive;
                    existing.DefaultPackaging = unit.DefaultPackaging;
                    existing.ModifiedDate = DateTime.Now;
                }

                await _db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"حدث خطأ أثناء حفظ الوحدة: {ex.Message}");
            }
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var entity = await _db.Units.FindAsync(new object[] { id }, ct);
                if (entity == null)
                {
                    System.Diagnostics.Debug.WriteLine("لم يتم العثور على الوحدة للحذف.");
                    return;
                }

                // التأكد من عدم استخدامها كوحدة أساسية
                var hasChildren = await _db.Units.AnyAsync(u => u.BaseUnitID == id, ct);
                if (hasChildren)
                {
                    System.Diagnostics.Debug.WriteLine("لا يمكن حذف الوحدة لأنها تستخدم كوحدة أساسية لوحدات أخرى.");
                    return;
                }

                _db.Units.Remove(entity);
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"حدث خطأ أثناء حذف الوحدة: {ex.Message}");
            }
        }
    }
}
