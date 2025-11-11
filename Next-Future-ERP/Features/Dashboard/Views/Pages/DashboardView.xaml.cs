using Next_Future_ERP.Features.Dashboard.ViewModels;
using System.Windows.Controls;

namespace Next_Future_ERP.Features.Dashboard.Views.Pages
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : Page
    {
        public DashboardView()
        {
            InitializeComponent();
            
            // تعيين DataContext مباشرة
            DataContext = new DashboardViewModel();
        }
    }
}
