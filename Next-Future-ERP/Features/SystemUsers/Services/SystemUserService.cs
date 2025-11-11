using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Models;
using Next_Future_ERP.Features.SystemUsers.Models;
using Next_Future_ERP.Features.Permissions.Models;
using Next_Future_ERP.Features.InitialSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace Next_Future_ERP.Features.SystemUsers.Services
{
    public class SystemUserService : ISystemUserService, IDisposable
    {
        private readonly AppDbContext _context;
        private readonly bool _ownsContext;

        public SystemUserService()
        {
            _context = new AppDbContext();
            _ownsContext = true;
        }

        public SystemUserService(AppDbContext context)
        {
            _context = context;
            _ownsContext = false;
        }

        public async Task<List<SystemUser>> GetAllAsync(int? companyId = null, int? branchId = null, bool? isActive = null)
        {
            var query = _context.Nextuser.AsQueryable();

            // ملاحظة: Nextuser model لا يحتوي على BranchId أو IsLocked
            // لذلك سنتجاهل هذه الفلاتر في الوقت الحالي

            var users = await query.OrderBy(x => x.Name).ToListAsync();

            // تحويل Nextuser إلى SystemUser
            var systemUsers = users.Select(u => new SystemUser
            {
                Id = u.ID,
                Code = u.Code,
                Name = u.Name,
                FirstName = u.Fname,
                Mobile = u.Mobile,
                Phone = u.Phone,
                Address = u.Address,
                Email = u.Email,
                Password = u.Password,
                Nsync = u.Nsync,
                DbTimestamp = u.Dbtimestamp,
                UserRoleId = u.UserRollid,
                PasswordHash = u.PasswordHash,
                PasswordSalt = u.PasswordSalt,
                LastLogin = u.LastLogin,
                FailedLoginAttempts = 0, // قيمة افتراضية
                IsLocked = false, // قيمة افتراضية
                LockoutEnd = null, // قيمة افتراضية
                UserJob = null, // قيمة افتراضية
                BranchId = null // قيمة افتراضية
            }).ToList();

            return systemUsers;
        }

        public async Task<SystemUser?> GetByIdAsync(int id)
        {
            var user = await _context.Nextuser.FirstOrDefaultAsync(u => u.ID == id);
            if (user == null) return null;

            return new SystemUser
            {
                Id = user.ID,
                Code = user.Code,
                Name = user.Name,
                FirstName = user.Fname,
                Mobile = user.Mobile,
                Phone = user.Phone,
                Address = user.Address,
                Email = user.Email,
                Password = user.Password,
                Nsync = user.Nsync,
                DbTimestamp = user.Dbtimestamp,
                UserRoleId = user.UserRollid,
                PasswordHash = user.PasswordHash,
                PasswordSalt = user.PasswordSalt,
                LastLogin = user.LastLogin,
                FailedLoginAttempts = 0, // قيمة افتراضية
                IsLocked = false, // قيمة افتراضية
                LockoutEnd = null, // قيمة افتراضية
                UserJob = null, // قيمة افتراضية
                BranchId = null // قيمة افتراضية
            };
        }

        public async Task<List<SystemUser>> GetByBranchIdAsync(int branchId)
        {
            // ملاحظة: Nextuser model لا يحتوي على BranchId
            // لذلك سنعيد جميع المستخدمين في الوقت الحالي
            var users = await _context.Nextuser
                .OrderBy(x => x.Name)
                .ToListAsync();

            return users.Select(u => new SystemUser
            {
                Id = u.ID,
                Code = u.Code,
                Name = u.Name,
                FirstName = u.Fname,
                Mobile = u.Mobile,
                Phone = u.Phone,
                Address = u.Address,
                Email = u.Email,
                Password = u.Password,
                Nsync = u.Nsync,
                DbTimestamp = u.Dbtimestamp,
                UserRoleId = u.UserRollid,
                PasswordHash = u.PasswordHash,
                PasswordSalt = u.PasswordSalt,
                LastLogin = u.LastLogin,
                FailedLoginAttempts = 0, // قيمة افتراضية
                IsLocked = false, // قيمة افتراضية
                LockoutEnd = null, // قيمة افتراضية
                UserJob = null, // قيمة افتراضية
                BranchId = null // قيمة افتراضية
            }).ToList();
        }

        public async Task<List<SystemUser>> GetByRoleIdAsync(int roleId)
        {
            var users = await _context.Nextuser
                .Where(u => u.UserRollid == roleId)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return users.Select(u => new SystemUser
            {
                Id = u.ID,
                Code = u.Code,
                Name = u.Name,
                FirstName = u.Fname,
                Mobile = u.Mobile,
                Phone = u.Phone,
                Address = u.Address,
                Email = u.Email,
                Password = u.Password,
                Nsync = u.Nsync,
                DbTimestamp = u.Dbtimestamp,
                UserRoleId = u.UserRollid,
                PasswordHash = u.PasswordHash,
                PasswordSalt = u.PasswordSalt,
                LastLogin = u.LastLogin,
                FailedLoginAttempts = 0, // قيمة افتراضية
                IsLocked = false, // قيمة افتراضية
                LockoutEnd = null, // قيمة افتراضية
                UserJob = null, // قيمة افتراضية
                BranchId = null // قيمة افتراضية
            }).ToList();
        }

        public async Task<SystemUser> AddAsync(SystemUser systemUser)
        {
            if (!await ValidateAsync(systemUser))
            {
                throw new InvalidOperationException("بيانات مستخدم النظام غير صالحة.");
            }

            var user = new Nextuser
            {
                Code = systemUser.Code,
                Name = systemUser.Name,
                Fname = systemUser.FirstName,
                Mobile = systemUser.Mobile,
                Phone = systemUser.Phone,
                Address = systemUser.Address,
                Email = systemUser.Email,
                Password = systemUser.Password,
                Nsync = systemUser.Nsync,
                UserRollid = systemUser.UserRoleId ?? 0,
                PasswordHash = systemUser.PasswordHash,
                PasswordSalt = systemUser.PasswordSalt,
                LastLogin = systemUser.LastLogin ?? DateTime.Now
            };

            // تشفير كلمة المرور
            if (!string.IsNullOrEmpty(systemUser.Password))
            {
                var (hash, salt) = HashPassword(systemUser.Password);
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
            }

            // هذه الخصائص غير متوفرة في Nextuser model حالياً
            // user.FailedLoginAttempts = 0;
            // user.IsLocked = false;
            // user.LastLogin = null;

            _context.Nextuser.Add(user);
            await _context.SaveChangesAsync();

            systemUser.Id = user.ID;
            return systemUser;
        }

        public async Task<SystemUser> UpdateAsync(SystemUser systemUser)
        {
            if (!await ValidateAsync(systemUser))
            {
                throw new InvalidOperationException("بيانات مستخدم النظام غير صالحة.");
            }

            var existingUser = await _context.Nextuser.FindAsync(systemUser.Id);
            if (existingUser == null)
            {
                throw new KeyNotFoundException($"لم يتم العثور على مستخدم النظام بالمعرف {systemUser.Id}.");
            }

            existingUser.Code = systemUser.Code;
            existingUser.Name = systemUser.Name;
            existingUser.Fname = systemUser.FirstName;
            existingUser.Mobile = systemUser.Mobile;
            existingUser.Phone = systemUser.Phone;
            existingUser.Address = systemUser.Address;
            existingUser.Email = systemUser.Email;
            existingUser.UserRollid = systemUser.UserRoleId ?? 0;

            // تحديث كلمة المرور إذا تم توفيرها
            if (!string.IsNullOrEmpty(systemUser.Password))
            {
                var (hash, salt) = HashPassword(systemUser.Password);
                existingUser.PasswordHash = hash;
                existingUser.PasswordSalt = salt;
            }

            _context.Entry(existingUser).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return systemUser;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Nextuser.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"لم يتم العثور على مستخدم النظام بالمعرف {id}.");
            }

            _context.Nextuser.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleLockAsync(int id)
        {
            // ملاحظة: Nextuser model لا يحتوي على IsLocked
            // لذلك سنعيد true فقط في الوقت الحالي
            var user = await _context.Nextuser.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"لم يتم العثور على مستخدم النظام بالمعرف {id}.");
            }

            // في المستقبل يمكن إضافة عمود IsLocked إلى جدول Nextuser
            return true;
        }

        public async Task<List<SystemUser>> SearchAsync(string searchTerm, int? companyId = null, int? branchId = null)
        {
            var query = _context.Nextuser.AsQueryable();

            // ملاحظة: Nextuser model لا يحتوي على BranchId
            // لذلك سنتجاهل هذا الفلتر في الوقت الحالي

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(x =>
                    x.Name.Contains(searchTerm) ||
                    x.Fname.Contains(searchTerm) ||
                    x.Code.Contains(searchTerm) ||
                    x.Email.Contains(searchTerm) ||
                    x.Mobile.Contains(searchTerm));
            }

            var users = await query.OrderBy(x => x.Name).ToListAsync();

            return users.Select(u => new SystemUser
            {
                Id = u.ID,
                Code = u.Code,
                Name = u.Name,
                FirstName = u.Fname,
                Mobile = u.Mobile,
                Phone = u.Phone,
                Address = u.Address,
                Email = u.Email,
                Password = u.Password,
                Nsync = u.Nsync,
                DbTimestamp = u.Dbtimestamp,
                UserRoleId = u.UserRollid,
                PasswordHash = u.PasswordHash,
                PasswordSalt = u.PasswordSalt,
                LastLogin = u.LastLogin,
                FailedLoginAttempts = 0, // قيمة افتراضية
                IsLocked = false, // قيمة افتراضية
                LockoutEnd = null, // قيمة افتراضية
                UserJob = null, // قيمة افتراضية
                BranchId = null // قيمة افتراضية
            }).ToList();
        }

        public async Task<bool> ValidateAsync(SystemUser systemUser)
        {
            if (string.IsNullOrWhiteSpace(systemUser.Code))
                return false;

            if (string.IsNullOrWhiteSpace(systemUser.Name))
                return false;

            if (string.IsNullOrWhiteSpace(systemUser.FirstName))
                return false;

            if (string.IsNullOrWhiteSpace(systemUser.Email))
                return false;

            // التحقق من عدم تكرار الكود
            if (await _context.Nextuser.AnyAsync(x => x.Code == systemUser.Code && x.ID != systemUser.Id))
                return false;

            // التحقق من عدم تكرار البريد الإلكتروني
            if (await _context.Nextuser.AnyAsync(x => x.Email == systemUser.Email && x.ID != systemUser.Id))
                return false;

            return true;
        }

        public async Task<bool> ResetPasswordAsync(int id, string newPassword)
        {
            var user = await _context.Nextuser.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"لم يتم العثور على مستخدم النظام بالمعرف {id}.");
            }

            var (hash, salt) = HashPassword(newPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLastLoginAsync(int id)
        {
            var user = await _context.Nextuser.FindAsync(id);
            if (user == null)
                return false;

            user.LastLogin = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementFailedLoginAttemptsAsync(int id)
        {
            // ملاحظة: Nextuser model لا يحتوي على FailedLoginAttempts
            // لذلك سنعيد true فقط في الوقت الحالي
            var user = await _context.Nextuser.FindAsync(id);
            if (user == null)
                return false;

            // في المستقبل يمكن إضافة عمود FailedLoginAttempts إلى جدول Nextuser
            return true;
        }

        public async Task<bool> ResetFailedLoginAttemptsAsync(int id)
        {
            // ملاحظة: Nextuser model لا يحتوي على FailedLoginAttempts
            // لذلك سنعيد true فقط في الوقت الحالي
            var user = await _context.Nextuser.FindAsync(id);
            if (user == null)
                return false;

            // في المستقبل يمكن إضافة عمود FailedLoginAttempts إلى جدول Nextuser
            return true;
        }

        public async Task<bool> LockUserAsync(int id, DateTime lockoutEnd)
        {
            // ملاحظة: Nextuser model لا يحتوي على IsLocked أو LockoutEnd
            // لذلك سنعيد true فقط في الوقت الحالي
            var user = await _context.Nextuser.FindAsync(id);
            if (user == null)
                return false;

            // في المستقبل يمكن إضافة أعمدة IsLocked و LockoutEnd إلى جدول Nextuser
            return true;
        }

        public async Task<bool> UnlockUserAsync(int id)
        {
            // ملاحظة: Nextuser model لا يحتوي على IsLocked أو LockoutEnd
            // لذلك سنعيد true فقط في الوقت الحالي
            var user = await _context.Nextuser.FindAsync(id);
            if (user == null)
                return false;

            // في المستقبل يمكن إضافة أعمدة IsLocked و LockoutEnd إلى جدول Nextuser
            return true;
        }

        private (string hash, string salt) HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var saltBytes = new byte[32];
            rng.GetBytes(saltBytes);
            var salt = Convert.ToBase64String(saltBytes);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000);
            var hashBytes = pbkdf2.GetBytes(32);
            var hash = Convert.ToBase64String(hashBytes);

            return (hash, salt);
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