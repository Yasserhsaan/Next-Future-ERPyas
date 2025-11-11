using Next_Future_ERP.Features.InitialSystem.ViewModels;
using System.Windows.Controls;

namespace Next_Future_ERP.Features.InitialSystem.Views.Pages
{
    /// <summary>
    /// Interaction logic for ReviewSettingsPage.xaml
    /// </summary>
    public partial class ReviewSettingsPage : UserControl
    {
        public ReviewSettingsPage(ReviewSettingsViewModel viewModel)
        {
            InitializeComponent();
             DataContext = viewModel;
        }
    }
} 