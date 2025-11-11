using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Payments.Models;
using Next_Future_ERP.Features.Payments.Models.Next_Future_ERP.Features.Payments.Models;
using Next_Future_ERP.Features.Payments.Services;
using Next_Future_ERP.Models;
using Next_Future_ERP.Features.Accounts.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.Payments.ViewModels
{
    public partial class PaymentMethodsViewModel : ObservableObject
    {
        private readonly IPaymentMethodsService _methods;
        private readonly IPaymentTypesService _types;
        private readonly AccountsService _accounts = new();

        public ObservableCollection<PaymentMethod> Items { get; } = new();
        public ObservableCollection<PaymentType> PaymentTypes { get; } = new();
        public ObservableCollection<Account> BankAccounts { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private PaymentMethod current = new() { IsActive = true, RequiresApproval = false, SupportsSplit = false };

        [ObservableProperty] private byte? selectedTypeFilter;
        [ObservableProperty] private string? searchText;

        public PaymentMethodsViewModel(IPaymentMethodsService methods, IPaymentTypesService types)
        {
            _methods = methods;
            _types = types;
            _ = LoadLookupsAsync();
            _ = LoadAsync();
        }

        private async Task LoadLookupsAsync()
        {
            PaymentTypes.Clear();
            var types = await _types.GetAllAsync();
            foreach (var t in types) PaymentTypes.Add(t);

            BankAccounts.Clear();
            var accounts = await _accounts.GetByCategoryKeyAsync("bank");
            if (accounts == null || accounts.Count == 0)
                accounts = await _accounts.GetAccountsOfType2Async();
            foreach (var a in accounts.OrderBy(x => x.AccountCode)) BankAccounts.Add(a);
        }

        partial void OnSelectedTypeFilterChanged(byte? oldValue, byte? newValue) => _ = LoadAsync();
        partial void OnSearchTextChanged(string? oldValue, string? newValue) => _ = LoadAsync();

        [RelayCommand]
        public async Task LoadAsync()
        {
            var keepId = Current?.MethodId;

            var list = await _methods.GetAllAsync(SelectedTypeFilter, SearchText);
            Items.Clear();
            foreach (var m in list) Items.Add(m);

            Current = Items.FirstOrDefault(x => x.MethodId == keepId)
                   ?? Items.FirstOrDefault()
                   ?? new PaymentMethod { IsActive = true, RequiresApproval = false, SupportsSplit = false };
        }

        [RelayCommand]
        public void New()
            => Current = new PaymentMethod { IsActive = true, RequiresApproval = false, SupportsSplit = false };

        [RelayCommand]
        public async Task SaveAsync()
        {
            try
            {
                if (Current.MethodId == 0)
                {
                    var dto = Clone(Current);
                    var id = await _methods.AddAsync(dto);
                    Current.MethodId = id;
                }
                else
                {
                    await _methods.UpdateAsync(Clone(Current));
                }

                await LoadAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ أثناء الحفظ");
            }
        }

        [RelayCommand(CanExecute = nameof(CanDelete))]
        public async Task DeleteAsync()
        {
            try
            {
                await _methods.DeleteAsync(Current.MethodId);
                await LoadAsync();
                Current = new PaymentMethod { IsActive = true, RequiresApproval = false, SupportsSplit = false };
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ أثناء الحذف");
            }
        }

        private bool CanDelete() => Current is { MethodId: > 0 };

        private static PaymentMethod Clone(PaymentMethod s) => new()
        {
            MethodId = s.MethodId,
            MethodName = s.MethodName,
            GLAccount = s.GLAccount,
            PaymentTypeId = s.PaymentTypeId,
            ProviderId = s.ProviderId,
            RequiresApproval = s.RequiresApproval,
            IsActive = s.IsActive,
            SupportsSplit = s.SupportsSplit
        };
    }
}
