using Next_Future_ERP.Core.Services.Contracts;
using Next_Future_ERP.Features.Auth.ViewModels;

namespace Next_Future_ERP.Features.Auth.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : IWindow
    {
        public LoginView(LoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ProgressRing_StylusInRange(object sender, System.Windows.Input.StylusEventArgs e)
        {

        }
    }
}
