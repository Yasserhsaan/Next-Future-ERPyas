using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Features.PosStations.Services;
using Next_Future_ERP.Features.PosStations.Models;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Auth.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Next_Future_ERP.Features.PosStations.Views;

namespace Next_Future_ERP.Features.PosStations.ViewModels
{
        /// <summary>
        /// ViewModel لقائمة نقاط البيع
        /// </summary>
    public partial class PosStationsListViewModel : ObservableObject, IDisposable
    {
        private readonly IPosStationService _posStationService;
        private readonly IServiceProvider _serviceProvider;
        private bool _disposed = false;

        [ObservableProperty]
        private ObservableCollection<PosStation> _stations = new();

        [ObservableProperty]
        private PosStation? _selectedStation;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _showActiveOnly = true;

        public PosStationsListViewModel(IPosStationService posStationService, IServiceProvider serviceProvider)
        {
            _posStationService = posStationService;
            _serviceProvider = serviceProvider;
            
            LoadStationsCommand = new AsyncRelayCommand(LoadStationsAsync);
            SearchCommand = new AsyncRelayCommand(SearchAsync);
            AddStationCommand = new RelayCommand(AddStation);
            EditStationCommand = new RelayCommand<PosStation>(EditStation);
            EditSelectedStationCommand = new RelayCommand(EditSelectedStation);
            DeleteStationCommand = new AsyncRelayCommand<PosStation>(DeleteStationAsync);
            ToggleActiveCommand = new AsyncRelayCommand<PosStation>(ToggleActiveAsync);
            RefreshCommand = new AsyncRelayCommand(LoadStationsAsync);
            ExportCommand = new AsyncRelayCommand(ExportAsync);
        }

        public IAsyncRelayCommand LoadStationsCommand { get; }
        public IAsyncRelayCommand SearchCommand { get; }
        public IRelayCommand AddStationCommand { get; }
        public IRelayCommand<PosStation> EditStationCommand { get; }
        public IRelayCommand EditSelectedStationCommand { get; }
        public IAsyncRelayCommand<PosStation> DeleteStationCommand { get; }
        public IAsyncRelayCommand<PosStation> ToggleActiveCommand { get; }
        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand ExportCommand { get; }

        /// <summary>
        /// تحميل نقاط البيع
        /// </summary>
        private async Task LoadStationsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "جاري تحميل نقاط البيع...";

                var stations = await _posStationService.GetAllAsync(
                    companyId: 1, // يجب الحصول من النظام
                    branchId: null,
                    isActive: ShowActiveOnly ? true : null
                );

                Stations.Clear();
                foreach (var station in stations)
                {
                    Stations.Add(station);
                }

                StatusMessage = $"تم تحميل {Stations.Count} نقطة بيع";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في تحميل البيانات: {ex.Message}";
                MessageBox.Show($"حدث خطأ في تحميل نقاط البيع:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// البحث في نقاط البيع
        /// </summary>
        private async Task SearchAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "جاري البحث...";

                var stations = await _posStationService.SearchAsync(
                    SearchText,
                    companyId: 1, // يجب الحصول من النظام
                    branchId: null
                );

                Stations.Clear();
                foreach (var station in stations)
                {
                    Stations.Add(station);
                }

                StatusMessage = $"تم العثور على {Stations.Count} نقطة بيع";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في البحث: {ex.Message}";
                MessageBox.Show($"حدث خطأ في البحث:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// إضافة نقطة بيع جديدة
        /// </summary>
        private async void AddStation()
        {
            try
            {
                var editWindow = _serviceProvider.GetRequiredService<PosStationEditWindow>();
                var editViewModel = _serviceProvider.GetRequiredService<PosStationEditViewModel>();
                
                await editViewModel.Initialize(new PosStation());

                editWindow.DataContext = editViewModel;
                
                if (editWindow.ShowDialog() == true)
                {
                    LoadStationsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ في فتح نافذة إضافة نقطة البيع:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// تعديل نقطة بيع
        /// </summary>
        private async void EditStation(PosStation? station)
        {
            if (station == null) return;

            // تشخيص
            System.Diagnostics.Debug.WriteLine($"EditStation: station.PosId={station.PosId}, station.PosName={station.PosName}");

            try
            {
                var editViewModel = await PosStationEditViewModel.FromExisting(
                    _posStationService,
                    _serviceProvider,
                    _serviceProvider.GetRequiredService<AppDbContext>(),
                    _serviceProvider.GetRequiredService<ISessionService>(),
                    station.PosId);

                var editWindow = _serviceProvider.GetRequiredService<PosStationEditWindow>();
                editWindow.DataContext = editViewModel;
                editWindow.Owner = Application.Current.MainWindow;
                
                if (editWindow.ShowDialog() == true)
                {
                    LoadStationsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ في فتح نافذة تعديل نقطة البيع:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// تعديل نقطة البيع المحددة
        /// </summary>
        private void EditSelectedStation()
        {
            if (SelectedStation == null)
            {
                MessageBox.Show("يرجى اختيار نقطة بيع للتعديل عليها", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EditStation(SelectedStation);
        }

        /// <summary>
        /// حذف نقطة بيع
        /// </summary>
        private async Task DeleteStationAsync(PosStation? station)
        {
            if (station == null) return;

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف نقطة البيع '{station.PosName}'؟\nهذا الإجراء لا يمكن التراجع عنه.",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    StatusMessage = "جاري حذف المحطة...";

                    var success = await _posStationService.DeleteAsync(station.PosId);
                    
                    if (success)
                    {
                        StatusMessage = "تم حذف المحطة بنجاح";
                        await LoadStationsAsync();
                    }
                    else
                    {
                        StatusMessage = "فشل في حذف المحطة";
                        MessageBox.Show("فشل في حذف المحطة. قد تكون المحطة مستخدمة في معاملات أخرى.",
                            "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"خطأ في الحذف: {ex.Message}";
                    MessageBox.Show($"حدث خطأ في حذف المحطة:\n{ex.Message}", 
                        "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        /// <summary>
        /// تغيير حالة محطة نقطة البيع
        /// </summary>
        private async Task ToggleActiveAsync(PosStation? station)
        {
            if (station == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "جاري تغيير حالة المحطة...";

                var success = await _posStationService.ToggleActiveAsync(station.PosId);
                
                if (success)
                {
                    StatusMessage = $"تم {(station.IsActive ? "إلغاء تفعيل" : "تفعيل")} المحطة بنجاح";
                    await LoadStationsAsync();
                }
                else
                {
                    StatusMessage = "فشل في تغيير حالة المحطة";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في تغيير الحالة: {ex.Message}";
                MessageBox.Show($"حدث خطأ في تغيير حالة المحطة:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// تصدير البيانات
        /// </summary>
        private async Task ExportAsync()
        {
            try
            {
                // TODO: تنفيذ تصدير البيانات إلى Excel أو CSV
                MessageBox.Show("وظيفة التصدير ستكون متاحة قريباً", 
                    "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ في التصدير:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// تحميل البيانات عند تهيئة ViewModel
        /// </summary>
        public async Task InitializeAsync()
        {
            await LoadStationsAsync();
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

        ~PosStationsListViewModel()
        {
            Dispose(false);
        }
    }
}
