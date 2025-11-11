using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.PosStations.ViewModels;
using System.Threading.Tasks;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;

namespace Next_Future_ERP.Features.PosStations.Views
{
    /// <summary>
    /// Interaction logic for PosStationEditWindow.xaml
    /// </summary>
    public partial class PosStationEditWindow : FluentWindow
    {
        private PosStationEditViewModel _viewModel;

        public PosStationEditWindow(PosStationEditViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            
            Loaded += async (_, __) =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("PosStationEditWindow Loaded event fired");
                    if (DataContext is PosStationEditViewModel vm)
                    {
                        System.Diagnostics.Debug.WriteLine("ViewModel found, calling InitializeAsync...");
                        await vm.InitializeAsync();
                        System.Diagnostics.Debug.WriteLine("InitializeAsync completed");
                        
                        // إضافة event handlers لتحديث زر الحفظ
                        AddEventHandlers();
                        
                        // ربط CloseRequested event مثل WarehouseEditWindow
                        vm.CloseRequested += (sender, result) =>
                        {
                            DialogResult = result;
                            Close();
                        };
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No ViewModel found in DataContext");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in Loaded event: {ex.Message}");
                    MessageBox.Show($"خطأ في تحميل النافذة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }
        
        // إضافة event handlers لتحديث زر الحفظ
        private void AddEventHandlers()
        {
            try
            {
                // البحث عن جميع TextBox و ComboBox في النافذة
                var textBoxes = FindVisualChildren<System.Windows.Controls.TextBox>(this);
                foreach (var textBox in textBoxes)
                {
                    textBox.TextChanged += (sender, args) => _viewModel?.RefreshCanSave();
                }
                
                var comboBoxes = FindVisualChildren<System.Windows.Controls.ComboBox>(this);
                foreach (var comboBox in comboBoxes)
                {
                    comboBox.SelectionChanged += (sender, args) => _viewModel?.RefreshCanSave();
                }
                
                var checkBoxes = FindVisualChildren<System.Windows.Controls.CheckBox>(this);
                foreach (var checkBox in checkBoxes)
                {
                    checkBox.Checked += (sender, args) => _viewModel?.RefreshCanSave();
                    checkBox.Unchecked += (sender, args) => _viewModel?.RefreshCanSave();
                }
                
                System.Diagnostics.Debug.WriteLine($"Added event handlers: {textBoxes.Count()} TextBoxes, {comboBoxes.Count()} ComboBoxes, {checkBoxes.Count()} CheckBoxes");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding event handlers: {ex.Message}");
            }
        }
        
        // Helper method للبحث عن جميع العناصر من نوع معين
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

        // Constructor بدون parameters للاستخدام مع Activator.CreateInstance
        public PosStationEditWindow()
        {
            InitializeComponent();
            Loaded += async (_, __) =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("PosStationEditWindow Loaded event fired (parameterless constructor)");
                    if (DataContext is PosStationEditViewModel vm)
                    {
                        System.Diagnostics.Debug.WriteLine("ViewModel found, calling InitializeAsync...");
                        await vm.InitializeAsync();
                        System.Diagnostics.Debug.WriteLine("InitializeAsync completed");
                        
                        // ربط CloseRequested event مثل WarehouseEditWindow
                        vm.CloseRequested += (sender, result) =>
                        {
                            DialogResult = result;
                            Close();
                        };
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No ViewModel found in DataContext");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in Loaded event: {ex.Message}");
                    MessageBox.Show($"خطأ في تحميل النافذة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PosStationEditViewModel vm)
            {
                vm.CloseRequested += (sender, result) =>
                {
                    DialogResult = result;
                    Close();
                };
            }
        }

    }
}
