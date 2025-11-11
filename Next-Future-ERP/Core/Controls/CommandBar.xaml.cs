using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Next_Future_ERP.Core.Controls
{
    public partial class CommandBar : UserControl
    {
        public CommandBar() => InitializeComponent();

        public ICommand? NewCommand
        {
            get => (ICommand?)GetValue(NewCommandProperty);
            set => SetValue(NewCommandProperty, value);
        }
        public static readonly DependencyProperty NewCommandProperty =
            DependencyProperty.Register(nameof(NewCommand), typeof(ICommand), typeof(CommandBar));

        public ICommand? EditCommand
        {
            get => (ICommand?)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }
        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register(nameof(EditCommand), typeof(ICommand), typeof(CommandBar));

        public ICommand? SaveCommand
        {
            get => (ICommand?)GetValue(SaveCommandProperty);
            set => SetValue(SaveCommandProperty, value);
        }
        public static readonly DependencyProperty SaveCommandProperty =
            DependencyProperty.Register(nameof(SaveCommand), typeof(ICommand), typeof(CommandBar));

        public ICommand? DeleteCommand
        {
            get => (ICommand?)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }
        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(CommandBar));

        public ICommand? RefreshCommand
        {
            get => (ICommand?)GetValue(RefreshCommandProperty);
            set => SetValue(RefreshCommandProperty, value);
        }
        public static readonly DependencyProperty RefreshCommandProperty =
            DependencyProperty.Register(nameof(RefreshCommand), typeof(ICommand), typeof(CommandBar));

        public ICommand? SearchCommand
        {
            get => (ICommand?)GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
        }
        public static readonly DependencyProperty SearchCommandProperty =
            DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(CommandBar));

        public object? SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(CommandBar));
    }
}
