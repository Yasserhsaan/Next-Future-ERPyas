namespace Next_Future_ERP.Core.Services;

/// <summary>
/// خدمة إدارة الصلاحيات - مبسطة للتطوير
/// </summary>
public class PermissionService
{
    /// <summary>
    /// التحقق من وجود صلاحية - يسمح بكل شيء حالياً
    /// </summary>
    public bool HasPermission(string permission)
    {
        // السماح بجميع الصلاحيات حالياً - بدون أي قيود
        return true;
    }
}
