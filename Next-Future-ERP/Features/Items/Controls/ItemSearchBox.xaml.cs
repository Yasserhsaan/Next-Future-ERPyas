using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items.Services;
using Next_Future_ERP.Features.Items.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Next_Future_ERP.Features.Items.Controls
{
    public partial class ItemSearchBox : UserControl
    {
        private readonly IItemsService? _itemsService;
        private List<Item> _allItems = new();
        private List<Item> _filteredItems = new();
        private int _selectedIndex = -1;
        private bool _isUpdatingText = false;

        public Item? SelectedItem { get; set; }
        public bool IsReadOnly { get; set; }

        public event EventHandler<Item>? ItemSelected;

        public ItemSearchBox()
        {
            InitializeComponent();
            
            // تحميل خدمة الأصناف
            try
            {
                _itemsService = App.ServiceProvider?.GetRequiredService<IItemsService>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل خدمة الأصناف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingText || _itemsService == null) return;

            var searchText = SearchTextBox.Text?.Trim();
            
            // إظهار/إخفاء زر الإلغاء
            ClearButton.Visibility = string.IsNullOrEmpty(searchText) ? Visibility.Collapsed : Visibility.Visible;
            
            if (string.IsNullOrEmpty(searchText))
            {
                _filteredItems.Clear();
                _selectedIndex = -1;
                SuggestionsPopup.IsOpen = false;
                return;
            }

            // إظهار الاقتراحات عند كتابة 3 حروف أو أكثر
            if (searchText.Length >= 3)
            {
                // البحث في الأصناف المحملة
                _filteredItems = _allItems.Where(item => 
                    item.ItemName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    item.ItemCode.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    item.ItemBarcode?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true
                ).Take(10).ToList();

                // إذا لم نجد نتائج في الأصناف المحملة، ابحث في قاعدة البيانات
                if (_filteredItems.Count == 0)
                {
                    try
                    {
                        var dbItems = await _itemsService.GetAllAsync(searchText);
                        _filteredItems = dbItems.Take(10).ToList();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"خطأ في البحث: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                // إظهار قائمة الاقتراحات
                if (_filteredItems.Count > 0)
                {
                    SuggestionsListBox.ItemsSource = _filteredItems;
                    SuggestionsPopup.IsOpen = true;
                    _selectedIndex = 0;
                }
                else
                {
                    SuggestionsPopup.IsOpen = false;
                }
            }
            else
            {
                // إخفاء الاقتراحات عند كتابة أقل من 3 حروف
                SuggestionsPopup.IsOpen = false;
                _filteredItems.Clear();
                _selectedIndex = -1;
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!SuggestionsPopup.IsOpen || _filteredItems.Count == 0) return;

            switch (e.Key)
            {
                case Key.Enter:
                    if (_selectedIndex >= 0 && _selectedIndex < _filteredItems.Count)
                    {
                        SelectItem(_filteredItems[_selectedIndex]);
                    }
                    break;
                    
                case Key.Down:
                    _selectedIndex = Math.Min(_selectedIndex + 1, _filteredItems.Count - 1);
                    SuggestionsListBox.SelectedIndex = _selectedIndex;
                    SuggestionsListBox.ScrollIntoView(SuggestionsListBox.Items[_selectedIndex]);
                    e.Handled = true;
                    break;
                    
                case Key.Up:
                    _selectedIndex = Math.Max(_selectedIndex - 1, 0);
                    SuggestionsListBox.SelectedIndex = _selectedIndex;
                    SuggestionsListBox.ScrollIntoView(SuggestionsListBox.Items[_selectedIndex]);
                    e.Handled = true;
                    break;
                    
                case Key.Escape:
                    SuggestionsPopup.IsOpen = false;
                    _filteredItems.Clear();
                    _selectedIndex = -1;
                    break;
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // إظهار الاقتراحات إذا كان هناك نص مكتوب
            if (!string.IsNullOrEmpty(SearchTextBox.Text?.Trim()) && SearchTextBox.Text.Trim().Length >= 3)
            {
                SuggestionsPopup.IsOpen = true;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // تأخير إخفاء الاقتراحات للسماح بالنقر على العناصر
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!SuggestionsListBox.IsMouseOver && !SuggestionsPopup.IsMouseOver)
                {
                    SuggestionsPopup.IsOpen = false;
                }
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        private void SuggestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuggestionsListBox.SelectedIndex >= 0)
            {
                _selectedIndex = SuggestionsListBox.SelectedIndex;
            }
        }

        private void SuggestionsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SuggestionsListBox.SelectedItem is Item selectedItem)
            {
                SelectItem(selectedItem);
            }
        }

        private void SelectItem(Item item)
        {
            SelectedItem = item;
            SearchTextBox.Text = item.ItemName;
            ItemSelected?.Invoke(this, item);
            SuggestionsPopup.IsOpen = false;
            _filteredItems.Clear();
            _selectedIndex = -1;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (_itemsService == null)
            {
                MessageBox.Show("خدمة الأصناف غير متاحة. يرجى التأكد من تسجيل الخدمات في DI Container.", 
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var searchWindow = new ItemSearchWindow(_itemsService);
            searchWindow.Owner = Application.Current.MainWindow;
            
            if (searchWindow.ShowDialog() == true && searchWindow.SelectedItem is not null)
            {
                SelectedItem = searchWindow.SelectedItem;
                SearchTextBox.Text = SelectedItem.ItemName;
                ItemSelected?.Invoke(this, SelectedItem);
            }
        }

        public async void LoadItems()
        {
            if (_itemsService == null) return;

            try
            {
                _allItems = (await _itemsService.GetAllAsync()).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الأصناف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetItem(Item? item)
        {
            SelectedItem = item;
            SearchTextBox.Text = item?.ItemName ?? string.Empty;
        }

        public void Clear()
        {
            SelectedItem = null;
            SearchTextBox.Text = string.Empty;
            _filteredItems.Clear();
            _selectedIndex = -1;
            SuggestionsPopup.IsOpen = false;
            ClearButton.Visibility = Visibility.Collapsed;
        }
    }
}
