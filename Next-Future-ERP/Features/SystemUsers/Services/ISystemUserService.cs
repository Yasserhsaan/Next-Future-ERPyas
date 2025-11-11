using Next_Future_ERP.Features.SystemUsers.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.SystemUsers.Services
{
    /// <summary>
    /// واجهة خدمة مستخدمي النظام
    /// </summary>
    public interface ISystemUserService
    {
        /// <summary>
        /// الحصول على جميع مستخدمي النظام
        /// </summary>
        Task<List<SystemUser>> GetAllAsync(int? companyId = null, int? branchId = null, bool? isActive = null);

        /// <summary>
        /// الحصول على مستخدم نظام بالمعرف
        /// </summary>
        Task<SystemUser?> GetByIdAsync(int id);

        /// <summary>
        /// الحصول على مستخدمي النظام حسب الفرع
        /// </summary>
        Task<List<SystemUser>> GetByBranchIdAsync(int branchId);

        /// <summary>
        /// الحصول على مستخدمي النظام حسب الدور
        /// </summary>
        Task<List<SystemUser>> GetByRoleIdAsync(int roleId);

        /// <summary>
        /// إضافة مستخدم نظام جديد
        /// </summary>
        Task<SystemUser> AddAsync(SystemUser systemUser);

        /// <summary>
        /// تحديث مستخدم نظام
        /// </summary>
        Task<SystemUser> UpdateAsync(SystemUser systemUser);

        /// <summary>
        /// حذف مستخدم نظام
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// تبديل حالة القفل/الفتح
        /// </summary>
        Task<bool> ToggleLockAsync(int id);

        /// <summary>
        /// البحث في مستخدمي النظام
        /// </summary>
        Task<List<SystemUser>> SearchAsync(string searchTerm, int? companyId = null, int? branchId = null);

        /// <summary>
        /// التحقق من صحة بيانات مستخدم النظام
        /// </summary>
        Task<bool> ValidateAsync(SystemUser systemUser);

        /// <summary>
        /// إعادة تعيين كلمة المرور
        /// </summary>
        Task<bool> ResetPasswordAsync(int id, string newPassword);

        /// <summary>
        /// تحديث آخر تسجيل دخول
        /// </summary>
        Task<bool> UpdateLastLoginAsync(int id);

        /// <summary>
        /// زيادة عدد محاولات تسجيل الدخول الفاشلة
        /// </summary>
        Task<bool> IncrementFailedLoginAttemptsAsync(int id);

        /// <summary>
        /// إعادة تعيين عدد محاولات تسجيل الدخول الفاشلة
        /// </summary>
        Task<bool> ResetFailedLoginAttemptsAsync(int id);

        /// <summary>
        /// قفل المستخدم مؤقتاً
        /// </summary>
        Task<bool> LockUserAsync(int id, DateTime lockoutEnd);

        /// <summary>
        /// فتح المستخدم
        /// </summary>
        Task<bool> UnlockUserAsync(int id);
    }
}
