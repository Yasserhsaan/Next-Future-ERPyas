using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.PosStations.Models;
using Next_Future_ERP.Data.Models;
using Next_Future_ERP.Features.InitialSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.PosStations.Services
{
    /// <summary>
    /// خدمة إدارة محطات نقاط البيع
    /// </summary>
    public class PosStationService : IPosStationService, IDisposable
    {
        private readonly AppDbContext _context;
        private readonly bool _ownsContext;

        public PosStationService()
        {
            _context = new AppDbContext();
            _ownsContext = true;
        }

        public PosStationService(AppDbContext context)
        {
            _context = context;
            _ownsContext = false;
        }

        /// <summary>
        /// جلب جميع محطات نقاط البيع
        /// </summary>
        public async Task<List<PosStation>> GetAllAsync(int? companyId = null, int? branchId = null, bool? isActive = null)
        {
            var query = _context.PosStations
                .Include(x => x.Branch)
                .Include(x => x.AssignedUserNavigation)
                .AsQueryable();

            if (companyId.HasValue)
                query = query.Where(x => x.CompanyId == companyId.Value);

            if (branchId.HasValue)
                query = query.Where(x => x.BranchId == branchId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            var stations = await query
                .OrderBy(x => x.PosName)
                .ToListAsync();

            // تحميل البيانات المرتبطة للعرض
            foreach (var station in stations)
            {
                station.BranchName = station.Branch?.BranchName;
                station.AssignedUserName = station.AssignedUserNavigation?.Name;
            }

            return stations;
        }

        /// <summary>
        /// جلب محطة نقطة بيع بالمعرف
        /// </summary>
        public async Task<PosStation?> GetByIdAsync(int posId)
        {
            var station = await _context.PosStations
                .Include(x => x.Branch)
                .Include(x => x.AssignedUserNavigation)
                .FirstOrDefaultAsync(x => x.PosId == posId);

            if (station != null)
            {
                station.BranchName = station.Branch?.BranchName;
                station.AssignedUserName = station.AssignedUserNavigation?.Name;
            }

            return station;
        }

        /// <summary>
        /// جلب محطة نقطة بيع بالكود
        /// </summary>
        public async Task<PosStation?> GetByCodeAsync(string posCode)
        {
            var station = await _context.PosStations
                .Include(x => x.Branch)
                .Include(x => x.AssignedUserNavigation)
                .FirstOrDefaultAsync(x => x.PosCode == posCode);

            if (station != null)
            {
                station.BranchName = station.Branch?.BranchName;
                station.AssignedUserName = station.AssignedUserNavigation?.Name;
            }

            return station;
        }

        /// <summary>
        /// إضافة محطة نقطة بيع جديدة
        /// </summary>
        public async Task<PosStation> AddAsync(PosStation station)
        {
            // التحقق من عدم تكرار الكود
            if (await _context.PosStations.AnyAsync(x => x.PosCode == station.PosCode))
            {
                throw new InvalidOperationException($"كود المحطة '{station.PosCode}' موجود بالفعل");
            }

            // تعيين القيم الافتراضية
            station.CreatedDate = DateTime.UtcNow;
            station.IsActive = true;

            // تطبيع البيانات
            station.PosName = station.PosName?.Trim();
            station.PosCode = station.PosCode?.Trim().ToUpper();
            station.GlCashAccount = station.GlCashAccount?.Trim();
            station.GlSalesAccount = station.GlSalesAccount?.Trim();

            _context.PosStations.Add(station);
            await _context.SaveChangesAsync();

            return station;
        }

        /// <summary>
        /// تحديث محطة نقطة بيع
        /// </summary>
        public async Task<PosStation> UpdateAsync(PosStation station)
        {
            var existingStation = await _context.PosStations.FindAsync(station.PosId);
            if (existingStation == null)
            {
                throw new InvalidOperationException("محطة نقطة البيع غير موجودة");
            }

            // التحقق من عدم تكرار الكود (باستثناء المحطة الحالية)
            if (await _context.PosStations.AnyAsync(x => x.PosCode == station.PosCode && x.PosId != station.PosId))
            {
                throw new InvalidOperationException($"كود المحطة '{station.PosCode}' موجود بالفعل");
            }

            // تطبيع البيانات
            station.PosName = station.PosName?.Trim();
            station.PosCode = station.PosCode?.Trim().ToUpper();
            station.GlCashAccount = station.GlCashAccount?.Trim();
            station.GlSalesAccount = station.GlSalesAccount?.Trim();

            // تحديث البيانات
            existingStation.BranchId = station.BranchId;
            existingStation.PosName = station.PosName;
            existingStation.PosCode = station.PosCode;
            existingStation.GlCashAccount = station.GlCashAccount;
            existingStation.GlSalesAccount = station.GlSalesAccount;
            existingStation.AssignedUser = station.AssignedUser;
            existingStation.AllowedPaymentMethods = station.AllowedPaymentMethods;
            existingStation.UserPermissions = station.UserPermissions;
            existingStation.IsActive = station.IsActive;
            existingStation.UpdatedDate = DateTime.UtcNow;
            existingStation.CompanyId = station.CompanyId;

            await _context.SaveChangesAsync();
            return existingStation;
        }

        /// <summary>
        /// حذف محطة نقطة بيع
        /// </summary>
        public async Task<bool> DeleteAsync(int posId)
        {
            var station = await _context.PosStations.FindAsync(posId);
            if (station == null)
            {
                return false;
            }

            _context.PosStations.Remove(station);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// تغيير حالة محطة نقطة البيع
        /// </summary>
        public async Task<bool> ToggleActiveAsync(int posId)
        {
            var station = await _context.PosStations.FindAsync(posId);
            if (station == null)
            {
                return false;
            }

            station.IsActive = !station.IsActive;
            station.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// البحث في محطات نقاط البيع
        /// </summary>
        public async Task<List<PosStation>> SearchAsync(string searchTerm, int? companyId = null, int? branchId = null)
        {
            var query = _context.PosStations
                .Include(x => x.Branch)
                .Include(x => x.AssignedUserNavigation)
                .AsQueryable();

            if (companyId.HasValue)
                query = query.Where(x => x.CompanyId == companyId.Value);

            if (branchId.HasValue)
                query = query.Where(x => x.BranchId == branchId.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(x => 
                    x.PosName.Contains(searchTerm) ||
                    x.PosCode.Contains(searchTerm) ||
                    x.AssignedUserNavigation.Name.Contains(searchTerm) ||
                    x.Branch.BranchName.Contains(searchTerm));
            }

            var stations = await query
                .OrderBy(x => x.PosName)
                .ToListAsync();

            // تحميل البيانات المرتبطة للعرض
            foreach (var station in stations)
            {
                station.BranchName = station.Branch?.BranchName;
                station.AssignedUserName = station.AssignedUserNavigation?.Name;
            }

            return stations;
        }

        /// <summary>
        /// التحقق من صحة البيانات
        /// </summary>
        public async Task<bool> ValidateAsync(PosStation station)
        {
            if (string.IsNullOrWhiteSpace(station.PosName))
                return false;

            if (string.IsNullOrWhiteSpace(station.PosCode))
                return false;

            if (string.IsNullOrWhiteSpace(station.GlCashAccount))
                return false;

            if (string.IsNullOrWhiteSpace(station.GlSalesAccount))
                return false;

            if (station.BranchId <= 0)
                return false;

            if (station.AssignedUser <= 0)
                return false;

            // التحقق من وجود الفرع
            if (!await _context.Branches.AnyAsync(x => x.BranchId == station.BranchId))
                return false;

            // التحقق من وجود المستخدم
            if (!await _context.Nextuser.AnyAsync(x => x.ID == station.AssignedUser))
                return false;

            return true;
        }

        /// <summary>
        /// توليد كود نقطة بيع جديد
        /// </summary>
        public async Task<string> GenerateNextCodeAsync()
        {
            var lastStation = await _context.PosStations
                .OrderByDescending(x => x.PosCode)
                .FirstOrDefaultAsync();

            if (lastStation == null)
            {
                return "POS001";
            }

            // استخراج الرقم من آخر كود
            var lastCode = lastStation.PosCode;
            if (lastCode.StartsWith("POS") && int.TryParse(lastCode.Substring(3), out int lastNumber))
            {
                return $"POS{(lastNumber + 1):D3}";
            }

            // إذا لم يكن الكود بتنسيق POS###، نبدأ من POS001
            return "POS001";
        }

        public void Dispose()
        {
            if (_ownsContext)
            {
                _context?.Dispose();
            }
        }
    }
}
