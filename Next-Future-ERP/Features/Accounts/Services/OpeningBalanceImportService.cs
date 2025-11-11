using OfficeOpenXml;
using Next_Future_ERP.Features.Accounts.Models;
using Next_Future_ERP.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public class OpeningBalanceImportService
    {
        private readonly IReferenceDataService _referenceDataService;

        public OpeningBalanceImportService(IReferenceDataService referenceDataService)
        {
            _referenceDataService = referenceDataService;
        }

        public async Task<ImportResult> ImportFromExcelAsync(string filePath, int companyId, int branchId)
        {
            var result = new ImportResult();

            try
            {
                // Set EPPlus license context
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Load reference data
                var accounts = await _referenceDataService.GetLeafAccountsAsync(companyId, branchId);
                var currencies = await _referenceDataService.GetCurrenciesAsync(companyId);
                var costCenters = await _referenceDataService.GetCostCentersAsync(companyId, branchId);

                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets[0]; // First worksheet

                var rowCount = worksheet.Dimension?.Rows ?? 0;

                for (int row = 2; row <= rowCount; row++) // Start from row 2 (skip header)
        {
            try
            {
                        var line = await ParseRowToOpeningBalanceLine(worksheet, row, accounts, currencies, costCenters, companyId, branchId);
                        if (line != null)
                        {
                            result.Lines.Add(line);
                        }
            }
            catch (Exception ex)
            {
                        result.Errors.Add($"خطأ في السطر {row}: {ex.Message}");
                    }
                }

                result.IsSuccess = result.Lines.Any() && !result.Errors.Any();
            }
            catch (Exception ex)
            {
                result.Errors.Add($"خطأ في قراءة الملف: {ex.Message}");
                result.IsSuccess = false;
            }

                    return result;
                }

        private async Task<OpeningBalanceLine?> ParseRowToOpeningBalanceLine(
            ExcelWorksheet worksheet, 
            int rowIndex,
            List<Account> accounts, 
            List<NextCurrency> currencies, 
            List<CostCenter> costCenters,
            int companyId, 
            int branchId)
        {
            // Column mapping (adjust based on your template structure)
            var accountCode = worksheet.Cells[rowIndex, 1].Text.Trim();
            var accountName = worksheet.Cells[rowIndex, 2].Text.Trim();
            var currencyCode = worksheet.Cells[rowIndex, 3].Text.Trim();
            var transactionDebitStr = worksheet.Cells[rowIndex, 4].Text.Trim();
            var transactionCreditStr = worksheet.Cells[rowIndex, 5].Text.Trim();
            var exchangeRateStr = worksheet.Cells[rowIndex, 6].Text.Trim();
            var costCenterName = worksheet.Cells[rowIndex, 7].Text.Trim();
            var statement = worksheet.Cells[rowIndex, 8].Text.Trim();

            // Skip empty rows
            if (string.IsNullOrEmpty(accountCode) && string.IsNullOrEmpty(accountName))
                return null;

            // Find account
            var account = accounts.FirstOrDefault(a => a.AccountCode == accountCode || a.AccountNameAr == accountName);
                    if (account == null)
                    {
                throw new Exception($"لم يتم العثور على الحساب: {accountCode} - {accountName}");
            }

            // Find currency
            var currency = currencies.FirstOrDefault(c => c.CurrencySymbol == currencyCode || c.CurrencyNameAr == currencyCode);
            if (currency == null)
            {
                // Default to company currency
                currency = currencies.FirstOrDefault(c => c.IsCompanyCurrency == true);
                    if (currency == null)
                    {
                    throw new Exception($"لم يتم العثور على العملة: {currencyCode}");
                }
            }

            // Parse amounts
            if (!decimal.TryParse(transactionDebitStr, out var transactionDebit))
                transactionDebit = 0;

            if (!decimal.TryParse(transactionCreditStr, out var transactionCredit))
                transactionCredit = 0;

            if (!decimal.TryParse(exchangeRateStr, out var exchangeRate))
                exchangeRate = 1;

            // Find cost center if specified
            CostCenter? costCenter = null;
            if (!string.IsNullOrEmpty(costCenterName))
            {
                costCenter = costCenters.FirstOrDefault(cc => cc.CostCenterName == costCenterName);
            }

            // Create opening balance line
                    var line = new OpeningBalanceLine
                    {
                AccountId = account.AccountId,
                AccountCode = account.AccountCode,
                AccountNameAr = account.AccountNameAr,
                CompanyCurrencyId = currencies.First(c => c.IsCompanyCurrency == true).CurrencyId,
                TransactionCurrencyId = currency.CurrencyId,
                TransactionCurrencyName = currency.CurrencyNameAr,
                TransactionDebit = transactionDebit,
                TransactionCredit = transactionCredit,
                ExchangeRate = exchangeRate,
                CompanyDebit = transactionDebit * exchangeRate,
                CompanyCredit = transactionCredit * exchangeRate,
                CostCenterId = costCenter?.CostCenterId,
                CostCenterName = costCenter?.CostCenterName,
                Statement = statement,
                UsesCostCenter = account.UsesCostCenter ?? false
            };

            return line;
        }

        public void CreateTemplate(string filePath, List<Account> accounts, List<NextCurrency> currencies, List<CostCenter> costCenters)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("نموذج الأرصدة الافتتاحية");

            // Create headers
            worksheet.Cells[1, 1].Value = "رمز الحساب";
            worksheet.Cells[1, 2].Value = "اسم الحساب";
            worksheet.Cells[1, 3].Value = "رمز العملة";
            worksheet.Cells[1, 4].Value = "مدين (عملة المعاملة)";
            worksheet.Cells[1, 5].Value = "دائن (عملة المعاملة)";
            worksheet.Cells[1, 6].Value = "سعر الصرف";
            worksheet.Cells[1, 7].Value = "مركز التكلفة";
            worksheet.Cells[1, 8].Value = "البيان";

            // Style headers
            var headerRange = worksheet.Cells[1, 1, 1, 8];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            headerRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            headerRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            headerRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            headerRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            // Add sample data
            int row = 2;
            foreach (var account in accounts.Take(10)) // Add first 10 accounts as examples
            {
                worksheet.Cells[row, 1].Value = account.AccountCode;
                worksheet.Cells[row, 2].Value = account.AccountNameAr;
                worksheet.Cells[row, 3].Value = currencies.FirstOrDefault(c => c.IsCompanyCurrency == true)?.CurrencySymbol ?? "RS";
                worksheet.Cells[row, 4].Value = 0; // Transaction Debit
                worksheet.Cells[row, 5].Value = 0; // Transaction Credit
                worksheet.Cells[row, 6].Value = 1; // Exchange Rate
                worksheet.Cells[row, 7].Value = ""; // Cost Center
                worksheet.Cells[row, 8].Value = "رصيد افتتاحي"; // Statement
                row++;
            }

            // Create accounts sheet for reference
            var accountsSheet = package.Workbook.Worksheets.Add("دليل الحسابات");
            accountsSheet.Cells[1, 1].Value = "رمز الحساب";
            accountsSheet.Cells[1, 2].Value = "اسم الحساب";
            accountsSheet.Cells[1, 3].Value = "يستخدم مركز تكلفة";
            
            var accountsHeaderRange = accountsSheet.Cells[1, 1, 1, 3];
            accountsHeaderRange.Style.Font.Bold = true;
            accountsHeaderRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            accountsHeaderRange.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);

            row = 2;
            foreach (var account in accounts)
            {
                accountsSheet.Cells[row, 1].Value = account.AccountCode;
                accountsSheet.Cells[row, 2].Value = account.AccountNameAr;
                accountsSheet.Cells[row, 3].Value = account.UsesCostCenter == true ? "نعم" : "لا";
                row++;
            }

            // Create currencies sheet for reference
            var currenciesSheet = package.Workbook.Worksheets.Add("العملات");
            currenciesSheet.Cells[1, 1].Value = "رمز العملة";
            currenciesSheet.Cells[1, 2].Value = "اسم العملة";
            currenciesSheet.Cells[1, 3].Value = "عملة الشركة";
            
            var currenciesHeaderRange = currenciesSheet.Cells[1, 1, 1, 3];
            currenciesHeaderRange.Style.Font.Bold = true;
            currenciesHeaderRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            currenciesHeaderRange.Style.Fill.BackgroundColor.SetColor(Color.LightYellow);

            row = 2;
            foreach (var currency in currencies)
            {
                currenciesSheet.Cells[row, 1].Value = currency.CurrencySymbol;
                currenciesSheet.Cells[row, 2].Value = currency.CurrencyNameAr;
                currenciesSheet.Cells[row, 3].Value = currency.IsCompanyCurrency == true ? "نعم" : "لا";
                row++;
            }

            // Create cost centers sheet for reference
            if (costCenters.Any())
            {
                var costCentersSheet = package.Workbook.Worksheets.Add("مراكز التكلفة");
                costCentersSheet.Cells[1, 1].Value = "اسم مركز التكلفة";
                costCentersSheet.Cells[1, 2].Value = "التصنيف";
                
                var costCentersHeaderRange = costCentersSheet.Cells[1, 1, 1, 2];
                costCentersHeaderRange.Style.Font.Bold = true;
                costCentersHeaderRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                costCentersHeaderRange.Style.Fill.BackgroundColor.SetColor(Color.LightCoral);

                row = 2;
                foreach (var costCenter in costCenters)
                {
                    costCentersSheet.Cells[row, 1].Value = costCenter.CostCenterName;
                    costCentersSheet.Cells[row, 2].Value = costCenter.Classification;
                    row++;
                }
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();
            accountsSheet.Cells.AutoFitColumns();
            currenciesSheet.Cells.AutoFitColumns();

            // Save the file
            package.SaveAs(new FileInfo(filePath));
        }
    }

    public class ImportResult
    {
        public bool IsSuccess { get; set; }
        public List<OpeningBalanceLine> Lines { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}