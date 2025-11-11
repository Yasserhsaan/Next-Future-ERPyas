using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.Payments.ViewModels;
using System.Windows.Controls;

namespace Next_Future_ERP.Features.Payments.Views
{
    public partial class PaymentTypesView : Page
    {
        public PaymentTypesView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<PaymentTypesViewModel>();
        }

        // قصّ أي لصق يزيد عن حرفين
        private void TxtCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb && tb.Text.Length > 2)
            {
                var caret = tb.CaretIndex;
                tb.Text = tb.Text.Substring(0, 2).ToUpperInvariant();
                tb.CaretIndex = caret > 2 ? 2 : caret;
            }
        }
    }
}
