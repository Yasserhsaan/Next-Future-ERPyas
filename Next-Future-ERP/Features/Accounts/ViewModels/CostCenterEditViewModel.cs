using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Models;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class CostCenterEditViewModel : ObservableObject
    {
        private readonly ICostCentersService _service;

        [ObservableProperty] 
        private CostCenter model;

        public event EventHandler<bool>? CloseRequested;

        public string WindowTitle => Model.CostCenterId == 0 ? "مركز تكلفة جديد" : "تعديل مركز التكلفة";

        public CostCenterEditViewModel(ICostCentersService service, CostCenter model)
        {
            _service = service;
            Model = Clone(model);
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            try
            {
                // التحقق من صحة البيانات قبل الحفظ
                if (string.IsNullOrWhiteSpace(Model.CostCenterName))
                {
                    MessageBox.Show("اسم مركز التكلفة مطلوب.", "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // تنظيف البيانات
                Model.CostCenterName = Model.CostCenterName.Trim();
                Model.LinkedAccounts = Model.LinkedAccounts?.Trim() ?? string.Empty;
                Model.Classification = Model.Classification?.Trim() ?? string.Empty;

                if (Model.CostCenterId == 0)
                {
                    var id = await _service.AddAsync(Clone(Model));
                    Model.CostCenterId = id;
                    MessageBox.Show("تم إضافة مركز التكلفة بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _service.UpdateAsync(Clone(Model));
                    MessageBox.Show("تم تحديث مركز التكلفة بنجاح.", "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                CloseRequested?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand] 
        public void Cancel() => CloseRequested?.Invoke(this, false);

        private static CostCenter Clone(CostCenter model) => new()
        {
            CostCenterId = model.CostCenterId,
            CostCenterName = model.CostCenterName,
            LinkedAccounts = model.LinkedAccounts,
            Classification = model.Classification,
            IsActive = model.IsActive,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        };
    }
}
