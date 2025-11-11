namespace Next_Future_ERP.Core.Models;

/// <summary>
/// مجموعات أوامر شريط الأوامر
/// </summary>
public enum CommandGroup
{
    /// <summary>
    /// الأوامر الأساسية - Core Commands
    /// NEW, SAVE, EDIT, DELETE, REFRESH
    /// </summary>
    Core = 1,

    /// <summary>
    /// البحث والفلترة - Search & Filter
    /// SEARCH, FILTER, REPORT, EXPORT, PRINT
    /// </summary>
    Search = 2,

    /// <summary>
    /// العمليات السريعة - Quick Actions
    /// IMPORT, BACKUP, LINK, APPROVE, REJECT
    /// </summary>
    Quick = 3,

    /// <summary>
    /// أوامر الذكاء الاصطناعي - AI Commands
    /// AI HELP, PREDICT, ANALYZE, SUGGEST, AUTOMATE
    /// </summary>
    AI = 4,

    /// <summary>
    /// أوامر مخصصة - Custom Commands
    /// للأوامر الخاصة بكل شاشة
    /// </summary>
    Custom = 5
}

/// <summary>
/// معلومات مجموعة الأوامر
/// </summary>
public static class CommandGroupInfo
{
    /// <summary>
    /// الحصول على معلومات المجموعة
    /// </summary>
    public static (string NameAr, string NameEn, string Color, string Icon) GetGroupInfo(CommandGroup group)
    {
        return group switch
        {
            CommandGroup.Core => ("أساسي", "CORE", "#FF6B6B", "🔥"),
            CommandGroup.Search => ("بحث", "SEARCH", "#4ECDC4", "🔍"),
            CommandGroup.Quick => ("سريع", "QUICK", "#FFE66D", "⚡"),
            CommandGroup.AI => ("ذكي", "AI", "#A8E6CF", "🤖"),
            CommandGroup.Custom => ("مخصص", "CUSTOM", "#B19CD9", "🎯"),
            _ => ("غير محدد", "UNKNOWN", "#999999", "❓")
        };
    }

    /// <summary>
    /// الحصول على لون المجموعة
    /// </summary>
    public static string GetGroupColor(CommandGroup group)
    {
        return GetGroupInfo(group).Color;
    }

    /// <summary>
    /// الحصول على أيقونة المجموعة
    /// </summary>
    public static string GetGroupIcon(CommandGroup group)
    {
        return GetGroupInfo(group).Icon;
    }
}
