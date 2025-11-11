using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public class GeneralJournalEntryViewModel : ObservableObject, IDisposable
    {
        private readonly GeneralJournalService _service;
        private bool _disposed = false;

        private GeneralJournalEntry _journalEntry = new();
        public GeneralJournalEntry JournalEntry
        {
            get => _journalEntry;
            set => SetProperty(ref _journalEntry, value);
        }
        public ObservableCollection<DocumentType> DocumentTypes { get; } = new();
        public ObservableCollection<GeneralJournalEntryDetailViewModel> Details { get; } = new();

        private GeneralJournalEntryDetailViewModel _selectedDetail;
        public GeneralJournalEntryDetailViewModel SelectedDetail
        {
            get => _selectedDetail;
            set => SetProperty(ref _selectedDetail, value);
        }



        // Properties للتحكم في عرض الأقسام
        private bool _showHeader = true;
        public bool ShowHeader
        {
            get => _showHeader;
            set => SetProperty(ref _showHeader, value);
        }
        public string TotalDebitDisplay => JournalEntry.TotalDebit.ToString("N2");
        public string TotalCreditDisplay => JournalEntry.TotalCredit.ToString("N2");

        public decimal RawDifference => Math.Abs(JournalEntry.TotalDebit - JournalEntry.TotalCredit);
        public string Difference => RawDifference.ToString("N2");


        private bool _showDetails = false;
        public bool ShowDetails
        {
            get => _showDetails;
            set => SetProperty(ref _showDetails, value);
        }

        private bool _showSummary = false;
        public bool ShowSummary
        {
            get => _showSummary;
            set => SetProperty(ref _showSummary, value);
        }

        // Commands
        public ICommand SaveCommand { get; }
        public ICommand AddDetailCommand { get; }
        public ICommand RemoveDetailCommand { get; }
        public ICommand ShowHeaderTabCommand { get; }
        public ICommand ShowDetailsTabCommand { get; }
        public ICommand ShowSummaryTabCommand { get; }

        public GeneralJournalEntryViewModel()
        {
            _service = new GeneralJournalService();
            SaveCommand = new RelayCommand(Save);
            RemoveDetailCommand = new RelayCommand(RemoveDetail);
            ShowHeaderTabCommand = new RelayCommand(ShowHeaderTab);
            ShowDetailsTabCommand = new RelayCommand(ShowDetailsTab);
            ShowSummaryTabCommand = new RelayCommand(ShowSummaryTab);

            InitializeNewEntry();
          //  LoadCurrencies();
            LoadDocumentTypes(); // إضافة هذا السطر
        }
        private async void LoadDocumentTypes()
        {
            try
            {
                using (var service = new GeneralJournalService())
                {
                    var documentTypes = await service.GetDocumentTypesAsync();
                    DocumentTypes.Clear();
                    foreach (var docType in documentTypes)
                    {
                        DocumentTypes.Add(docType);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ في تحميل أنواع المستندات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void InitializeNewEntry()
        {
            JournalEntry = new GeneralJournalEntry
            {
                PostingDate = DateTime.Today,
                DocumentNumber = GenerateDocumentNumber(),
                Status = 1,
                CreatedBy = 1,
                CreatedAt = DateTime.Now
            };

            CalculateTotals(); // ⬅️ ضروري لحساب الفرق في البداية
        }

        // إضافة هذه الخاصية إلى الكلاس
        public List<KeyValuePair<byte, string>> StatusOptions { get; } = new()
{
    new KeyValuePair<byte, string>(1, "مسودة"),
    new KeyValuePair<byte, string>(2, "مُرحّل"),
    new KeyValuePair<byte, string>(3, "ملغى")
};

        private string GenerateDocumentNumber()
        {
            return $"JV-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }
        // إضافة هذه الخاصية إلى الـ ViewModel
        // إضافة هذه الخصائص إلى الكلاس



      
        private void RemoveDetail()
        {
            if (SelectedDetail != null && SelectedDetail.AccountNumber != "الإجمالي")
            {
                Details.Remove(SelectedDetail);
                CalculateTotals(); // سيُحدث صف الإجمالي تلقائيًا
            }
        }



       
        private void Detail_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // إعادة حساب الإجماليات عند تغيير أي قيمة
            if (e.PropertyName == nameof(GeneralJournalEntryDetailViewModel.DebitAmount) ||
                e.PropertyName == nameof(GeneralJournalEntryDetailViewModel.CreditAmount))
            {
                CalculateTotals();
            }
        }
        public Brush DifferenceBackground
        {
            get
            {
                var difference = Math.Abs(JournalEntry.TotalDebit - JournalEntry.TotalCredit);
                return difference > 0.0001m
                    ? new SolidColorBrush(Colors.LightCoral) // أو أي لون أحمر فاتح تفضله
                    : new SolidColorBrush(Colors.LightGreen); // أو أي لون أخضر فاتح تفضله
            }
        }
        public string TotalRowLabel => "الإجمالي";
        public string TotalDebitFooter => JournalEntry.TotalDebit.ToString("N2");
        public string TotalCreditFooter => JournalEntry.TotalCredit.ToString("N2");
        private void UpdateFooterRow()
        {
            // إزالة السابق إن وجد
            var existing = Details.FirstOrDefault(d => d.AccountNumber == "الإجمالي");
            if (existing != null)
                Details.Remove(existing);

            // لا تضف الإجمالي إذا لا يوجد أي تفاصيل فعلية
            if (!Details.Any(d => d.AccountNumber != "الإجمالي"))
                return;

            var footer = new GeneralJournalEntryDetailViewModel
            {
                AccountNumber = "الإجمالي",
                DebitAmount = JournalEntry.TotalDebit,
                CreditAmount = JournalEntry.TotalCredit,
                Statement = "",
                ExchangeRate = 1,
                CurrencyId = 0
            };

            Details.Add(footer);
        }


        private void CalculateTotals()
        {
            var totalDebit = 0m;
            var totalCredit = 0m;

            foreach (var detail in Details.Where(d => d.AccountNumber != "الإجمالي"))
            {
                totalDebit += detail.DebitAmount ?? 0;
                totalCredit += detail.CreditAmount ?? 0;
            }

            JournalEntry.TotalDebit = totalDebit;
            JournalEntry.TotalCredit = totalCredit;

            OnPropertyChanged(nameof(Difference));
            OnPropertyChanged(nameof(DifferenceBackground));
            OnPropertyChanged(nameof(TotalDebitDisplay));
            OnPropertyChanged(nameof(TotalCreditDisplay));

            UpdateFooterRow(); // تحديث صف الإجمالي بعد الحساب
        }



        private bool ValidateEntry()
        {
            if (string.IsNullOrWhiteSpace(JournalEntry.DocumentNumber))
            {
                MessageBox.Show("رقم المستند مطلوب", "حقل إجباري", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (JournalEntry.DocumentTypeId <= 0)
            {
                MessageBox.Show("نوع المستند مطلوب", "حقل إجباري", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(JournalEntry.Description))
            {
                MessageBox.Show("الوصف مطلوب", "حقل إجباري", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Details.Count == 0)
            {
                MessageBox.Show("يجب إضافة تفاصيل القيد", "حقل إجباري", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // التحقق من أن كل تفصيل له حساب وقيمة
foreach (var detail in Details.Where(d => d.AccountNumber != "الإجمالي"))
            {
                if (string.IsNullOrWhiteSpace(detail.AccountNumber))
                {
                    MessageBox.Show("رقم الحساب مطلوب في جميع التفاصيل", "حقل إجباري", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (!detail.DebitAmount.HasValue && !detail.CreditAmount.HasValue)
                {
                    MessageBox.Show("يجب إدخال قيمة مدين أو دائن في جميع التفاصيل", "حقل إجباري", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (detail.DebitAmount.HasValue && detail.CreditAmount.HasValue &&
                    detail.DebitAmount.Value > 0 && detail.CreditAmount.Value > 0)
                {
                    MessageBox.Show("لا يمكن إدخال قيمة مدين ودائن في نفس التفصيل", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            CalculateTotals();
            if (Math.Abs(JournalEntry.TotalDebit - JournalEntry.TotalCredit) > 0.0001m)
            {
                MessageBox.Show($"إجمالي المدين ({JournalEntry.TotalDebit:N2}) يجب أن يساوي إجمالي الدائن ({JournalEntry.TotalCredit:N2})",
                               "خطأ في التوازن", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
      

       
        // Methods للتحكم في عرض الأقسام
        public void ShowHeaderTab()
        {
            ShowHeader = true;
            ShowDetails = false;
            ShowSummary = false;
        }

        public void ShowDetailsTab()
        {
            ShowHeader = false;
            ShowDetails = true;
            ShowSummary = false;
        }

        public void ShowSummaryTab()
        {
            ShowHeader = false;
            ShowDetails = false;
            ShowSummary = true;
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
                _service?.Dispose();
                _disposed = true;
            }
        }
        // في ViewModel
        // في ViewModel
       

        private async void Save()
        {
            try
            {
                if (!ValidateEntry())
                    return;
                JournalEntry.Details = Details
      .Where(d => d.AccountNumber != "الإجمالي") // 🔴 استثناء الإجمالي
      .Select(d => d.GetModel())
      .ToList();

                //JournalEntry.Details = Details.Select(d => d.GetModel()).ToList();

                using (var service = new GeneralJournalService())
                {
                    await service.SaveAsync(JournalEntry);
                    MessageBox.Show("تم حفظ القيد بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        ~GeneralJournalEntryViewModel()
        {
            Dispose(false);
        }
    }
}