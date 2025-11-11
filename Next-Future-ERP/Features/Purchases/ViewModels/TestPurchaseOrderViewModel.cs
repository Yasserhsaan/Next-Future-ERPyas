using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Items.Controls;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Purchases.Models;
using Next_Future_ERP.Features.Suppliers.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace Next_Future_ERP.Features.Purchases.ViewModels
{
    public partial class TestPurchaseOrderViewModel : ObservableObject
    {
        [ObservableProperty] private PurchaseTxn model;
        [ObservableProperty] private PurchaseTxnDetail? selectedDetail;
        [ObservableProperty] private Item? selectedItem;

        public ObservableCollection<PurchaseTxnDetail> Details { get; } = new();
        public ObservableCollection<Supplier> Suppliers { get; } = new();

        public event EventHandler<bool>? CloseRequested;

        public string WindowTitle => "تجربة أمر الشراء";

        public TestPurchaseOrderViewModel()
        {
            Model = new PurchaseTxn
            {
                TxnType = 'P',
                TxnDate = DateTime.Today,
                Status = 0,
                CompanyID = 1,
                BranchID = 1,
                TxnNumber = "P-TEST-001"
            };

            // إضافة بعض البيانات التجريبية
            LoadTestData();
        }

        private void LoadTestData()
        {
            // إضافة موردين تجريبيين
            Suppliers.Add(new Supplier { SupplierID = 1, SupplierName = "مورد تجريبي 1" });
            Suppliers.Add(new Supplier { SupplierID = 2, SupplierName = "مورد تجريبي 2" });

            // إضافة تفاصيل تجريبية
            Details.Add(new PurchaseTxnDetail
            {
                DetailID = 1,
                ItemID = 1,
                Quantity = 10,
                UnitPrice = 100,
                VATRate = 15,
                TaxableAmount = 1000,
                VATAmount = 150,
                LineTotal = 1150
            });
        }

        [RelayCommand]
        public void AddRow()
        {
            Details.Add(new PurchaseTxnDetail
            {
                DetailID = 0,
                TxnID = 0,
                CompanyID = Model.CompanyID,
                BranchID = Model.BranchID,
                ItemID = 0,
                UnitID = 1,
                Quantity = 1,
                UnitPrice = 0,
                VATRate = 15,
                TaxableAmount = 0,
                VATAmount = 0,
                LineTotal = 0,
                ReceivedQuantity = 0,
                IsClosed = false,
                IsSynced = false
            });
        }

        [RelayCommand]
        public void RemoveRow()
        {
            if (SelectedDetail != null)
            {
                Details.Remove(SelectedDetail);
                SelectedDetail = null;
                RecalcTotals();
            }
        }

        [RelayCommand]
        public void RecalcTotals()
        {
            foreach (var d in Details)
            {
                d.TaxableAmount = Math.Round(d.Quantity * d.UnitPrice, 4);
                d.VATAmount = Math.Round(d.TaxableAmount * (d.VATRate / 100m), 4);
                d.LineTotal = Math.Round(d.TaxableAmount + d.VATAmount, 4);
            }
            Model.SubTotal = Math.Round(Details.Sum(x => x.TaxableAmount), 4);
            Model.TaxAmount = Math.Round(Details.Sum(x => x.VATAmount), 4);
            Model.TotalAmount = Math.Round((Model.SubTotal ?? 0) + (Model.TaxAmount ?? 0), 4);
            OnPropertyChanged(nameof(Model));
        }

        [RelayCommand]
        public void Save()
        {
            MessageBox.Show("تم حفظ التجربة بنجاح!", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
            CloseRequested?.Invoke(this, true);
        }

        [RelayCommand]
        public void Cancel() => CloseRequested?.Invoke(this, false);

        public void OnItemSelected(object sender, Item item)
        {
            SelectedItem = item;
            MessageBox.Show($"تم اختيار الصنف: {item.ItemName}", "تم الاختيار", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
