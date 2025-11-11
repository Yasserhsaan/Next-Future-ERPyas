using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.Payments.Models;
using Next_Future_ERP.Features.Payments.Models.Next_Future_ERP.Features.Payments.Models;
using Next_Future_ERP.Features.Payments.Services;
using Next_Future_ERP.Features.Suppliers.Models;
using Next_Future_ERP.Features.Suppliers.Services;
using Next_Future_ERP.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

public partial class SupplierEditViewModel : ObservableObject
{
    private readonly ISuppliersService _service;
    private readonly IPaymentMethodsService _pmService;
    private readonly IPaymentTermsService _termsService;
    private readonly AccountsService _accounts;

    [ObservableProperty] private Supplier model;
    public ObservableCollection<Account> AccountOptions { get; } = new();
    public ObservableCollection<PaymentMethod> AllMethods { get; } = new();
    public ObservableCollection<PaymentTerm> AllTerms { get; } = new();
    public ObservableCollection<SupplierPaymentMethod> SupplierMethods { get; } = new();

    [ObservableProperty] private PaymentMethod? selectedMethodToAdd;
    [ObservableProperty] private SupplierPaymentMethod? selectedSupplierMethod;

    public SupplierEditViewModel(
        ISuppliersService service,
        IPaymentMethodsService pmService,
        IPaymentTermsService termsService,
        AccountsService accounts,
        Supplier model)
    {
        _service = service; _pmService = pmService; _termsService = termsService; _accounts = accounts;
        Model = Clone(model);
        _ = LoadLookupsAsync();
        _ = LoadSupplierMethodsAsync();
    }

    private async Task LoadLookupsAsync()
    {
        AccountOptions.Clear();
        var accounts = await _accounts.GetByCategoryKeyAsync("vendor") ?? await _accounts.GetAccountsOfType2Async();
        foreach (var a in accounts.OrderBy(x => x.AccountCode)) AccountOptions.Add(a);

        AllMethods.Clear();
        foreach (var m in await _pmService.GetAllAsync(null, null)) AllMethods.Add(m);

        AllTerms.Clear();
        foreach (var t in await _termsService.GetAllAsync(null)) AllTerms.Add(t);
    }

    private async Task LoadSupplierMethodsAsync()
    {
        SupplierMethods.Clear();
        if (Model.SupplierID <= 0) return;
        var rows = await _service.GetSupplierPaymentMethodsAsync(Model.SupplierID);
        foreach (var r in rows)
        {
            if (r.Method == null && AllMethods.Count > 0)
                r.Method = AllMethods.FirstOrDefault(m => m.MethodId == r.Method_ID);
            SupplierMethods.Add(r);
        }
    }

    [RelayCommand]
    private void AddMethod()
    {
        if (SelectedMethodToAdd is null) return;
        if (SupplierMethods.Any(x => x.Method_ID == SelectedMethodToAdd.MethodId)) return;

        SupplierMethods.Add(new SupplierPaymentMethod
        {
            SupplierID = Model.SupplierID,
            Method_ID = SelectedMethodToAdd.MethodId,
            Method = SelectedMethodToAdd,
            Is_Default = SupplierMethods.Count == 0
        });
        SelectedMethodToAdd = null;
    }

    [RelayCommand]
    private void RemoveMethod()
    {
        if (SelectedSupplierMethod is null) return;
        bool wasDefault = SelectedSupplierMethod.Is_Default;
        SupplierMethods.Remove(SelectedSupplierMethod);
        if (wasDefault && SupplierMethods.Count > 0)
        {
            foreach (var r in SupplierMethods) r.Is_Default = false;
            SupplierMethods[0].Is_Default = true;
            Model.DefaultPaymentMethodID = SupplierMethods[0].Method_ID;
        }
    }

    [RelayCommand]
    private void MakeDefault()
    {
        if (SelectedSupplierMethod is null) return;
        foreach (var r in SupplierMethods) r.Is_Default = false;
        SelectedSupplierMethod.Is_Default = true;
        Model.DefaultPaymentMethodID = SelectedSupplierMethod.Method_ID;
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        try
        {
            // التحقق من صحة البيانات الأساسية
            if (string.IsNullOrWhiteSpace(Model.SupplierCode))
            {
                MessageBox.Show("كود المورد مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.SupplierName))
            {
                MessageBox.Show("اسم المورد مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.TaxNumber))
            {
                MessageBox.Show("الرقم الضريبي مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Model.AccountID <= 0)
            {
                MessageBox.Show("الحساب مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Model.SupplierID == 0)
            {
                var id = await _service.AddAsync(Clone(Model));
                Model.SupplierID = id;
                MessageBox.Show("تم إضافة المورد بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                await _service.UpdateAsync(Clone(Model));
                MessageBox.Show("تم تحديث المورد بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            await _service.SetSupplierPaymentMethodsAsync(
                Model.SupplierID,
                SupplierMethods.Select(x => new SupplierPaymentMethod
                {
                    SupplierID = Model.SupplierID,
                    Method_ID = x.Method_ID,
                    Is_Default = x.Is_Default
                })
            );

            CloseRequested?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand] public void Cancel() => CloseRequested?.Invoke(this, false);

    public event EventHandler<bool>? CloseRequested;

    private static Supplier Clone(Supplier s) => new()
    {
        SupplierID = s.SupplierID,
        SupplierCode = s.SupplierCode,
        SupplierName = s.SupplierName,
        TaxNumber = s.TaxNumber,
        AccountID = s.AccountID,
        CostCenterID = s.CostCenterID,
        PaymentTerms = s.PaymentTerms,
        CreditLimit = s.CreditLimit,
        ContactPerson = s.ContactPerson,
        Phone = s.Phone,
        Email = s.Email,
        Address = s.Address,
        IsActive = s.IsActive,
        Nationality = s.Nationality,
        IDNumber = s.IDNumber,
        CRNumber = s.CRNumber,
        VATNumber = s.VATNumber,
        DefaultPaymentMethodID = s.DefaultPaymentMethodID
    };
}
