using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items.Services;
using Next_Future_ERP.Features.Purchases.Models;
using Next_Future_ERP.Features.Purchases.Services;
using Next_Future_ERP.Features.Suppliers.Models;
using Next_Future_ERP.Features.Suppliers.Services;
using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Warehouses.Services;
using System.Collections.ObjectModel;

namespace Next_Future_ERP.Features.Purchases.ViewModels
{
    public partial class PurchaseEditViewModel : ObservableObject
    {
        private readonly IPurchaseTxnsService _service;
        private readonly ISuppliersService _suppliers;
        private readonly IItemsService _items;
        private readonly IUnitsService _units;
        private readonly char _txnType;

        [ObservableProperty] private PurchaseTxn model;
        [ObservableProperty] private PurchaseTxnDetail? selectedDetail;
        [ObservableProperty]
        private bool isReturn; // true إذا كان المرتجع
        
        [ObservableProperty] private bool isParentOrdersPopupOpen;

        public ObservableCollection<PurchaseTxn> ParentOrders { get; } = new(); // أوامر الشراء المعتمدة

        public ObservableCollection<Supplier> Suppliers { get; } = new();
        public ObservableCollection<PurchaseTxnDetail> Details { get; } = new();
        public ObservableCollection<Item> Items { get; } = new();
        public ObservableCollection<UnitModel> Units { get; } = new();

        public event EventHandler<bool>? CloseRequested;

        public string WindowTitle =>
            Model.TxnType == 'P' ? "مستند مشتريات" :
            Model.TxnType == 'R' ? "مرتجع مشتريات" : "مستند";

        public bool ShowExpectedDelivery => Model.TxnType == 'P';
        public bool ShowParentSelector => Model.TxnType == 'R';

        public PurchaseEditViewModel(IPurchaseTxnsService service,
                                     ISuppliersService suppliersService,
                                     IItemsService itemsService,
                                     IUnitsService unitsService,
                                     char txnType,
                                     PurchaseTxn model)
        {
            _service = service;
            _suppliers = suppliersService;
            _items = itemsService;
            _units = unitsService;
            _txnType = txnType;
            Model = Clone(model);
        }

        public static async Task<PurchaseEditViewModel> FromExisting(
            IPurchaseTxnsService service,
            ISuppliersService suppliersService,
            IItemsService itemsService,
            IUnitsService unitsService,
            int txnId)
        {
            var head = await service.GetByIdAsync(txnId)
                       ?? throw new InvalidOperationException("المستند غير موجود.");
            var vm = new PurchaseEditViewModel(service, suppliersService, itemsService, unitsService, head.TxnType, head);
            await vm.InitializeAsync();
            return vm;
        }

        public async Task InitializeAsync()
        {
            if (Model.TxnID == 0)
            {
                Model.CompanyID = 1;
                Model.BranchID = 1;
                Model.TxnType = _txnType;
            }

            isReturn = Model.TxnType == 'R';        // ← تعيين isReturn
            OnPropertyChanged(nameof(ShowParentSelector)); // ← إخطار الـ UI

            Model.TxnDate = DateTime.Today;
            Model.TxnNumber = await _service.GenerateNextNumberAsync(Model.CompanyID, Model.BranchID, _txnType);

            await LoadLookupsAsync();

            if (Model.TxnID != 0 && Model.TxnType == 'P')
                await LoadDetailsAsync(Model.TxnID);

            if (Model.TxnType == 'R')
            {
                await LoadParentOrdersAsync();       // ← سيتم تحميل أوامر الشراء الأصلية
                if (Model.ParentTxnID.HasValue)
                    await LoadParentDetailsAsync(Model.ParentTxnID.Value);
            }

            RecalcTotals();
        }

        // دالة للحصول على المورد المحدد حالياً (لـ SupplierSearchBox)
        public Supplier? GetSelectedSupplier()
        {
            if (Model.SupplierID <= 0) return null;
            return Suppliers.FirstOrDefault(s => s.SupplierID == Model.SupplierID);
        }


        private async Task LoadLookupsAsync()
        {
            Suppliers.Clear();
            foreach (var s in (await _suppliers.GetAllAsync()).OrderBy(x => x.SupplierName))
                Suppliers.Add(s);

            Items.Clear();
            foreach (var it in (await _items.GetAllAsync()).OrderBy(x => x.ItemName))
                Items.Add(it);

            Units.Clear();
            foreach (var u in (await _units.GetAllAsync()).OrderBy(x => x.UnitName))
                Units.Add(u);
        }

        private async Task LoadDetailsAsync(int txnId)
        {
            Details.Clear();
            var fresh = await _service.GetByIdAsync(txnId);
            if (fresh?.Details != null)
                foreach (var d in fresh.Details.OrderBy(x => x.DetailID))
                    Details.Add(Clone(d));
        }

        private async Task LoadParentOrdersAsync()
        {
            if (!isReturn) return;

            ParentOrders.Clear();
            var orders = await _service.GetApprovedOrdersAsync(); // فقط المعتمدة
            foreach (var o in orders.OrderBy(x => x.TxnNumber))
                ParentOrders.Add(o);
        }


        private async Task LoadParentDetailsAsync(int parentId)
        {
            Details.Clear();
            var parent = await _service.GetByIdAsync(parentId);
            if (parent?.Details != null)
            {
                foreach (var d in parent.Details)
                {
                    Details.Add(new PurchaseTxnDetail
                    {
                        DetailID = 0,
                        TxnID = 0,
                        CompanyID = parent.CompanyID,
                        BranchID = parent.BranchID,
                        ItemID = d.ItemID,
                        UnitID = d.UnitID,
                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice,
                        VATRate = d.VATRate,
                        ReceivedQuantity = 0,
                        IsClosed = false,
                        IsSynced = false
                    });
                }
            }
        }

        [RelayCommand]
        public void AddRow() => Details.Add(new PurchaseTxnDetail 
        { 
            DetailID = 0,
            TxnID = 0,
            CompanyID = Model.CompanyID,
            BranchID = Model.BranchID,
            ItemID = 0,
            UnitID = 0,
            Quantity = 1, 
            UnitPrice = 0, 
            VATRate = 0,
            TaxableAmount = 0,
            VATAmount = 0,
            LineTotal = 0,
            ReceivedQuantity = 0,
            IsClosed = false,
            IsSynced = false
        });

        [RelayCommand]
        public void RemoveRow()
        {
            if (SelectedDetail == null) return;
            Details.Remove(SelectedDetail);
            SelectedDetail = null;
            RecalcTotals();
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
        public async Task SaveAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("PurchaseEditViewModel.SaveAsync: Starting save operation");
                
                RecalcTotals();
                
                if (Model.TxnID == 0)
                {
                    System.Diagnostics.Debug.WriteLine("PurchaseEditViewModel.SaveAsync: Adding new transaction");
                    var id = await _service.AddAsync(Clone(Model), Details.Select(Clone).ToList());
                    Model.TxnID = id;
                    System.Diagnostics.Debug.WriteLine($"PurchaseEditViewModel.SaveAsync: Successfully added transaction with ID: {id}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"PurchaseEditViewModel.SaveAsync: Updating transaction with ID: {Model.TxnID}");
                    await _service.UpdateAsync(Clone(Model), Details.Select(Clone).ToList());
                    System.Diagnostics.Debug.WriteLine($"PurchaseEditViewModel.SaveAsync: Successfully updated transaction with ID: {Model.TxnID}");
                }
                
                CloseRequested?.Invoke(this, true);
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Debug.WriteLine($"PurchaseEditViewModel.SaveAsync: Validation error: {ex.Message}");
                
                // عرض رسالة خطأ واضحة للمستخدم
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        System.Windows.MessageBox.Show(
                            $"خطأ في حفظ المستند:\n\n{ex.Message}\n\nيرجى التحقق من البيانات المدخلة والمحاولة مرة أخرى.",
                            "خطأ في الحفظ",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Warning);
                    }
                    catch (Exception messageBoxEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"MessageBox error: {messageBoxEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"Original error: {ex.Message}");
                    }
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PurchaseEditViewModel.SaveAsync: Unexpected error: {ex.Message}");
                
                // عرض رسالة خطأ عامة للمستخدم
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        System.Windows.MessageBox.Show(
                            $"حدث خطأ غير متوقع أثناء حفظ المستند:\n\n{ex.Message}\n\nيرجى المحاولة مرة أخرى أو الاتصال بالدعم الفني.",
                            "خطأ في النظام",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error);
                    }
                    catch (Exception messageBoxEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"MessageBox error: {messageBoxEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"Original error: {ex.Message}");
                    }
                }));
            }
        }

        [RelayCommand] public void Cancel() => CloseRequested?.Invoke(this, false);

        [RelayCommand]
        public void SelectParentOrder()
        {
            if (isReturn)
            {
                IsParentOrdersPopupOpen = true;
            }
        }

        [RelayCommand]
        public void CloseParentOrdersPopup()
        {
            IsParentOrdersPopupOpen = false;
        }

        [RelayCommand]
        public void ConfirmParentOrder()
        {
            if (SelectedParentOrder != null)
            {
                FillReturnData(SelectedParentOrder);
                IsParentOrdersPopupOpen = false;
            }
        }

        private static PurchaseTxn Clone(PurchaseTxn h) => new()
        {
            TxnID = h.TxnID,
            CompanyID = h.CompanyID,
            BranchID = h.BranchID,
            TxnNumber = h.TxnNumber,
            TxnType = h.TxnType,
            SupplierID = h.SupplierID,
            TxnDate = h.TxnDate,
            ExpectedDelivery = h.ExpectedDelivery,
            Status = h.Status,
            SubTotal = h.SubTotal,
            TaxAmount = h.TaxAmount,
            TotalAmount = h.TotalAmount,
            Remarks = h.Remarks,
            CreatedAt = h.CreatedAt,
            CreatedBy = h.CreatedBy,
            ModifiedAt = h.ModifiedAt,
            ModifiedBy = h.ModifiedBy,
            ParentTxnID = h.ParentTxnID,
            IsSynced = h.IsSynced
        };

        private static PurchaseTxnDetail Clone(PurchaseTxnDetail d) => new()
        {
            DetailID = 0,
            TxnID = d.TxnID,
            CompanyID = d.CompanyID,
            BranchID = d.BranchID,
            ItemID = d.ItemID,
            Quantity = d.Quantity,
            UnitID = d.UnitID,
            UnitPrice = d.UnitPrice,
            TaxableAmount = d.TaxableAmount,
            VATRate = d.VATRate,
            VATAmount = d.VATAmount,
            LineTotal = d.LineTotal,
            ReceivedQuantity = d.ReceivedQuantity,
            IsClosed = d.IsClosed,
            IsSynced = d.IsSynced
        };

        private PurchaseTxn? selectedParentOrder;
        public PurchaseTxn? SelectedParentOrder
        {
            get => selectedParentOrder;
            set
            {
                SetProperty(ref selectedParentOrder, value);
                if (value != null)
                {
                    FillReturnData(value);
                }
            }
        }
        private void FillReturnData(PurchaseTxn parent)
        {
            Model.SupplierID = parent.SupplierID;
            Model.ParentTxnID = parent.TxnID;
            Model.Remarks = $"مرتجع لـ {parent.TxnNumber}";

            Details.Clear();
            foreach (var d in parent.Details)
            {
                Details.Add(new PurchaseTxnDetail
                {
                    DetailID = 0,
                    TxnID = 0,
                    CompanyID = parent.CompanyID,
                    BranchID = parent.BranchID,
                    ItemID = d.ItemID,
                    UnitID = d.UnitID,
                    UnitPrice = d.UnitPrice,
                    Quantity = d.Quantity,  // يمكن للمستخدم تعديله فقط
                    TaxableAmount = d.TaxableAmount,
                    VATRate = d.VATRate,
                    VATAmount = d.VATAmount,
                    LineTotal = d.LineTotal,
                    ReceivedQuantity = 0,
                    IsClosed = false,
                    IsSynced = false
                });
            }
        }


    }
}
