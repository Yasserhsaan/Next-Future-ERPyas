using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Inventory.ViewModels;
using Next_Future_ERP.Features.Inventory.Models;
using System.Windows;

namespace Next_Future_ERP.Features.Inventory.Views
{
    public partial class InventoryOpeningBrowseView : Window
    {
        public InventoryOpeningHeader? SelectedHeader => (DataContext as InventoryOpeningBrowseViewModel)?.SelectedHeader;

        public InventoryOpeningBrowseView(InventoryOpeningBrowseViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            Loaded += async (_, __) => await vm.LoadAsync();

            // تعيين AcceptButton: عند الضغط على Enter يتم اختيار المستند إن كان مسودة
            this.AddHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, new RoutedEventHandler((s, e) =>
            {
                if (e.OriginalSource is System.Windows.Controls.Button btn && (string?)btn.Content == "اختيار")
                {
                    var sel = (DataContext as InventoryOpeningBrowseViewModel)?.SelectedHeader;
                    if (sel != null && sel.Status == InventoryOpeningStatus.Draft)
                    {
                        this.DialogResult = true;
                        this.Close();
                    }
                    else if (sel != null && sel.Status == InventoryOpeningStatus.Approved)
                    {
                        MessageBox.Show("لا يمكن تعديل المستندات المعتمدة", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }));
        }
    }
}


