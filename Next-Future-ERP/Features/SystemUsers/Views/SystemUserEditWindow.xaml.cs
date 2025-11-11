using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.SystemUsers.ViewModels;
using System.Threading.Tasks;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;

namespace Next_Future_ERP.Features.SystemUsers.Views
{
    /// <summary>
    /// Interaction logic for SystemUserEditWindow.xaml
    /// </summary>
    public partial class SystemUserEditWindow : Window
    {
        private SystemUserEditViewModel _viewModel;

        public SystemUserEditWindow(SystemUserEditViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            
            Loaded += async (_, __) =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("SystemUserEditWindow Loaded event fired");
                    if (DataContext is SystemUserEditViewModel vm)
                    {
                        System.Diagnostics.Debug.WriteLine("ViewModel found, calling InitializeAsync...");
                        await vm.InitializeAsync();
                        System.Diagnostics.Debug.WriteLine("InitializeAsync completed");
                        
                        // إضافة event handlers لتحديث زر الحفظ
                        AddEventHandlers();
                        
                        // ربط CloseRequested event مثل WarehouseEditWindow
                        vm.CloseRequested += (sender, result) =>
                        {
                            // تأكد من أن النافذة تم إنشاؤها وعرضها كـ dialog قبل تعيين DialogResult
                            if (IsLoaded && IsVisible)
                            {
                                DialogResult = result;
                            }
                            Close();
                        };
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No ViewModel found in DataContext");
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in Loaded event: {ex.Message}\n{ex.StackTrace}");
                    MessageBox.Show($"خطأ في تحميل النافذة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }

        private void AddEventHandlers()
        {
            System.Diagnostics.Debug.WriteLine("Adding event handlers...");
            
            // البحث عن جميع TextBoxes وإضافة event handlers
            var textBoxes = FindVisualChildren<System.Windows.Controls.TextBox>(this).ToList();
            System.Diagnostics.Debug.WriteLine($"Found {textBoxes.Count} TextBoxes");
            
            foreach (var textBox in textBoxes)
            {
                textBox.TextChanged += (s, e) => _viewModel.SaveCommand.NotifyCanExecuteChanged();
            }

            // البحث عن جميع ComboBoxes وإضافة event handlers
            var comboBoxes = FindVisualChildren<System.Windows.Controls.ComboBox>(this).ToList();
            System.Diagnostics.Debug.WriteLine($"Found {comboBoxes.Count} ComboBoxes");
            
            foreach (var comboBox in comboBoxes)
            {
                comboBox.SelectionChanged += (s, e) => _viewModel.SaveCommand.NotifyCanExecuteChanged();
            }

            // البحث عن جميع DatePickers وإضافة event handlers
            var datePickers = FindVisualChildren<System.Windows.Controls.DatePicker>(this).ToList();
            System.Diagnostics.Debug.WriteLine($"Found {datePickers.Count} DatePickers");
            
            foreach (var datePicker in datePickers)
            {
                datePicker.SelectedDateChanged += (s, e) => _viewModel.SaveCommand.NotifyCanExecuteChanged();
            }

            // البحث عن جميع CheckBoxes وإضافة event handlers
            var checkBoxes = FindVisualChildren<System.Windows.Controls.CheckBox>(this).ToList();
            System.Diagnostics.Debug.WriteLine($"Found {checkBoxes.Count} CheckBoxes");
            
            foreach (var checkBox in checkBoxes)
            {
                checkBox.Checked += (s, e) => _viewModel.SaveCommand.NotifyCanExecuteChanged();
                checkBox.Unchecked += (s, e) => _viewModel.SaveCommand.NotifyCanExecuteChanged();
            }

            System.Diagnostics.Debug.WriteLine($"Added event handlers: {textBoxes.Count} TextBoxes, {comboBoxes.Count} ComboBoxes, {datePickers.Count} DatePickers, {checkBoxes.Count} CheckBoxes");
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.PasswordBox passwordBox && _viewModel != null)
            {
                _viewModel.Password = passwordBox.Password;
                _viewModel.SaveCommand.NotifyCanExecuteChanged();
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.PasswordBox passwordBox && _viewModel != null)
            {
                _viewModel.ConfirmPassword = passwordBox.Password;
                _viewModel.SaveCommand.NotifyCanExecuteChanged();
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SystemUserEditViewModel vm)
            {
                vm.CloseRequested += (sender, result) =>
                {
                    // تأكد من أن النافذة تم إنشاؤها وعرضها كـ dialog قبل تعيين DialogResult
                    if (IsLoaded && IsVisible)
                    {
                        DialogResult = result;
                    }
                    Close();
                };
            }
        }

    }
}
