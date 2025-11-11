using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Next_Future_ERP.Features.Permissions.ViewModels;
using Next_Future_ERP;
using Microsoft.Extensions.DependencyInjection;

namespace Next_Future_ERP.Features.Permissions.Views
{
    /// <summary>
    /// Interaction logic for PermissionsMainView.xaml
    /// </summary>
    public partial class PermissionsMainView : Page
    {
        public PermissionsMainView()
        {
            InitializeComponent();
            
            // Explicitly set the DataContext using DI
            DataContext = App.ServiceProvider.GetRequiredService<PermissionsMainViewModel>();
            
            Loaded += PermissionsMainView_Loaded;
            
            // Handle popup positioning
            AddRolePopup.Opened += Popup_Opened;
            EditRolePopup.Opened += Popup_Opened;
        }

        private async void PermissionsMainView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PermissionsMainViewModel viewModel)
            {
                await viewModel.LoadDataAsync();
            }

            // Subscribe to window events for popup repositioning
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.LocationChanged += Window_LocationChanged;
                window.SizeChanged += Window_SizeChanged;
            }
        }

        private void Popup_Opened(object sender, EventArgs e)
        {
            if (sender is Popup popup)
            {
                CenterPopupOnScreen(popup);
            }
        }

        private void Window_LocationChanged(object? sender, EventArgs e)
        {
            // Recenter popups when window moves
            if (AddRolePopup.IsOpen)
                CenterPopupOnScreen(AddRolePopup);
            if (EditRolePopup.IsOpen)
                CenterPopupOnScreen(EditRolePopup);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Recenter popups when window resizes
            if (AddRolePopup.IsOpen)
                CenterPopupOnScreen(AddRolePopup);
            if (EditRolePopup.IsOpen)
                CenterPopupOnScreen(EditRolePopup);
        }

        private void CenterPopupOnScreen(Popup popup)
        {
            if (popup?.Child == null) return;

            try
            {
                // Force measure to get actual size
                popup.Child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var popupSize = popup.Child.DesiredSize;

                // Get screen work area (excluding taskbar)
                var workArea = SystemParameters.WorkArea;
                
                // Calculate absolute center position on screen
                var centerX = workArea.Left + (workArea.Width - popupSize.Width) / 2;
                var centerY = workArea.Top + (workArea.Height - popupSize.Height) / 2;
                
                // Set popup position as absolute coordinates
                popup.HorizontalOffset = centerX;
                popup.VerticalOffset = centerY;
            }
            catch
            {
                // Fallback to simple centering if calculation fails
                popup.HorizontalOffset = (SystemParameters.PrimaryScreenWidth - 420) / 2;
                popup.VerticalOffset = (SystemParameters.PrimaryScreenHeight - 300) / 2;
            }
        }
    }
}
