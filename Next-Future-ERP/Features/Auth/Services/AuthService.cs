using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Models;
using System.Security.Cryptography;
using System.Text;

namespace Next_Future_ERP.Features.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ISessionService _sessionService;

        public AuthService(AppDbContext context, ISessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        public async Task<AuthResult> LoginAsync(string username, string password, int? companyId = null, int? branchId = null)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Message = "يرجى إدخال اسم المستخدم وكلمة المرور"
                    };
                }

                // Find user by username (Name, Code, or Email)
                var user = await _context.Nextuser
                    .FirstOrDefaultAsync(u => 
                        u.Name == username || 
                        u.Code == username || 
                        u.Email == username);

                if (user == null)
                {
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Message = "اسم المستخدم غير صحيح"
                    };
                }

                // Check if user is active (you might want to add an IsActive field to your model)
                // if (!user.IsActive) { ... }

                // Debug: Log password information (remove this in production)
                Console.WriteLine($"🔍 Debug Info:");
                Console.WriteLine($"   Input Password: {password}");
                Console.WriteLine($"   Stored Password: {user.Password}");
                Console.WriteLine($"   Stored Salt: {user.PasswordSalt}");
                Console.WriteLine($"   Password Length: {user.Password?.Length ?? 0}");
                
                // Verify password
                var passwordValid = VerifyPassword(password, user.Password, user.PasswordSalt);
                Console.WriteLine($"   Password Valid: {passwordValid}");
                
                if (!passwordValid)
                {
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Message = "كلمة المرور غير صحيحة"
                    };
                }

                // Update last login timestamp
                await UpdateLastLoginAsync(user.ID);

                // Initialize user session with permissions
                var sessionInitialized = await _sessionService.InitializeSessionAsync(user.ID, companyId, branchId);
                if (!sessionInitialized)
                {
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Message = "حدث خطأ أثناء تهيئة جلسة المستخدم"
                    };
                }

                return new AuthResult
                {
                    IsSuccess = true,
                    Message = "تم تسجيل الدخول بنجاح",
                    User = user
                };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    Message = $"حدث خطأ أثناء تسجيل الدخول: {ex.Message}"
                };
            }
        }

        public void Logout()
        {
            _sessionService.ClearSession();
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            return await _context.Nextuser
                .AnyAsync(u => 
                    u.Name == username || 
                    u.Code == username || 
                    u.Email == username);
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            try
            {
                var user = await _context.Nextuser.FindAsync(userId);
                if (user != null)
                {
                    user.LastLogin = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Nextuser?> GetUserByIdAsync(int userId)
        {
            return await _context.Nextuser.FindAsync(userId);
        }

        /// <summary>
        /// Verifies a password against stored hash and salt
        /// </summary>
        private bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            try
            {
                // If no salt is stored, assume plain text password (for backward compatibility)
                if (string.IsNullOrEmpty(storedSalt))
                {
                    return password == storedHash;
                }

                // Check if the stored hash is actually a hash or plain text
                // If it's a hash, verify with salt. If it's plain text, compare directly
                if (storedHash.Length > 20) // Likely a hash
                {
                    var hashedPassword = HashPassword(password, storedSalt);
                    return hashedPassword == storedHash;
                }
                else
                {
                    // Plain text password stored
                    return password == storedHash;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Hashes a password with a salt
        /// </summary>
        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var combinedBytes = Encoding.UTF8.GetBytes(password + salt);
                var hashBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Generates a random salt for password hashing
        /// </summary>
        public static string GenerateSalt()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Creates a new user with hashed password
        /// </summary>
        public async Task<bool> CreateUserAsync(Nextuser user, string plainPassword)
        {
            try
            {
                var salt = GenerateSalt();
                var hashedPassword = HashPassword(plainPassword, salt);

                user.PasswordSalt = salt;
                user.PasswordHash = hashedPassword;
                user.Password = hashedPassword; // Keep for backward compatibility
                user.LastLogin = DateTime.Now;

                _context.Nextuser.Add(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
