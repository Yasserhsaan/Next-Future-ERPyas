using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Payments.Models;
using Next_Future_ERP.Features.Payments.Services;

namespace Next_Future_ERP.Features.Payments.ViewModels
{
    public partial class PaymentTermsViewModel : ObservableObject
    {
        private readonly IPaymentTermsService _service;

        public ObservableCollection<PaymentTerm> Items { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private PaymentTerm current = new() { IsActive = true, NetDays = 0 };
        [ObservableProperty] private string? searchText;

        public PaymentTermsViewModel(IPaymentTermsService service)
        {
            _service = service;
            _ = LoadAsync();
        }

        partial void OnSearchTextChanged(string? oldValue, string? newValue) => _ = LoadAsync();

        [RelayCommand]
        public async Task LoadAsync()
        {
            var keepId = Current?.TermId;

            var list = await _service.GetAllAsync(SearchText);
            Items.Clear();
            foreach (var x in list) Items.Add(x);

            Current = Items.FirstOrDefault(x => x.TermId == keepId)
                   ?? Items.FirstOrDefault()
                   ?? new PaymentTerm { IsActive = true, NetDays = 0 };
        }

        [RelayCommand] public void New() => Current = new PaymentTerm { IsActive = true, NetDays = 0 };

        [RelayCommand]
        public async Task SaveAsync()
        {
            try
            {
                if (Current.TermId == 0)
                {
                    var dto = Clone(Current);
                    var id = await _service.AddAsync(dto); // AddAsync تُرجع int
                    Current.TermId = id;
                }
                else
                {
                    await _service.UpdateAsync(Clone(Current));
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
                await _service.DeleteAsync(Current.TermId);
                await LoadAsync();
                Current = new PaymentTerm { IsActive = true, NetDays = 0 };
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ أثناء الحذف");
            }
        }

        private bool CanDelete() => Current is { TermId: > 0 };

        private static PaymentTerm Clone(PaymentTerm s) => new()
        {
            TermId = s.TermId,
            TermCode = s.TermCode,
            TermName = s.TermName,
            NetDays = s.NetDays,
            DiscountPercent = s.DiscountPercent,
            DiscountDays = s.DiscountDays,
            LateFeePercent = s.LateFeePercent,
            IsActive = s.IsActive
        };
    }
}
