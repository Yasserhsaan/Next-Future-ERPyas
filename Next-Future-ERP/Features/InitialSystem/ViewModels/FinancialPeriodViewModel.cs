using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.InitialSystem.Interface;
using Next_Future_ERP.Features.InitialSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.InitialSystem.ViewModels
{
    public partial class FinancialPeriodViewModel : ObservableValidator, IWizardStepValidation
    {
        private readonly WizardState _wizardState;
        [ObservableProperty]
        private bool allowFutureDateEntry;

        [ObservableProperty]
        private bool autoPeriodRollover;

        [ObservableProperty]
        [Required(ErrorMessage = "الرجاء اختيار سياسة الإغلاق")]
        private string selectedPolicy = "شهري";

        [ObservableProperty]
        private int fiscalYear = DateTime.Now.Year;

        [ObservableProperty]
        private int selectedStartMonth = 1;


        public ObservableCollection<int> AvailableMonths { get; } = new(Enumerable.Range(1, 12)
       .Select(i =>
       {

           return i;
       }).ToList());

        [Required(ErrorMessage = "الرجاء توليد الفترات")]
        public ObservableCollection<PeriodModel> Periods { get; } = new();

        public ObservableCollection<string> Policies { get; } = new()
            {
                "شهري", // Monthly
                "ربعي", // Quarterly
                "نصفي", // Semiannual
                "سنوي"  // Yearly
            };

        [RelayCommand]
        private void GeneratePeriods()
        {
            Periods.Clear();

            try
            {
                // Fix: Convert SelectedStartMonth (int) to a valid month name string
                string startMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(SelectedStartMonth);
                int startMonthIndex = DateTime.ParseExact(startMonthName, "MMMM", CultureInfo.CurrentCulture).Month;

                DateTime current = new(FiscalYear, startMonthIndex, 1);
                int periodNumber = 1;

                while (periodNumber <= 12)
                {
                    DateTime start = current;
                    DateTime end;

                    switch (SelectedPolicy)
                    {
                        case "شهري": // Monthly
                            end = start.AddMonths(1).AddDays(-1);
                            break;

                        case "ربعي": // Quarterly
                            end = start.AddMonths(3).AddDays(-1);
                            break;

                        case "نصفي": // Semiannual
                            end = start.AddMonths(6).AddDays(-1);
                            break;

                        case "سنوي": // Yearly
                            end = start.AddYears(1).AddDays(-1);
                            break;

                        default:
                            end = start.AddMonths(1).AddDays(-1); // Fallback to monthly
                            break;
                    }

                    Periods.Add(new PeriodModel
                    {
                        PeriodId = periodNumber,
                        StartDateOfYear = start,
                        EndDateOfYear = end
                    });

                    if (SelectedPolicy == "سنوي" || end.Year > FiscalYear)
                        break;

                    current = end.AddDays(1);
                    periodNumber++;
                }
            }
            catch (Exception ex)
            {
                // Handle parsing errors or invalid input
                MessageBox.Show("حدث خطأ أثناء إنشاء الفترات: " + ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public FinancialPeriodViewModel(WizardState wizardState)
        {
            _wizardState = wizardState;

            if (_wizardState.FinancialPeriods != null)
            {
                AllowFutureDateEntry = _wizardState.FinancialPeriods.AllowFutureDateEntry;
                AutoPeriodRollover = _wizardState.FinancialPeriods.AutoPeriodRollover;
                SelectedPolicy = GetSelectedPeriodClosePolicy(_wizardState.FinancialPeriods.PeriodClosePolicy) ;
                FiscalYear = _wizardState.AccountingSetup.FiscalYear;
                SelectedStartMonth = _wizardState.AccountingSetup.CurrentMonth;

                if (_wizardState.FinancialPeriods?.GeneratedPeriods != null)
                {
                    foreach (PeriodModel item in _wizardState.FinancialPeriods.GeneratedPeriods)
                    {
                        Periods.Add(new PeriodModel
                        {
                            PeriodId = item.PeriodId,
                            StartDateOfYear = item.StartDateOfYear,
                            EndDateOfYear = item.EndDateOfYear
                        });
                    }
                }

            }
            else
            {
                _wizardState.FinancialPeriods = new FinancialPeriodsSettingModlel();
            }
        }


        public bool ValidateStep(out string? errorMessage)
        {
            ValidateAllProperties();

            var errors = new[]
            {

                GetErrors(nameof(SelectedPolicy)).Cast<ValidationResult>().FirstOrDefault(),
                GetErrors(nameof(FiscalYear)).Cast<ValidationResult>().FirstOrDefault(),
                GetErrors(nameof(SelectedStartMonth)).Cast<ValidationResult>().FirstOrDefault(),
                GetErrors(nameof(Periods)).Cast<ValidationResult>().FirstOrDefault(),





            }
            .Where(e => e != null)
            .Select(e => e!.ErrorMessage)
            .ToList();

            if (errors.Any())
            {
                errorMessage = errors.First();
                return false;
            }


            // Update the wizard state with the current values

            _wizardState.FinancialPeriods.AllowFutureDateEntry = AllowFutureDateEntry;
            _wizardState.FinancialPeriods.AutoPeriodRollover = AutoPeriodRollover;
            _wizardState.FinancialPeriods.PeriodClosePolicy = GetSelectedPeriodClosePolicy();
           
           // _wizardState.FinancialPeriods.StartMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(SelectedStartMonth);
            _wizardState.FinancialPeriods.GeneratedPeriods = Periods.ToList();


            errorMessage = null;
            return true;
        }

        public int GetSelectedPeriodClosePolicy()
        {
            return SelectedPolicy switch
            {
                "شهري" => 1, // Monthly
                "ربعي" => 2, // Quarterly
                "نصفي" => 3, // Semiannual
                "سنوي" => 4, // Yearly
                _ => 1 // Default to Monthly if not recognized
            };


        }

        public String GetSelectedPeriodClosePolicy(int selectedPolicy)
        {
            return selectedPolicy switch
            {
                1 => "شهري", // Monthly
                2 => "ربعي", // Quarterly
                3 => "نصفي", // Semiannual
                4 => "سنوي", // Yearly
                _ => "شهري" // Default to Monthly if not recognized
            };
           
           


        }
    }
}
