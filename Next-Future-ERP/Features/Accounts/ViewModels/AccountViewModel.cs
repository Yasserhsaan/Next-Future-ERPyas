using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Models;
using Next_Future_ERP.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class AccountsViewModel : ObservableObject
    {
        private readonly AccountsService _service = new();

        [ObservableProperty]
        private ObservableCollection<Account> rootAccounts = new();

        [ObservableProperty]
        private ObservableCollection<Account> accounts = new();

        [ObservableProperty]
        private Account? selectedAccount;

        [ObservableProperty]
        private string? searchQuery;

        private Account? _selectedTabAccount;
        public Account? SelectedTabAccount
        {
            get => _selectedTabAccount;
            set
            {
                SetProperty(ref _selectedTabAccount, value);
                SelectedAccount = value; // لتفعيل الأزرار عند اختيار تبويب
            }
        }

        private ICommand _addSubAccountCommand;
        public ICommand AddSubAccountCommand => _addSubAccountCommand ??= new RelayCommand(AddSubAccount);

        public AccountsViewModel()
        {
            // التحميل عند الاستخدام فقط
        }

        public Task ReloadTreeAsync() => LoadTreeAsync();

        [RelayCommand]
        private async Task LoadTreeAsync()
        {
            try
            {
                RootAccounts.Clear();
                var items = await _service.GetAccountsTreeAsync();
                foreach (var item in items)
                    RootAccounts.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ حدث خطأ أثناء تحميل الشجرة:\n{ex.Message}");
            }
        }

        [RelayCommand]
        private async Task LoadAccountsAsync()
        {
            try
            {
                Accounts.Clear();
                var items = await _service.GetAllAsync();
                foreach (var item in items)
                    Accounts.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ حدث خطأ أثناء تحميل الحسابات:\n{ex.Message}");
            }
        }

        [RelayCommand]
        private async Task AddAsync()
        {
            try
            {
                if (SelectedAccount is not null)
                {
                    await _service.AddAsync(SelectedAccount);
                    await LoadAccountsAsync();
                    SelectedAccount = new Account();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء الإضافة:\n{ex.Message}");
            }
        }

        [RelayCommand]
        private async Task UpdateAsync()
        {
            try
            {
                if (SelectedAccount is not null)
                {
                    await _service.UpdateAsync(SelectedAccount);
                    await LoadAccountsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء التعديل:\n{ex.Message}");
            }
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            try
            {
                if (SelectedAccount is not null)
                {
                    await _service.DeleteAsync(SelectedAccount.AccountId);
                    await LoadAccountsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء الحذف:\n{ex.Message}");
            }
        }
        [RelayCommand]
        private async Task EditAccountAsync()
        {
            try
            {
                if (SelectedAccount == null)
                    return;

                var dialog = new AccountDialog(null, SelectedAccount); // null لأننا نعدل مباشرة
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    await LoadAccountsAsync(); // إعادة تحميل القائمة
                    await LoadTreeAsync();     // إعادة تحميل الشجرة
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ حدث خطأ أثناء تعديل الحساب:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddSubAccount()
        {
            try
            {
                //if (SelectedAccount == null)
                //    return;

                var dialog = new AccountDialog(SelectedAccount);

                dialog.AccountSaved += async (_, __) =>
                {
                    try
                    {
                        await ReloadTreeAsync();
                        // اختيار الحساب الجديد تلقائيًا ممكن لاحقًا
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"❌ خطأ أثناء تحديث الشجرة:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ حدث خطأ أثناء إضافة الحساب الفرعي:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        [RelayCommand]
        private async Task OpenAddDialogAsync(Account? parent = null)
        {
            // سيتم التفعيل لاحقاً
        }

        //[RelayCommand]
        //private async Task OpenEditDialogAsync(Account account)
        //{
        //    // سيتم التفعيل لاحقاً
        //}

        [RelayCommand]
        private async Task DeleteAccountAsync(Account account)
        {
            try
            {
                if (SelectedAccount is not null)
                {
                    await _service.DeleteAsync(SelectedAccount.AccountId);
                    await LoadAccountsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ أثناء الحذف:\n{ex.Message}");
            }
        }

        partial void OnSearchQueryChanged(string? value)
        {
            _ = FilterTreeAsync();
        }

        private async Task FilterTreeAsync()
        {
            try
            {
                var allAccounts = await _service.GetAccountsTreeAsync();

                if (string.IsNullOrWhiteSpace(SearchQuery))
                {
                    RootAccounts = new ObservableCollection<Account>(allAccounts);
                }
                else
                {
                    var filtered = allAccounts
                        .Where(a => ContainsRecursive(a, SearchQuery!))
                        .ToList();

                    RootAccounts = new ObservableCollection<Account>(filtered);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ حدث خطأ أثناء البحث:\n{ex.Message}");
            }
        }

        private bool ContainsRecursive(Account acc, string query)
        {
            if (acc.AccountNameAr?.Contains(query, StringComparison.OrdinalIgnoreCase) == true)
                return true;

            return acc.Children?.Any(child => ContainsRecursive(child, query)) == true;
        }
    }
}
