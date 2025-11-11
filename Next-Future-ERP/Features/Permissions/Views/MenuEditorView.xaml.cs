using Next_Future_ERP.Features.Permissions.ViewModels;
using System.Windows.Controls;

namespace Next_Future_ERP.Features.Permissions.Views
{
    /// <summary>
    /// Interaction logic for MenuEditorView.xaml
    /// </summary>
    public partial class MenuEditorView : Page
    {
        public MenuEditorView()
        {
            InitializeComponent();
        }

        public MenuEditorView(MenuEditorViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
