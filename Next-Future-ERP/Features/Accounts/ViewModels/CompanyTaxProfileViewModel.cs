using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Next_Future_ERP.Features.Accounts.Models;
using Next_Future_ERP.Features.Accounts.Services;
using System;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


using Next_Future_ERP.Models;

using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public partial class CompanyTaxProfileViewModel : ObservableObject
    {
        private readonly CompanyTaxProfileService _svc = new();

        // ===== Entity & Lookups =====
        [ObservableProperty] private CompanyTaxProfile profile = new();
        [ObservableProperty] private ObservableCollection<CompanyOption> companyOptions = new();
        [ObservableProperty] private ObservableCollection<BranchOption> branchOptions = new();
        [ObservableProperty] private ObservableCollection<BranchOption> searchBranchOptions = new();
        [ObservableProperty] private ObservableCollection<CompanyTaxProfileLookup> searchResults = new();
        [ObservableProperty] private CompanyTaxProfileLookup? selectedLookup;

        // ===== UI State =====
        [ObservableProperty] private bool isHeaderEnabled = true;
        [ObservableProperty] private bool isSearchPanelExpanded = true;
        [ObservableProperty] private bool isResultsPanelExpanded = true;

        // ===== Paging =====
        [ObservableProperty] private int currentPage = 1;
        [ObservableProperty] private int pageSize = 20;
        [ObservableProperty] private int totalCount = 0;
        public bool HasPrev => CurrentPage > 1;
        public bool HasNext => (CurrentPage * PageSize) < TotalCount;
        public string ResultCountText => $"العدد: {TotalCount}";

        // ===== Search fields =====
        [ObservableProperty] private int? searchCompanyId;
        [ObservableProperty] private int? searchBranchId;
        [ObservableProperty] private string? searchVAT;
        [ObservableProperty] private string? searchActivityCode;
        [ObservableProperty] private string? searchTaxOffice;
        [ObservableProperty] private DateTime? searchFrom;
        [ObservableProperty] private DateTime? searchTo;

        // ===== Certificate UI =====
        [ObservableProperty] private string certificateStatusText = "لم يتم إرفاق ملف";
        [ObservableProperty] private string? certificateTooltip;

        public string? SelectedCompanyName => CompanyOptions.FirstOrDefault(c => c.CompanyId == Profile.CompanyId)?.CompanyName;
        public string? SelectedBranchName => BranchOptions.FirstOrDefault(b => b.BranchId == Profile.BranchId)?.BranchName;

        public CompanyTaxProfileViewModel()
        {
            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            CompanyOptions = new ObservableCollection<CompanyOption>(await _svc.GetCompanyOptionsAsync());
            SearchBranchOptions = new ObservableCollection<BranchOption>(await _svc.GetBranchOptionsAsync(null));
            await SearchAsync();
            await NewAsync();
        }

        private void UpdateCertificateUi()
        {
            if (Profile?.TaxCertificate is { Length: > 0 })
            {
                CertificateStatusText = $"مرفق ({Profile.TaxCertificate.Length / 1024} ك.ب)";
                CertificateTooltip = $"حجم الملف: {Profile.TaxCertificate.Length} بايت";
            }
            else
            {
                CertificateStatusText = "لم يتم إرفاق ملف";
                CertificateTooltip = null;
            }
        }

        // ===== Commands =====
        [RelayCommand]
        public async Task NewAsync()
        {
            Profile = new CompanyTaxProfile
            {
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            await LoadBranchesForCompanyAsync(null);
            IsHeaderEnabled = true;
            UpdateCertificateUi();
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                if (Profile.CompanyId <= 0) throw new InvalidOperationException("اختر الشركة.");
                if (string.IsNullOrWhiteSpace(Profile.VATRegistrationNumber)) throw new InvalidOperationException("أدخل الرقم الضريبي (VAT).");

                var id = await _svc.UpsertAsync(Profile);
                var reloaded = await _svc.GetAsync(id);
                if (reloaded != null) Profile = reloaded;

                await SearchAsync();
                MessageBox.Show("تم الحفظ بنجاح.", "حفظ", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"تعذّر الحفظ:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (Profile.ProfileId == 0) return;

            if (MessageBox.Show("متأكد من الحذف؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                await _svc.DeleteAsync(Profile.ProfileId);
                await SearchAsync();
                await NewAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"تعذّر الحذف:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            var (items, total) = await _svc.SearchAsync(
                SearchCompanyId, SearchBranchId, SearchVAT, SearchActivityCode, SearchTaxOffice,
                SearchFrom, SearchTo, CurrentPage, PageSize);

            SearchResults = new ObservableCollection<CompanyTaxProfileLookup>(items);
            TotalCount = total;
            OnPropertyChanged(nameof(HasPrev));
            OnPropertyChanged(nameof(HasNext));
            OnPropertyChanged(nameof(ResultCountText));
        }

        [RelayCommand]
        private void ResetSearch()
        {
            SearchCompanyId = null;
            SearchBranchId = null;
            SearchVAT = null;
            SearchActivityCode = null;
            SearchTaxOffice = null;
            SearchFrom = null;
            SearchTo = null;
            CurrentPage = 1;
            _ = SearchAsync();
        }

        [RelayCommand] private async Task NextPageAsync() { if (!HasNext) return; CurrentPage++; await SearchAsync(); }
        [RelayCommand] private async Task PrevPageAsync() { if (!HasPrev) return; CurrentPage--; await SearchAsync(); }

        [RelayCommand]
        private async Task OpenSelected(object? row)
        {
            var item = row as CompanyTaxProfileLookup ?? SelectedLookup;
            if (item == null) return;

            var loaded = await _svc.GetAsync(item.ProfileId);
            if (loaded != null)
            {
                Profile = loaded;
                await LoadBranchesForCompanyAsync(Profile.CompanyId);
                IsHeaderEnabled = false;
                UpdateCertificateUi();
            }
        }

        [RelayCommand]
        private async Task EditSelected(object? row)
        {
            var item = row as CompanyTaxProfileLookup ?? SelectedLookup;
            if (item == null) return;

            var loaded = await _svc.GetAsync(item.ProfileId);
            if (loaded != null)
            {
                Profile = loaded;
                await LoadBranchesForCompanyAsync(Profile.CompanyId);
                IsHeaderEnabled = true;
                UpdateCertificateUi();
            }
        }

        [RelayCommand]
        private void PickCertificate()
        {
            var dlg = new OpenFileDialog
            {
                Title = "اختر شهادة التسجيل الضريبي",
                Filter = "All Files|*.*|PDF|*.pdf|Images|*.png;*.jpg;*.jpeg",
                Multiselect = false
            };
            if (dlg.ShowDialog() == true)
            {
                Profile.TaxCertificate = File.ReadAllBytes(dlg.FileName);
                UpdateCertificateUi();
            }
        }

        [RelayCommand]
        private void ClearCertificate()
        {
            Profile.TaxCertificate = null;
            UpdateCertificateUi();
        }

        // ===== Helpers =====
        public async Task LoadBranchesForCompanyAsync(int? companyId)
        {
            BranchOptions = new ObservableCollection<BranchOption>(await _svc.GetBranchOptionsAsync(companyId));
        }

        public async Task CompanyChangedAsync(int? companyId)
        {
            await LoadBranchesForCompanyAsync(companyId);
            Profile.BranchId = null;
            OnPropertyChanged(nameof(SelectedBranchName));
        }

        public async Task SearchCompanyChangedAsync(int? companyId)
        {
            SearchBranchOptions = new ObservableCollection<BranchOption>(await _svc.GetBranchOptionsAsync(companyId));
            SearchBranchId = null;
        }
    }
}
