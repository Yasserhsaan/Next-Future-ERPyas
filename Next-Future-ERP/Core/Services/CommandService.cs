using System.Collections.ObjectModel;
using Next_Future_ERP.Core.Models;

namespace Next_Future_ERP.Core.Services;

/// <summary>
/// خدمة إدارة أوامر شريط الأوامر
/// </summary>
public class CommandService
{
    private readonly ObservableCollection<Command> _commands = new();
    private readonly PermissionService _permissionService;

    public CommandService(PermissionService permissionService)
    {
        _permissionService = permissionService;
        InitializeDefaultCommands();
    }

    /// <summary>
    /// جميع الأوامر المتاحة
    /// </summary>
    public ObservableCollection<Command> Commands => _commands;

    /// <summary>
    /// الحصول على أوامر مجموعة معينة
    /// </summary>
    public IEnumerable<Command> GetCommandsByGroup(CommandGroup group)
    {
        return _commands
            .Where(c => c.Group == group && c.IsVisible && HasPermission(c))
            .OrderBy(c => c.Order);
    }

    /// <summary>
    /// الحصول على أمر بالمعرف
    /// </summary>
    public Command? GetCommand(string id)
    {
        return _commands.FirstOrDefault(c => c.Id == id);
    }

    /// <summary>
    /// إضافة أمر جديد
    /// </summary>
    public void AddCommand(Command command)
    {
        if (_commands.Any(c => c.Id == command.Id))
        {
            throw new InvalidOperationException($"Command with ID '{command.Id}' already exists.");
        }

        _commands.Add(command);
    }

    /// <summary>
    /// حذف أمر
    /// </summary>
    public void RemoveCommand(string id)
    {
        var command = GetCommand(id);
        if (command != null)
        {
            _commands.Remove(command);
        }
    }

    /// <summary>
    /// تحديث حالة الأمر
    /// </summary>
    public void UpdateCommandState(string id, bool isEnabled, bool isVisible = true)
    {
        var command = GetCommand(id);
        if (command != null)
        {
            command.IsEnabled = isEnabled;
            command.IsVisible = isVisible;
        }
    }

    /// <summary>
    /// التحقق من صلاحية الأمر
    /// </summary>
    private bool HasPermission(Command command)
    {
        if (string.IsNullOrEmpty(command.RequiredPermission))
            return true;

        return _permissionService.HasPermission(command.RequiredPermission);
    }

    /// <summary>
    /// تهيئة الأوامر الافتراضية
    /// </summary>
    private void InitializeDefaultCommands()
    {
        // 🔥 الأوامر الأساسية - Core Commands
        var coreCommands = new[]
        {
            new Command
            {
                Id = "new",
                NameAr = "جديد",
                NameEn = "NEW",
                Icon = "🆕",
                Shortcut = "Ctrl+N",
                Tooltip = "إنشاء سجل جديد",
                Group = CommandGroup.Core,
                Color = "#FF6B6B",
                Order = 1,
                RequiredPermission = "create"
            },
            new Command
            {
                Id = "save",
                NameAr = "حفظ",
                NameEn = "SAVE",
                Icon = "💾",
                Shortcut = "Ctrl+S",
                Tooltip = "حفظ التغييرات",
                Group = CommandGroup.Core,
                Color = "#FF6B6B",
                Order = 2,
                RequiredPermission = "update"
            },
            new Command
            {
                Id = "edit",
                NameAr = "تعديل",
                NameEn = "EDIT",
                Icon = "📝",
                Shortcut = "F2",
                Tooltip = "تعديل السجل المحدد",
                Group = CommandGroup.Core,
                Color = "#FF6B6B",
                Order = 3,
                RequiredPermission = "update"
            },
            new Command
            {
                Id = "delete",
                NameAr = "حذف",
                NameEn = "DELETE",
                Icon = "🗑️",
                Shortcut = "Delete",
                Tooltip = "حذف السجل المحدد",
                Group = CommandGroup.Core,
                Color = "#FF6B6B",
                Order = 4,
                RequiredPermission = "delete",
                RequiresConfirmation = true,
                ConfirmationMessage = "هل أنت متأكد من حذف هذا السجل؟"
            },
            new Command
            {
                Id = "refresh",
                NameAr = "تحديث",
                NameEn = "REFRESH",
                Icon = "🔄",
                Shortcut = "F5",
                Tooltip = "تحديث البيانات",
                Group = CommandGroup.Core,
                Color = "#FF6B6B",
                Order = 5
            }
        };

        // 🔍 أوامر البحث والفلترة - Search & Filter
        var searchCommands = new[]
        {
            new Command
            {
                Id = "search",
                NameAr = "بحث",
                NameEn = "SEARCH",
                Icon = "🔍",
                Shortcut = "Ctrl+F",
                Tooltip = "البحث في السجلات",
                Group = CommandGroup.Search,
                Color = "#4ECDC4",
                Order = 1
            },
            new Command
            {
                Id = "filter",
                NameAr = "فلتر",
                NameEn = "FILTER",
                Icon = "🎯",
                Shortcut = "Ctrl+L",
                Tooltip = "تطبيق فلاتر متقدمة",
                Group = CommandGroup.Search,
                Color = "#4ECDC4",
                Order = 2
            },
            new Command
            {
                Id = "report",
                NameAr = "تقرير",
                NameEn = "REPORT",
                Icon = "📊",
                Shortcut = "Ctrl+R",
                Tooltip = "إنشاء تقرير",
                Group = CommandGroup.Search,
                Color = "#4ECDC4",
                Order = 3,
                RequiredPermission = "reports"
            },
            new Command
            {
                Id = "export",
                NameAr = "تصدير",
                NameEn = "EXPORT",
                Icon = "📋",
                Shortcut = "Ctrl+E",
                Tooltip = "تصدير البيانات",
                Group = CommandGroup.Search,
                Color = "#4ECDC4",
                Order = 4,
                RequiredPermission = "export"
            },
            new Command
            {
                Id = "print",
                NameAr = "طباعة",
                NameEn = "PRINT",
                Icon = "🖨️",
                Shortcut = "Ctrl+P",
                Tooltip = "طباعة البيانات",
                Group = CommandGroup.Search,
                Color = "#4ECDC4",
                Order = 5,
                RequiredPermission = "print"
            }
        };

        // ⚡ العمليات السريعة - Quick Actions
        var quickCommands = new[]
        {
            new Command
            {
                Id = "import",
                NameAr = "استيراد",
                NameEn = "IMPORT",
                Icon = "📤",
                Shortcut = "Ctrl+I",
                Tooltip = "استيراد بيانات",
                Group = CommandGroup.Quick,
                Color = "#FFE66D",
                Order = 1,
                RequiredPermission = "import"
            },
            new Command
            {
                Id = "backup",
                NameAr = "نسخ احتياطي",
                NameEn = "BACKUP",
                Icon = "📥",
                Shortcut = "Ctrl+B",
                Tooltip = "إنشاء نسخة احتياطية",
                Group = CommandGroup.Quick,
                Color = "#FFE66D",
                Order = 2,
                RequiredPermission = "backup"
            },
            new Command
            {
                Id = "link",
                NameAr = "ربط",
                NameEn = "LINK",
                Icon = "🔗",
                Shortcut = "Ctrl+K",
                Tooltip = "ربط مع سجل آخر",
                Group = CommandGroup.Quick,
                Color = "#FFE66D",
                Order = 3
            },
            new Command
            {
                Id = "approve",
                NameAr = "موافقة",
                NameEn = "APPROVE",
                Icon = "✅",
                Shortcut = "Ctrl+Y",
                Tooltip = "الموافقة على السجل",
                Group = CommandGroup.Quick,
                Color = "#FFE66D",
                Order = 4,
                RequiredPermission = "approve"
            },
            new Command
            {
                Id = "reject",
                NameAr = "رفض",
                NameEn = "REJECT",
                Icon = "❌",
                Shortcut = "Ctrl+X",
                Tooltip = "رفض السجل",
                Group = CommandGroup.Quick,
                Color = "#FFE66D",
                Order = 5,
                RequiredPermission = "reject"
            }
        };

        // 🤖 أوامر الذكاء الاصطناعي - AI Commands
        var aiCommands = new[]
        {
            new Command
            {
                Id = "ai_help",
                NameAr = "مساعد ذكي",
                NameEn = "AI HELP",
                Icon = "🧠",
                Shortcut = "F1",
                Tooltip = "الحصول على مساعدة ذكية",
                Group = CommandGroup.AI,
                Color = "#A8E6CF",
                Order = 1
            },
            new Command
            {
                Id = "predict",
                NameAr = "توقع",
                NameEn = "PREDICT",
                Icon = "🔮",
                Shortcut = "Ctrl+Alt+P",
                Tooltip = "التنبؤ الذكي",
                Group = CommandGroup.AI,
                Color = "#A8E6CF",
                Order = 2,
                RequiredPermission = "ai_predict"
            },
            new Command
            {
                Id = "analyze",
                NameAr = "تحليل",
                NameEn = "ANALYZE",
                Icon = "📈",
                Shortcut = "Ctrl+Alt+A",
                Tooltip = "تحليل ذكي للبيانات",
                Group = CommandGroup.AI,
                Color = "#A8E6CF",
                Order = 3,
                RequiredPermission = "ai_analyze"
            },
            new Command
            {
                Id = "suggest",
                NameAr = "اقتراحات",
                NameEn = "SUGGEST",
                Icon = "🎨",
                Shortcut = "Ctrl+Alt+S",
                Tooltip = "اقتراحات ذكية",
                Group = CommandGroup.AI,
                Color = "#A8E6CF",
                Order = 4,
                RequiredPermission = "ai_suggest"
            },
            new Command
            {
                Id = "automate",
                NameAr = "أتمتة",
                NameEn = "AUTOMATE",
                Icon = "🚀",
                Shortcut = "Ctrl+Alt+M",
                Tooltip = "أتمتة العمليات",
                Group = CommandGroup.AI,
                Color = "#A8E6CF",
                Order = 5,
                RequiredPermission = "ai_automate"
            }
        };

        // إضافة جميع الأوامر
        foreach (var command in coreCommands.Concat(searchCommands).Concat(quickCommands).Concat(aiCommands))
        {
            _commands.Add(command);
        }
    }
}
