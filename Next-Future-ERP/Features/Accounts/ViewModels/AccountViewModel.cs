using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Data;
using Microsoft.Win32;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.Auth.Services;
using Next_Future_ERP.Models;
using Next_Future_ERP.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class AccountsViewModel : ObservableObject
    {
        private readonly AccountsImportService _importService = new(App.ServiceProvider.GetService<AppDbContext>()!);
        private readonly AccountsService _service = new();
        private ISessionService? _session;

        // ⬅️ إضافة

        // لضمان استدعاء تهيئة الجذور مرة واحدة فقط
        private bool _rootsEnsured;

        [ObservableProperty] private int companyId;
        [ObservableProperty] private int? branchId;

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
                SelectedAccount = value;
            }
        }

        private ICommand _addSubAccountCommand;
        public ICommand AddSubAccountCommand => _addSubAccountCommand ??= new RelayCommand(AddSubAccount);

        public AccountsViewModel() { }

        // (اختياري) منشئ يعتمد الـ Session لو أردت استعماله من code-behind بدلاً من InitializeFromSession
        public AccountsViewModel(ISessionService session) : this()
        {
            InitializeFromSession(session);
        }

        // ✅ مهيّئ يُستدعى بعد الإنشاء عندما يُوفّر لنا Session
        public void InitializeFromSession(ISessionService session)
        {
            _session = session;
            var cu = _session.CurrentUser;
            CompanyId = cu?.CompanyId ?? 1;
            BranchId = cu?.BranchId;
        }
        public Task ReloadTreeAsync() => LoadTreeAsync();

        /// <summary>
        /// يضمن تهيئة جذور الدليل (1/2/3/4/5) من الفيو عند فراغ الدليل أو نقص الجذور.
        /// يستدعي: sp_EnsureMainAccountsSeeded(@CompanyId,@BranchId) مرة واحدة فقط.
        /// </summary>
        private async Task EnsureRootsOnceAsync(CancellationToken ct = default)
        {
            if (_rootsEnsured) return;

            try
            {
                // CompanyId/BranchId يجب أن يكونا مضبوطين قبل الاستدعاء
                await _service.EnsureMainAccountsSeededAsync(companyId, branchId, ct);
                _rootsEnsured = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"⚠️ تعذر تهيئة جذور الدليل تلقائيًا:\n{ex.Message}", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        [RelayCommand]
        private async Task DownloadCoaTemplateAsync()
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    FileName = "COA_Template.xlsx",
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx"
                };
                if (sfd.ShowDialog() == true)
                {
                    await _importService.CreateTemplateAsync(sfd.FileName);
                    MessageBox.Show("✅ تم إنشاء نموذج الدليل بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ فشل إنشاء النموذج:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ImportCoaFromExcelAsync()
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx"
                };
                if (ofd.ShowDialog() != true) return;

                // ضمان الجذور (لو الشاشة تُفتح لأول مرة)
                await EnsureRootsOnceAsync();

                var result = await _importService.ImportAsync(ofd.FileName, CompanyId, BranchId, ensureRoots: true);
                if (!result.Ok)
                {
                    var msg = string.Join("\n", result.Errors ?? new());
                    MessageBox.Show($"فشل الاستيراد:\n{msg}", "أخطاء", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await LoadTreeAsync();
                await LoadAccountsAsync();
                MessageBox.Show(result.Message!, "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ حدث خطأ أثناء الاستيراد:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task LoadTreeAsync()
        {
            try
            {
                await EnsureRootsOnceAsync();

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
                await EnsureRootsOnceAsync();

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
                    await LoadTreeAsync();
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
                    await LoadTreeAsync();
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
                    await LoadTreeAsync();
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

                var dialog = new AccountDialog(null, SelectedAccount);
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    await LoadAccountsAsync();
                    await LoadTreeAsync();
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
                if (SelectedAccount == null)
                    return;

                var dialog = new AccountDialog(SelectedAccount);

                dialog.AccountSaved += async (_, __) =>
                {
                    try
                    {
                        await ReloadTreeAsync();
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
            // سيتم التفعيل لاحقًا وفق تدفق التطبيق
        }

        [RelayCommand]
        private async Task DeleteAccountAsync(Account account)
        {
            try
            {
                if (account is not null)
                {
                    await _service.DeleteAsync(account.AccountId);
                    await LoadAccountsAsync();
                    await LoadTreeAsync();
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
                await EnsureRootsOnceAsync();

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
