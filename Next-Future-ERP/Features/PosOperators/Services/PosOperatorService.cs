using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.PosOperators.Models;
using Next_Future_ERP.Data.Models; // Added
using Next_Future_ERP.Features.PosStations.Models; // Added
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.PosOperators.Services
{
    public class PosOperatorService : IPosOperatorService, IDisposable
    {
        private readonly AppDbContext _context;
        private readonly bool _ownsContext;

        public PosOperatorService()
        {
            _context = new AppDbContext();
            _ownsContext = true;
        }

        public PosOperatorService(AppDbContext context)
        {
            _context = context;
            _ownsContext = false;
        }

        public async Task<List<PosOperator>> GetAllAsync(int? companyId = null, int? branchId = null, bool? isActive = null)
        {
            var query = _context.PosOperators
                                .Include(p => p.PosStation)
                                .Include(p => p.User)
                                .AsQueryable();

            if (companyId.HasValue)
                query = query.Where(x => x.CompanyId == companyId.Value);

            if (branchId.HasValue)
                query = query.Where(x => x.BranchId == branchId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            var operators = await query
                .OrderBy(x => x.PosStation.PosName)
                .ThenBy(x => x.User.Name)
                .ToListAsync();

            // تحميل البيانات المرتبطة للعرض
            foreach (var op in operators)
            {
                op.PosStationName = op.PosStation?.PosName;
                op.UserName = op.User?.Name;
            }

            return operators;
        }

        public async Task<PosOperator?> GetByIdAsync(int id)
        {
            var posOperator = await _context.PosOperators
                                     .Include(p => p.PosStation)
                                     .Include(p => p.User)
                                     .FirstOrDefaultAsync(p => p.OperatorId == id);

            if (posOperator != null)
            {
                posOperator.PosStationName = posOperator.PosStation?.PosName;
                posOperator.UserName = posOperator.User?.Name;
            }
            return posOperator;
        }

        public async Task<List<PosOperator>> GetByPosIdAsync(int posId)
        {
            var operators = await _context.PosOperators
                                    .Include(p => p.PosStation)
                                    .Include(p => p.User)
                                    .Where(p => p.PosId == posId)
                                    .OrderBy(x => x.IsPrimary ? 0 : 1)
                                    .ThenBy(x => x.User.Name)
                                    .ToListAsync();

            // تحميل البيانات المرتبطة للعرض
            foreach (var op in operators)
            {
                op.PosStationName = op.PosStation?.PosName;
                op.UserName = op.User?.Name;
            }

            return operators;
        }

        public async Task<List<PosOperator>> GetByUserIdAsync(int userId)
        {
            var operators = await _context.PosOperators
                                    .Include(p => p.PosStation)
                                    .Include(p => p.User)
                                    .Where(p => p.UserId == userId)
                                    .OrderBy(x => x.PosStation.PosName)
                                    .ToListAsync();

            // تحميل البيانات المرتبطة للعرض
            foreach (var op in operators)
            {
                op.PosStationName = op.PosStation?.PosName;
                op.UserName = op.User?.Name;
            }

            return operators;
        }

        public async Task<PosOperator> AddAsync(PosOperator posOperator)
        {
            if (!await ValidateAsync(posOperator))
            {
                throw new InvalidOperationException("بيانات مشغل نقطة البيع غير صالحة.");
            }

            posOperator.StartDate = posOperator.StartDate ?? DateTime.Now;
            posOperator.IsActive = posOperator.IsActive;

            _context.PosOperators.Add(posOperator);
            await _context.SaveChangesAsync();
            return posOperator;
        }

        public async Task<PosOperator> UpdateAsync(PosOperator posOperator)
        {
            if (!await ValidateAsync(posOperator))
            {
                throw new InvalidOperationException("بيانات مشغل نقطة البيع غير صالحة.");
            }

            var existingOperator = await _context.PosOperators.FindAsync(posOperator.OperatorId);
            if (existingOperator == null)
            {
                throw new KeyNotFoundException($"لم يتم العثور على مشغل نقطة البيع بالمعرف {posOperator.OperatorId}.");
            }

            existingOperator.PosId = posOperator.PosId;
            existingOperator.UserId = posOperator.UserId;
            existingOperator.IsPrimary = posOperator.IsPrimary;
            existingOperator.StartDate = posOperator.StartDate;
            existingOperator.EndDate = posOperator.EndDate;
            existingOperator.IsActive = posOperator.IsActive;
            existingOperator.CompanyId = posOperator.CompanyId;
            existingOperator.BranchId = posOperator.BranchId;

            _context.Entry(existingOperator).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return existingOperator;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var posOperator = await _context.PosOperators.FindAsync(id);
            if (posOperator == null)
            {
                throw new KeyNotFoundException($"لم يتم العثور على مشغل نقطة البيع بالمعرف {id}.");
            }

            _context.PosOperators.Remove(posOperator);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            var posOperator = await _context.PosOperators.FindAsync(id);
            if (posOperator == null)
            {
                throw new KeyNotFoundException($"لم يتم العثور على مشغل نقطة البيع بالمعرف {id}.");
            }

            posOperator.IsActive = !posOperator.IsActive;
            _context.Entry(posOperator).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PosOperator>> SearchAsync(string searchTerm, int? companyId = null, int? branchId = null)
        {
            var query = _context.PosOperators
                                .Include(p => p.PosStation)
                                .Include(p => p.User)
                                .AsQueryable();

            if (companyId.HasValue)
                query = query.Where(x => x.CompanyId == companyId.Value);

            if (branchId.HasValue && branchId.Value > 0)
                query = query.Where(x => x.BranchId == branchId.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(x =>
                    (x.PosStation != null && x.PosStation.PosName.Contains(searchTerm)) ||
                    (x.User != null && x.User.Name.Contains(searchTerm)) ||
                    (x.PosStation != null && x.PosStation.PosCode.Contains(searchTerm)));
            }

            var operators = await query
                .OrderBy(x => x.PosStation.PosName)
                .ThenBy(x => x.User.Name)
                .ToListAsync();

            // تحميل البيانات المرتبطة للعرض
            foreach (var op in operators)
            {
                op.PosStationName = op.PosStation?.PosName;
                op.UserName = op.User?.Name;
            }

            return operators;
        }

        public async Task<bool> ValidateAsync(PosOperator posOperator)
        {
            if (posOperator.PosId <= 0)
                return false;

            if (posOperator.UserId <= 0)
                return false;

            // التحقق من وجود نقطة البيع
            if (!await _context.PosStations.AnyAsync(x => x.PosId == posOperator.PosId))
                return false;

            // التحقق من وجود المستخدم
            if (!await _context.Nextuser.AnyAsync(x => x.ID == posOperator.UserId))
                return false;

            // التحقق من عدم تكرار نفس المستخدم لنفس نقطة البيع
            if (await _context.PosOperators.AnyAsync(x => x.PosId == posOperator.PosId && 
                                                          x.UserId == posOperator.UserId && 
                                                          x.OperatorId != posOperator.OperatorId))
                return false;

            return true;
        }

        public async Task<bool> SetPrimaryOperatorAsync(int posId, int operatorId)
        {
            // إلغاء تعيين جميع المشغلين الرئيسيين لنقطة البيع هذه
            var existingPrimary = await _context.PosOperators
                .Where(x => x.PosId == posId && x.IsPrimary)
                .ToListAsync();

            foreach (var op in existingPrimary)
            {
                op.IsPrimary = false;
                _context.Entry(op).State = EntityState.Modified;
            }

            // تعيين المشغل الجديد كرئيسي
            var newPrimary = await _context.PosOperators
                .FirstOrDefaultAsync(x => x.PosId == posId && x.OperatorId == operatorId);

            if (newPrimary != null)
            {
                newPrimary.IsPrimary = true;
                _context.Entry(newPrimary).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return true;
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
