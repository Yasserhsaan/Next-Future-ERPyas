using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Next_Future_ERP.Features.Controls
{
    /// <summary>
    /// Interaction logic for SearchBar.xaml
    /// </summary>
    public partial class SearchBar : UserControl
    {
        public SearchBar()
        {
            InitializeComponent();
        }

        // ========== Dependency Properties ==========
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(SearchBar),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(SearchBar),
                new PropertyMetadata("Search"));

        public ICommand? SearchCommand
        {
            get => (ICommand?)GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
        }
        public static readonly DependencyProperty SearchCommandProperty =
            DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(SearchBar), new PropertyMetadata(null));

        public ICommand? RefreshCommand
        {
            get => (ICommand?)GetValue(RefreshCommandProperty);
            set => SetValue(RefreshCommandProperty, value);
        }
        public static readonly DependencyProperty RefreshCommandProperty =
            DependencyProperty.Register(nameof(RefreshCommand), typeof(ICommand), typeof(SearchBar), new PropertyMetadata(null));

        public ICommand? FilterCommand
        {
            get => (ICommand?)GetValue(FilterCommandProperty);
            set => SetValue(FilterCommandProperty, value);
        }
        public static readonly DependencyProperty FilterCommandProperty =
            DependencyProperty.Register(nameof(FilterCommand), typeof(ICommand), typeof(SearchBar), new PropertyMetadata(null));

        // ========== Routed Events ==========
        public static readonly RoutedEvent SearchClickedEvent =
            EventManager.RegisterRoutedEvent(nameof(SearchClicked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SearchBar));
        public event RoutedEventHandler SearchClicked
        {
            add => AddHandler(SearchClickedEvent, value);
            remove => RemoveHandler(SearchClickedEvent, value);
        }

        public static readonly RoutedEvent RefreshClickedEvent =
            EventManager.RegisterRoutedEvent(nameof(RefreshClicked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SearchBar));
        public event RoutedEventHandler RefreshClicked
        {
            add => AddHandler(RefreshClickedEvent, value);
            remove => RemoveHandler(RefreshClickedEvent, value);
        }

        public static readonly RoutedEvent FilterClickedEvent =
            EventManager.RegisterRoutedEvent(nameof(FilterClicked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SearchBar));
        public event RoutedEventHandler FilterClicked
        {
            add => AddHandler(FilterClickedEvent, value);
            remove => RemoveHandler(FilterClickedEvent, value);
        }

        // ========== Handlers ==========
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SearchClickedEvent));
            if (SearchCommand?.CanExecute(Text) == true) SearchCommand.Execute(Text);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(RefreshClickedEvent));
            if (RefreshCommand?.CanExecute(null) == true) RefreshCommand.Execute(null);
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(FilterClickedEvent));
            if (FilterCommand?.CanExecute(null) == true) FilterCommand.Execute(null);
        }

        private void PART_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search_Click(sender, new RoutedEventArgs());
                e.Handled = true;
            }
        }
    }
}
