using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Payments.ViewModels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Next_Future_ERP.Features.Payments.Views
{
    public partial class PaymentMethodsView : Page
    {
        public PaymentMethodsView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<PaymentMethodsViewModel>();
        }

        private static readonly Regex _digits = new(@"^\d+$");

        private void OnlyDigits_PreviewTextInput(object sender, TextCompositionEventArgs e)
            => e.Handled = !_digits.IsMatch(e.Text);

        private void OnlyDigits_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(DataFormats.Text)) { e.CancelCommand(); return; }
            var text = (string)e.DataObject.GetData(DataFormats.Text);
            if (!_digits.IsMatch(text)) e.CancelCommand();
        }
    }
}
