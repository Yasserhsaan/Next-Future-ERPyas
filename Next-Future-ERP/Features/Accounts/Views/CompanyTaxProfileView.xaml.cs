using System.Windows.Controls;
using Next_Future_ERP.Features.Accounts.ViewModels;

namespace Next_Future_ERP.Features.Accounts.Views
{
    public partial class CompanyTaxProfileView : Page
    {
        public CompanyTaxProfileView()
        {
            InitializeComponent();
            DataContext = new CompanyTaxProfileViewModel();
        }

        private async void CompanyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is CompanyTaxProfileViewModel vm)
            {
                var cb = (ComboBox)sender;
                var companyId = cb.SelectedValue as int?;
                await vm.CompanyChangedAsync(companyId);
            }
        }

        private async void SearchCompanyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is CompanyTaxProfileViewModel vm)
            {
                var cb = (ComboBox)sender;
                var companyId = cb.SelectedValue as int?;
                await vm.SearchCompanyChangedAsync(companyId);
            }
        }
    }
}
