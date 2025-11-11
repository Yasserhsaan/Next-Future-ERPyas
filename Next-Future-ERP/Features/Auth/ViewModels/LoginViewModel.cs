using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Data.Models;
using Next_Future_ERP.Features.Auth.Services;
using Next_Future_ERP.Features.Dashboard.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui;

namespace Next_Future_ERP.Features.Auth.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly ISessionService _sessionService;
        [ObservableProperty] private string username = "user";   // القيمة الافتراضية
        [ObservableProperty] private string password = "123";    // فقط للتجربة
        [ObservableProperty]
        private string applicationTitle = "تسجيل الدخول";

        //[ObservableProperty]
        //private string username;

        //[ObservableProperty]
        //private string password;

        [ObservableProperty]
        private bool isLoading;

        public LoginViewModel(IAuthService authService, ISessionService sessionService)
        {
            _authService = authService;
            _sessionService = sessionService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;

                // Validate input
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    MessageBox.Show("يرجى إدخال اسم المستخدم وكلمة المرور", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Attempt authentication
                var authResult = await _authService.LoginAsync(Username, Password);

                if (authResult.IsSuccess)
                {
                    // Session is already initialized by AuthService
                    var currentUser = _sessionService.CurrentUser;
                    
                    // Show success message with user info
                    var welcomeMessage = $"{authResult.Message}\n" ;
                    
                    MessageBox.Show(welcomeMessage, "نجح تسجيل الدخول", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Navigate to main window
                    var activeWindow = Application.Current.Windows
                        .OfType<Window>()
                        .FirstOrDefault(w => w.IsActive);

                    // Get MainWindowModel from DI (already initialized in App.xaml.cs)
                    var mainWindowModel = App.MainViewModel ?? throw new InvalidOperationException("MainViewModel not initialized");
                    Application.Current.MainWindow = new MainWindow(mainWindowModel);
                    Application.Current.MainWindow.Show();
                    
                    activeWindow?.Close();
                }
                else
                {
                    // Show error message
                    MessageBox.Show(authResult.Message, "خطأ في تسجيل الدخول", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ غير متوقع: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
