using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Next_Future_ERP.Models;

namespace Next_Future_ERP.Features.Accounts.Controls
{
    public partial class AccountSearchBox : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<Account>? AccountSelected;

        // Dependency Properties
        public static readonly DependencyProperty SelectedAccountProperty =
            DependencyProperty.Register(nameof(SelectedAccount), typeof(Account), typeof(AccountSearchBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty AccountsSourceProperty =
            DependencyProperty.Register(nameof(AccountsSource), typeof(ObservableCollection<Account>), typeof(AccountSearchBox),
                new PropertyMetadata(null));

        public Account? SelectedAccount
        {
            get => (Account?)GetValue(SelectedAccountProperty);
            set => SetValue(SelectedAccountProperty, value);
        }

        public ObservableCollection<Account>? AccountsSource
        {
            get => (ObservableCollection<Account>?)GetValue(AccountsSourceProperty);
            set => SetValue(AccountsSourceProperty, value);
        }

        // Search properties
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    PerformSearch();
                }
            }
        }

        private ObservableCollection<Account> _searchResults = new();
        public ObservableCollection<Account> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        public AccountSearchBox()
        {
            InitializeComponent();
        }

        private void PerformSearch()
        {
            SearchResults.Clear();

            if (string.IsNullOrWhiteSpace(SearchText) || AccountsSource == null)
            {
                ResultsPopup.IsOpen = false;
                return;
            }

            var searchTerm = SearchText.Trim().ToLower();
            var results = AccountsSource.Where(account =>
                account.AccountCode.ToLower().Contains(searchTerm) ||
                account.AccountNameAr.ToLower().Contains(searchTerm) ||
                (!string.IsNullOrEmpty(account.AccountNameEn) && account.AccountNameEn.ToLower().Contains(searchTerm)) ||
                GetCategoryDisplayName(account.AccountCategoryKey).ToLower().Contains(searchTerm)
            ).Take(10).ToList();

            foreach (var result in results)
            {
                SearchResults.Add(result);
            }

            ResultsPopup.IsOpen = SearchResults.Any();
        }

        private string GetCategoryDisplayName(string? categoryKey)
        {
            return categoryKey?.ToLower() switch
            {
                "inventory" => "مخازن",
                "bank" => "بنوك",
                "cash" => "صناديق",
                "other_receivable" => "عملاء",
                "accounts_payable" => "موردين",
                "fixed_assets" => "أصول ثابتة",
                "current_assets" => "أصول متداولة",
                "revenue" => "إيرادات",
                "expenses" => "مصروفات",
                "equity" => "حقوق ملكية",
                _ => categoryKey ?? "غير محدد"
            };
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // يتم التعامل معها من خلال Binding
        }

        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                // فتح البحث المتقدم عند الضغط على F1
                OpenAdvancedSearch();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                // معالجة Enter للبحث أو الاختيار
                HandleEnterKey();
                e.Handled = true;
            }
            else if (e.Key == Key.Down && SearchResults.Any())
            {
                // التنقل لأسفل في النتائج
                ResultsPopup.IsOpen = true;
                if (ResultsItemsControl.Items.Count > 0)
                {
                    var firstItem = ResultsItemsControl.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement;
                    firstItem?.Focus();
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                // إغلاق النتائج أو مسح النص
                if (ResultsPopup.IsOpen)
                {
                    ResultsPopup.IsOpen = false;
                }
                else
                {
                    SearchText = string.Empty;
                    SelectedAccount = null;
                }
                e.Handled = true;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                PerformSearch();
            }
        }

        private void ResultItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is Account account)
            {
                SelectAccount(account);
            }
        }

        private void SelectAccount(Account account)
        {
            SelectedAccount = account;
            SearchText = $"{account.AccountCode} - {account.AccountNameAr}";
            ResultsPopup.IsOpen = false;
            
            // إثارة الحدث
            AccountSelected?.Invoke(this, account);
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // تحديث المظهر عند التركيز - سيتم تطبيقه تلقائياً من خلال XAML Triggers
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // إخفاء النتائج عند فقدان التركيز إذا لم يتم اختيار أي شيء
            if (!ResultsPopup.IsMouseOver)
            {
                Task.Delay(150).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (!SearchTextBox.IsFocused && !ResultsPopup.IsMouseOver)
                        {
                            ResultsPopup.IsOpen = false;
                        }
                    });
                });
            }
        }

        private void OpenAdvancedSearch()
        {
            try
            {
                // إذا كان هناك نص في مربع البحث، نبحث به
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    PerformSearch();
                    ResultsPopup.IsOpen = true;
                }
                else
                {
                    // إظهار جميع الحسابات المتاحة للاختيار
                    if (AccountsSource?.Any() == true)
                    {
                        SearchResults.Clear();
                        foreach (var account in AccountsSource.Take(50)) // عرض أول 50 حساب
                        {
                            SearchResults.Add(account);
                        }
                        ResultsPopup.IsOpen = true;
                    }
                }
                
                // التركيز على أول عنصر إذا وجد
                if (SearchResults.Any())
                {
                    Task.Delay(100).ContinueWith(_ =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            if (ResultsItemsControl.Items.Count > 0)
                            {
                                var firstItem = ResultsItemsControl.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement;
                                firstItem?.Focus();
                            }
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في البحث المتقدم: {ex.Message}");
            }
        }

        private void HandleEnterKey()
        {
            try
            {
                if (ResultsPopup.IsOpen && SearchResults.Any())
                {
                    // إذا كانت النتائج مفتوحة، اختر النتيجة الأولى
                    SelectAccount(SearchResults.First());
                }
                else if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    // إذا كان هناك نص، ابحث أولاً
                    PerformSearch();
                    
                    // إذا كانت هناك نتيجة واحدة فقط، اخترها تلقائياً
                    if (SearchResults.Count == 1)
                    {
                        SelectAccount(SearchResults.First());
                    }
                    else if (SearchResults.Any())
                    {
                        // إذا كان هناك عدة نتائج، اظهرها
                        ResultsPopup.IsOpen = true;
                    }
                }
                else
                {
                    // إذا لم يكن هناك نص، فتح البحث المتقدم
                    OpenAdvancedSearch();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في معالجة Enter: {ex.Message}");
            }
        }
    }
}

