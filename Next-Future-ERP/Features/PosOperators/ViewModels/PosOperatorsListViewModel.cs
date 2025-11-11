using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.PosOperators.Services;
using Next_Future_ERP.Features.PosOperators.Models;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Auth.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Next_Future_ERP.Features.PosOperators.Views;

namespace Next_Future_ERP.Features.PosOperators.ViewModels
{
    /// <summary>
    /// ViewModel لقائمة مشغلي نقاط البيع
    /// </summary>
    public partial class PosOperatorsListViewModel : ObservableObject, IDisposable
    {
        private readonly IPosOperatorService _posOperatorService;
        private readonly IServiceProvider _serviceProvider;
        private bool _disposed = false;

        [ObservableProperty]
        private ObservableCollection<PosOperator> _operators = new();

        [ObservableProperty]
        private PosOperator? _selectedOperator;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _showActiveOnly = true;

        public PosOperatorsListViewModel(IPosOperatorService posOperatorService, IServiceProvider serviceProvider)
        {
            _posOperatorService = posOperatorService;
            _serviceProvider = serviceProvider;
            
            LoadOperatorsCommand = new AsyncRelayCommand(LoadOperatorsAsync);
            SearchCommand = new AsyncRelayCommand(SearchAsync);
            AddOperatorCommand = new RelayCommand(AddOperator);
            EditOperatorCommand = new RelayCommand<PosOperator>(EditOperator);
            EditSelectedOperatorCommand = new RelayCommand(EditSelectedOperator);
            DeleteOperatorCommand = new AsyncRelayCommand<PosOperator>(DeleteOperatorAsync);
            ToggleActiveCommand = new AsyncRelayCommand<PosOperator>(ToggleActiveAsync);
            RefreshCommand = new AsyncRelayCommand(LoadOperatorsAsync);
            ExportCommand = new AsyncRelayCommand(ExportAsync);
        }

        public IAsyncRelayCommand LoadOperatorsCommand { get; }
        public IAsyncRelayCommand SearchCommand { get; }
        public IRelayCommand AddOperatorCommand { get; }
        public IRelayCommand<PosOperator> EditOperatorCommand { get; }
        public IRelayCommand EditSelectedOperatorCommand { get; }
        public IAsyncRelayCommand<PosOperator> DeleteOperatorCommand { get; }
        public IAsyncRelayCommand<PosOperator> ToggleActiveCommand { get; }
        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand ExportCommand { get; }

        public async Task InitializeAsync()
        {
            await LoadOperatorsAsync();
        }

        private async Task LoadOperatorsAsync()
        {
            IsLoading = true;
            StatusMessage = "جاري تحميل مشغلي نقاط البيع...";
            try
            {
                // جرب تحميل جميع البيانات أولاً للتأكد من وجودها
                var allResult = await _posOperatorService.GetAllAsync();
                System.Diagnostics.Debug.WriteLine($"LoadOperatorsAsync: Found {allResult.Count} total operators");
                
                // ثم جرب مع الفلتر
                var result = await _posOperatorService.GetAllAsync(
                    companyId: null, // جرب بدون فلتر أولاً
                    branchId: null,
                    isActive: ShowActiveOnly ? true : null
                );
                System.Diagnostics.Debug.WriteLine($"LoadOperatorsAsync: Found {result.Count} filtered operators");
                
                // إذا لم توجد بيانات مع الفلتر، استخدم جميع البيانات
                if (result.Count == 0 && allResult.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("LoadOperatorsAsync: No filtered operators found, using all operators");
                    result = allResult;
                }
                
                Operators = new ObservableCollection<PosOperator>(result.OrderBy(o => o.PosStationName).ThenBy(o => o.UserName));
                StatusMessage = $"تم تحميل {Operators.Count} مشغل نقطة بيع.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في تحميل مشغلي نقاط البيع: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"LoadOperatorsAsync Error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"خطأ في تحميل مشغلي نقاط البيع: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchAsync()
        {
            IsLoading = true;
            StatusMessage = "جاري البحث...";
            try
            {
                var result = await _posOperatorService.SearchAsync(SearchText);
                Operators = new ObservableCollection<PosOperator>(result.OrderBy(o => o.PosStationName).ThenBy(o => o.UserName));
                StatusMessage = $"تم العثور على {Operators.Count} مشغل نقطة بيع.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في البحث عن مشغلي نقاط البيع: {ex.Message}";
                MessageBox.Show($"خطأ في البحث عن مشغلي نقاط البيع: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// إضافة مشغل نقطة بيع جديد
        /// </summary>
        private async void AddOperator()
        {
            try
            {
                var editWindow = _serviceProvider.GetRequiredService<PosOperatorEditWindow>();
                var editViewModel = _serviceProvider.GetRequiredService<PosOperatorEditViewModel>();
                
                await editViewModel.Initialize(new PosOperator());
                await editViewModel.InitializeAsync(); // تحميل البيانات المساعدة

                editWindow.DataContext = editViewModel;
                
                if (editWindow.ShowDialog() == true)
                {
                    LoadOperatorsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ في فتح نافذة إضافة مشغل نقطة البيع:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// تعديل مشغل نقطة بيع
        /// </summary>
        private async void EditOperator(PosOperator? posOperator)
        {
            if (posOperator == null) return;

            try
            {
                var editViewModel = await PosOperatorEditViewModel.FromExisting(
                    _posOperatorService,
                    _serviceProvider,
                    _serviceProvider.GetRequiredService<AppDbContext>(),
                    _serviceProvider.GetRequiredService<ISessionService>(),
                    posOperator.OperatorId);

                var editWindow = _serviceProvider.GetRequiredService<PosOperatorEditWindow>();
                editWindow.DataContext = editViewModel;
                
                if (editWindow.ShowDialog() == true)
                {
                    LoadOperatorsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ في فتح نافذة تعديل مشغل نقطة البيع:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditSelectedOperator()
        {
            if (SelectedOperator == null)
            {
                MessageBox.Show("يرجى اختيار مشغل نقطة بيع للتعديل عليها", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EditOperator(SelectedOperator);
        }

        private async Task DeleteOperatorAsync(PosOperator? posOperator)
        {
            if (posOperator == null) return;

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف مشغل نقطة البيع '{posOperator.UserName}' من '{posOperator.PosStationName}'؟\nهذا الإجراء لا يمكن التراجع عنه.",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                IsLoading = true;
                StatusMessage = "جاري حذف مشغل نقطة البيع...";
                try
                {
                    await _posOperatorService.DeleteAsync(posOperator.OperatorId);
                    Operators.Remove(posOperator);
                    StatusMessage = "تم حذف مشغل نقطة البيع بنجاح.";
                    MessageBox.Show("تم حذف مشغل نقطة البيع بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    StatusMessage = $"خطأ في حذف مشغل نقطة البيع: {ex.Message}";
                    MessageBox.Show($"خطأ في حذف مشغل نقطة البيع: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task ToggleActiveAsync(PosOperator? posOperator)
        {
            if (posOperator == null) return;

            IsLoading = true;
            StatusMessage = "جاري تحديث حالة مشغل نقطة البيع...";
            try
            {
                await _posOperatorService.ToggleActiveAsync(posOperator.OperatorId);
                posOperator.IsActive = !posOperator.IsActive; // Update UI
                StatusMessage = "تم تحديث حالة مشغل نقطة البيع بنجاح.";
                MessageBox.Show("تم تحديث حالة مشغل نقطة البيع بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في تحديث حالة مشغل نقطة البيع: {ex.Message}";
                MessageBox.Show($"خطأ في تحديث حالة مشغل نقطة البيع: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Task ExportAsync()
        {
            StatusMessage = "وظيفة التصدير قيد التنفيذ.";
            MessageBox.Show("وظيفة التصدير قيد التنفيذ.", "تصدير", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;
            }
        }

        ~PosOperatorsListViewModel()
        {
            Dispose(false);
        }
    }
}
