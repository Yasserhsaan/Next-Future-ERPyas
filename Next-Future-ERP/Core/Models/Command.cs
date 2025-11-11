using System.Windows.Input;

namespace Next_Future_ERP.Core.Models;

/// <summary>
/// نموذج الأمر في شريط الأوامر
/// </summary>
public class Command
{
    /// <summary>
    /// معرف الأمر الفريد
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// اسم الأمر باللغة العربية
    /// </summary>
    public string NameAr { get; set; } = string.Empty;

    /// <summary>
    /// اسم الأمر باللغة الإنجليزية
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// الأيقونة (Emoji أو Unicode)
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// اختصار لوحة المفاتيح
    /// </summary>
    public string Shortcut { get; set; } = string.Empty;

    /// <summary>
    /// نص المساعدة
    /// </summary>
    public string Tooltip { get; set; } = string.Empty;

    /// <summary>
    /// مجموعة الأمر
    /// </summary>
    public CommandGroup Group { get; set; } = CommandGroup.Core;

    /// <summary>
    /// لون الأمر (Hex)
    /// </summary>
    public string Color { get; set; } = "#FF6B6B";

    /// <summary>
    /// هل الأمر مفعل؟
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// هل الأمر مرئي؟
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// الصلاحية المطلوبة
    /// </summary>
    public string? RequiredPermission { get; set; }

    /// <summary>
    /// ترتيب الأمر في المجموعة
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// الأمر القابل للتنفيذ
    /// </summary>
    public ICommand? ExecuteCommand { get; set; }

    /// <summary>
    /// هل يحتاج الأمر لتأكيد؟
    /// </summary>
    public bool RequiresConfirmation { get; set; } = false;

    /// <summary>
    /// رسالة التأكيد
    /// </summary>
    public string? ConfirmationMessage { get; set; }
}
