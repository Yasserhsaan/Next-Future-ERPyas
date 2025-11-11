using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Data.Models;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Data.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService()
        {
            _context = DbContextFactory.Create();
        }

        public async Task SaveAdminUserAsync(Nextuser adminUser)
        {
            if (string.IsNullOrWhiteSpace(adminUser.Name))
                throw new ArgumentException("اسم المستخدم لا يمكن أن يكون فارغاً");

            if (string.IsNullOrWhiteSpace(adminUser.Fname))
                throw new ArgumentException("الاسم الكامل لا يمكن أن يكون فارغاً");

            if (string.IsNullOrWhiteSpace(adminUser.Password))
                throw new ArgumentException("كلمة المرور لا يمكن أن تكون فارغة");

            // Hash the password
            var (passwordHash, passwordSalt) = HashPassword(adminUser.Password);
            adminUser.PasswordHash = passwordHash;
            adminUser.PasswordSalt = passwordSalt;
            adminUser.Password = string.Empty; // Don't store plain text password

            // Set default values
            adminUser.UserRollid = 1; // Admin role
            adminUser.Nsync = 0;
            adminUser.LastLogin = DateTime.Now;

            // Check if user already exists
            var existing = await _context.Nextuser.FirstOrDefaultAsync(u => u.Name == adminUser.Name);
            if (existing != null)
            {
                // Update existing
                existing.Fname = adminUser.Fname;
                existing.Mobile = adminUser.Mobile;
                existing.Phone = adminUser.Phone;
                existing.Address = adminUser.Address;
                existing.Email = adminUser.Email;
                existing.PasswordHash = adminUser.PasswordHash;
                existing.PasswordSalt = adminUser.PasswordSalt;
                existing.UserRollid = adminUser.UserRollid;
            }
            else
            {
                // Add new
                _context.Nextuser.Add(adminUser);
            }

            await _context.SaveChangesAsync();
        }

        private (string hash, string salt) HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var salt = Guid.NewGuid().ToString();
            var combined = password + salt;
            var bytes = Encoding.UTF8.GetBytes(combined);
            var hash = sha256.ComputeHash(bytes);
            return (Convert.ToBase64String(hash), salt);
        }
    }
} 