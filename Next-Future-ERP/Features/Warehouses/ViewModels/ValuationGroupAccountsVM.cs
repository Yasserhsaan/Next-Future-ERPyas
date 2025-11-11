using CommunityToolkit.Mvvm.ComponentModel;

namespace Next_Future_ERP.Features.Warehouses.ViewModels
{
    public partial class ValuationGroupAccountsVM : ObservableObject
    {
        [ObservableProperty] private string? inventoryAcc;
        [ObservableProperty] private string? cogsAcc;             // ← CogsAcc
        [ObservableProperty] private string? salesAcc;
        [ObservableProperty] private string? salesDiscountAcc;
        [ObservableProperty] private string? lossAcc;
        [ObservableProperty] private string? adjustmentAcc;
        [ObservableProperty] private string? earnedDiscountAccount;
        [ObservableProperty] private string? expenseAcc;
        [ObservableProperty] private string? taxAccPurchase;
    }

}
