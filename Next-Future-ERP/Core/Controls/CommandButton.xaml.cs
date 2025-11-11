using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Next_Future_ERP.Core.Controls;

/// <summary>
/// زر أمر عصري مع تصميم AI
/// </summary>
public partial class CommandButton : Button
{
    public CommandButton()
    {
        InitializeComponent();
    }

    #region Dependency Properties

    /// <summary>
    /// أيقونة الأمر
    /// </summary>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(string), typeof(CommandButton), 
            new PropertyMetadata("🔥"));

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// نص الأمر
    /// </summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(CommandButton), 
            new PropertyMetadata("أمر"));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// اسم الأمر بالعربية
    /// </summary>
    public static readonly DependencyProperty CommandNameArProperty =
        DependencyProperty.Register(nameof(CommandNameAr), typeof(string), typeof(CommandButton), 
            new PropertyMetadata("أمر"));

    public string CommandNameAr
    {
        get => (string)GetValue(CommandNameArProperty);
        set => SetValue(CommandNameArProperty, value);
    }

    /// <summary>
    /// اسم الأمر بالإنجليزية
    /// </summary>
    public static readonly DependencyProperty CommandNameEnProperty =
        DependencyProperty.Register(nameof(CommandNameEn), typeof(string), typeof(CommandButton), 
            new PropertyMetadata("COMMAND"));

    public string CommandNameEn
    {
        get => (string)GetValue(CommandNameEnProperty);
        set => SetValue(CommandNameEnProperty, value);
    }

    /// <summary>
    /// اختصار الأمر
    /// </summary>
    public static readonly DependencyProperty CommandShortcutProperty =
        DependencyProperty.Register(nameof(CommandShortcut), typeof(string), typeof(CommandButton), 
            new PropertyMetadata(""));

    public string CommandShortcut
    {
        get => (string)GetValue(CommandShortcutProperty);
        set => SetValue(CommandShortcutProperty, value);
    }

    /// <summary>
    /// لون الأمر
    /// </summary>
    public static readonly DependencyProperty CommandColorProperty =
        DependencyProperty.Register(nameof(CommandColor), typeof(string), typeof(CommandButton), 
            new PropertyMetadata("#FF6B6B"));

    public string CommandColor
    {
        get => (string)GetValue(CommandColorProperty);
        set => SetValue(CommandColorProperty, value);
    }

    /// <summary>
    /// نص المساعدة
    /// </summary>
    public static readonly DependencyProperty CommandTooltipProperty =
        DependencyProperty.Register(nameof(CommandTooltip), typeof(string), typeof(CommandButton), 
            new PropertyMetadata(""));

    public string CommandTooltip
    {
        get => (string)GetValue(CommandTooltipProperty);
        set => SetValue(CommandTooltipProperty, value);
    }

    /// <summary>
    /// الأمر القابل للتنفيذ
    /// </summary>
    public static readonly DependencyProperty CommandExecuteProperty =
        DependencyProperty.Register(nameof(CommandExecute), typeof(ICommand), typeof(CommandButton));

    public ICommand CommandExecute
    {
        get => (ICommand)GetValue(CommandExecuteProperty);
        set => SetValue(CommandExecuteProperty, value);
    }

    /// <summary>
    /// هل الأمر مفعل؟
    /// </summary>
    public static readonly DependencyProperty IsCommandEnabledProperty =
        DependencyProperty.Register(nameof(IsCommandEnabled), typeof(bool), typeof(CommandButton), 
            new PropertyMetadata(true));

    public bool IsCommandEnabled
    {
        get => (bool)GetValue(IsCommandEnabledProperty);
        set => SetValue(IsCommandEnabledProperty, value);
    }

    /// <summary>
    /// هل الأمر مرئي؟
    /// </summary>
    public static readonly DependencyProperty IsCommandVisibleProperty =
        DependencyProperty.Register(nameof(IsCommandVisible), typeof(bool), typeof(CommandButton), 
            new PropertyMetadata(true));

    public bool IsCommandVisible
    {
        get => (bool)GetValue(IsCommandVisibleProperty);
        set => SetValue(IsCommandVisibleProperty, value);
    }

    #endregion
}

