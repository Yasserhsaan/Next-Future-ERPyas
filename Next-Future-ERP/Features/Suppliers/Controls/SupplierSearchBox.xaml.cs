using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Suppliers.Models;
using Next_Future_ERP.Features.Suppliers.Services;
using Next_Future_ERP.Features.Suppliers.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Next_Future_ERP.Features.Suppliers.Controls
{
    public partial class SupplierSearchBox : UserControl
    {
        private readonly ISuppliersService? _suppliersService;
        private List<Supplier> _allSuppliers = new();
        private List<Supplier> _filteredSuppliers = new();
        private int _selectedIndex = -1;
        private bool _isUpdatingText = false;

        public Supplier? SelectedSupplier { get; set; }
        public bool IsReadOnly { get; set; }

        public event EventHandler<Supplier>? SupplierSelected;

        public SupplierSearchBox()
        {
            InitializeComponent();
            
            // تحميل خدمة الموردين
            try
            {
                _suppliersService = App.ServiceProvider?.GetRequiredService<ISuppliersService>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل خدمة الموردين: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingText || _suppliersService == null) return;

            var searchText = SearchTextBox.Text?.Trim();
            
            // إظهار/إخفاء زر الإلغاء
            ClearButton.Visibility = string.IsNullOrEmpty(searchText) ? Visibility.Collapsed : Visibility.Visible;
            
            if (string.IsNullOrEmpty(searchText))
            {
                _filteredSuppliers.Clear();
                _selectedIndex = -1;
                SuggestionsPopup.IsOpen = false;
                return;
            }

            // إظهار الاقتراحات عند كتابة 3 حروف أو أكثر
            if (searchText.Length >= 3)
            {
                // البحث في الموردين المحملين
                _filteredSuppliers = _allSuppliers.Where(supplier => 
                    supplier.SupplierName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    supplier.SupplierCode.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    supplier.TaxNumber?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true
                ).Take(10).ToList();

                // إذا لم نجد نتائج في الموردين المحملين، ابحث في قاعدة البيانات
                if (_filteredSuppliers.Count == 0)
                {
                    try
                    {
                        var dbSuppliers = await _suppliersService.GetAllAsync(searchText);
                        _filteredSuppliers = dbSuppliers.Take(10).ToList();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"خطأ في البحث: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                // إظهار قائمة الاقتراحات
                if (_filteredSuppliers.Count > 0)
                {
                    SuggestionsListBox.ItemsSource = _filteredSuppliers;
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
                _filteredSuppliers.Clear();
                _selectedIndex = -1;
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!SuggestionsPopup.IsOpen || _filteredSuppliers.Count == 0) return;

            switch (e.Key)
            {
                case Key.Enter:
                    if (_selectedIndex >= 0 && _selectedIndex < _filteredSuppliers.Count)
                    {
                        SelectSupplier(_filteredSuppliers[_selectedIndex]);
                    }
                    break;
                    
                case Key.Down:
                    _selectedIndex = Math.Min(_selectedIndex + 1, _filteredSuppliers.Count - 1);
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
                    _filteredSuppliers.Clear();
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
            if (SuggestionsListBox.SelectedItem is Supplier selectedSupplier)
            {
                SelectSupplier(selectedSupplier);
            }
        }

        private void SelectSupplier(Supplier supplier)
        {
            SelectedSupplier = supplier;
            SearchTextBox.Text = supplier.SupplierName;
            SupplierSelected?.Invoke(this, supplier);
            SuggestionsPopup.IsOpen = false;
            _filteredSuppliers.Clear();
            _selectedIndex = -1;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (_suppliersService == null)
            {
                MessageBox.Show("خدمة الموردين غير متاحة. يرجى التأكد من تسجيل الخدمات في DI Container.", 
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var searchWindow = App.ServiceProvider.GetRequiredService<SupplierLookupWindow>();
            searchWindow.Owner = Application.Current.MainWindow;
            
            if (searchWindow.ShowDialog() == true && searchWindow.SelectedSupplier is not null)
            {
                SelectedSupplier = searchWindow.SelectedSupplier;
                SearchTextBox.Text = SelectedSupplier.SupplierName;
                SupplierSelected?.Invoke(this, SelectedSupplier);
            }
        }

        public async void LoadSuppliers()
        {
            if (_suppliersService == null) return;

            try
            {
                _allSuppliers = (await _suppliersService.GetAllAsync()).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الموردين: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetSupplier(Supplier? supplier)
        {
            SelectedSupplier = supplier;
            SearchTextBox.Text = supplier?.SupplierName ?? string.Empty;
        }

        public void Clear()
        {
            SelectedSupplier = null;
            SearchTextBox.Text = string.Empty;
            _filteredSuppliers.Clear();
            _selectedIndex = -1;
            SuggestionsPopup.IsOpen = false;
            ClearButton.Visibility = Visibility.Collapsed;
        }
    }
}
