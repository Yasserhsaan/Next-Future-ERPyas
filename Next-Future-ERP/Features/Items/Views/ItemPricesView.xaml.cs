using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Items.ViewModels;
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

namespace Next_Future_ERP.Features.Items.Views
{
    /// <summary>
    /// Interaction logic for ItemPricesView.xaml
    /// </summary>
    public partial class ItemPricesView : UserControl
    {
        public ItemPricesView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<ItemPricesViewModel>();
        }
    }
}
