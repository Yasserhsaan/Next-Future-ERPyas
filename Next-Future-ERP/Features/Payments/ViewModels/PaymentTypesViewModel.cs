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
    public partial class PaymentTypesViewModel : ObservableObject
    {
        private readonly IPaymentTypesService _service;

        public ObservableCollection<PaymentType> Items { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private PaymentType current = new(); // Identity → اتركه 0 للإضافة

        [ObservableProperty] private string? searchText;

        public PaymentTypesViewModel(IPaymentTypesService service)
        {
            _service = service;
            _ = LoadAsync();
        }

        partial void OnSearchTextChanged(string? oldValue, string? newValue) => _ = LoadAsync();

        [RelayCommand]
        public async Task LoadAsync()
        {
            var keepId = Current?.TypeId;

            var list = await _service.GetAllAsync(SearchText);
            Items.Clear();
            foreach (var x in list) Items.Add(x);

            Current = Items.FirstOrDefault(x => x.TypeId == keepId)
                   ?? Items.FirstOrDefault()
                   ?? new PaymentType();
        }

        [RelayCommand] public void New() => Current = new PaymentType();

        [RelayCommand]
        public async Task SaveAsync()
        {
            try
            {
                if (Current.TypeId == 0)
                {
                    // إضافة
                    var dto = Clone(Current);
                    var id = await _service.AddAsync(dto);
                    Current.TypeId = id;
                }
                else
                {
                    // تعديل
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
                await _service.DeleteAsync(Current.TypeId);
                await LoadAsync();
                Current = new PaymentType();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ أثناء الحذف");
            }
        }

        private bool CanDelete() => Current is { TypeId: > 0 };

        private static PaymentType Clone(PaymentType s) => new()
        {
            TypeId = s.TypeId,
            Code = s.Code,
            Description = s.Description
        };
    }
}
