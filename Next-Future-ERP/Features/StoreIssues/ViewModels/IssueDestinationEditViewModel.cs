using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Models;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Features.StoreIssues.Models;
using Next_Future_ERP.Features.StoreIssues.Services;
using Next_Future_ERP.Models;
using System.Collections.ObjectModel;

namespace Next_Future_ERP.Features.StoreIssues.ViewModels
{
    public partial class IssueDestinationEditViewModel : ObservableObject
    {
        private readonly IIssueDestinationsService _service;
        private readonly AccountsService _accountsService;
        private readonly ICostCentersService _costCentersService;

        [ObservableProperty] private IssueDestination model;
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private bool isEditMode;

        // Data for dropdowns
        public ObservableCollection<Account> Accounts { get; } = new();
        public ObservableCollection<CostCenter> CostCenters { get; } = new();

        public event EventHandler<bool>? CloseRequested;

        public IssueDestinationEditViewModel(IssueDestination model, IIssueDestinationsService service, AccountsService accountsService, ICostCentersService costCentersService)
        {
            _service = service;
            _accountsService = accountsService;
            _costCentersService = costCentersService;
            Model = model;
            IsEditMode = model.DestinationID > 0;
        }

        public string WindowTitle => IsEditMode ? "تعديل جهة الصرف" : "إضافة جهة صرف جديدة";

        [RelayCommand]
        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine("IssueDestinationEditViewModel.InitializeAsync: Starting initialization");

                // Load accounts
                var accounts = await _accountsService.GetAllAsync();
                Accounts.Clear();
                foreach (var account in accounts)
                {
                    Accounts.Add(account);
                }

                // Load cost centers
                var costCenters = await _costCentersService.GetAllAsync();
                CostCenters.Clear();
                foreach (var costCenter in costCenters)
                {
                    CostCenters.Add(costCenter);
                }

                // Generate code if new
                if (!IsEditMode && string.IsNullOrEmpty(Model.DestinationCode))
                {
                    Model.DestinationCode = await _service.GenerateNextCodeAsync(Model.CompanyID, Model.BranchID);
                }

                System.Diagnostics.Debug.WriteLine("IssueDestinationEditViewModel.InitializeAsync: Initialization completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IssueDestinationEditViewModel.InitializeAsync: Error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine("IssueDestinationEditViewModel.SaveAsync: Starting save operation");

                if (IsEditMode)
                {
                    await _service.UpdateAsync(Model);
                    System.Diagnostics.Debug.WriteLine($"IssueDestinationEditViewModel.SaveAsync: Successfully updated destination with ID: {Model.DestinationID}");
                }
                else
                {
                    var id = await _service.AddAsync(Model);
                    Model.DestinationID = id;
                    System.Diagnostics.Debug.WriteLine($"IssueDestinationEditViewModel.SaveAsync: Successfully added destination with ID: {id}");
                }

                CloseRequested?.Invoke(this, true);
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Debug.WriteLine($"IssueDestinationEditViewModel.SaveAsync: Validation error: {ex.Message}");
                
                // عرض رسالة خطأ واضحة للمستخدم
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        System.Windows.MessageBox.Show(
                            $"خطأ في حفظ جهة الصرف:\n\n{ex.Message}\n\nيرجى التحقق من البيانات المدخلة والمحاولة مرة أخرى.",
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
                System.Diagnostics.Debug.WriteLine($"IssueDestinationEditViewModel.SaveAsync: Unexpected error: {ex.Message}");
                
                // عرض رسالة خطأ عامة للمستخدم
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        System.Windows.MessageBox.Show(
                            $"حدث خطأ غير متوقع أثناء حفظ جهة الصرف:\n\n{ex.Message}\n\nيرجى المحاولة مرة أخرى أو الاتصال بالدعم الفني.",
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
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void Cancel()
        {
            CloseRequested?.Invoke(this, false);
        }

        // Destination type options for ComboBox
        public List<DestinationTypeOption> DestinationTypeOptions { get; } = new()
        {
            new DestinationTypeOption { Value = 'E', Text = "مصروف" },
            new DestinationTypeOption { Value = 'P', Text = "تشغيل" },
            new DestinationTypeOption { Value = 'C', Text = "تكلفة مبيعات" },
            new DestinationTypeOption { Value = 'S', Text = "هالك" },
            new DestinationTypeOption { Value = 'A', Text = "تسوية" },
            new DestinationTypeOption { Value = 'O', Text = "أخرى" }
        };

        // Computed properties for UI
        public string AccountRequiredText => Model.DestinationType switch
        {
            'E' => "حساب المصروف مطلوب",
            'P' => "حساب التشغيل مطلوب",
            'C' => "حساب تكلفة المبيعات مطلوب",
            'S' => "حساب الهالك (اختياري)",
            'A' => "حساب التسوية (اختياري)",
            'O' => "حساب أخرى (اختياري)",
            _ => "حساب مطلوب"
        };

        public bool IsAccountRequired => Model.DestinationType == 'E' || Model.DestinationType == 'P' || Model.DestinationType == 'C';
        public bool IsCostCenterRequired => Model.UsesCostCenter;
    }

    public class DestinationTypeOption
    {
        public char Value { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
