using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Payments.ViewModels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Next_Future_ERP.Features.Payments.Views
{
    public partial class PaymentTermsView : Page
    {
        public PaymentTermsView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<PaymentTermsViewModel>();
        }

        private static readonly Regex _digits = new(@"^\d+$");
        private static readonly Regex _decimal = new(@"^[0-9]*([.,][0-9]{0,2})?$"); // حتى خانتين

        private void OnlyDigits_PreviewTextInput(object sender, TextCompositionEventArgs e)
            => e.Handled = !_digits.IsMatch(e.Text);

        private void OnlyDigits_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(DataFormats.Text)) { e.CancelCommand(); return; }
            var text = (string)e.DataObject.GetData(DataFormats.Text);
            if (!_digits.IsMatch(text)) e.CancelCommand();
        }

        private void Decimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
            => e.Handled = !_decimal.IsMatch(e.Text);

        private void Decimal_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(DataFormats.Text)) { e.CancelCommand(); return; }
            var text = (string)e.DataObject.GetData(DataFormats.Text);
            if (!_decimal.IsMatch(text)) e.CancelCommand();
        }
    }
}
