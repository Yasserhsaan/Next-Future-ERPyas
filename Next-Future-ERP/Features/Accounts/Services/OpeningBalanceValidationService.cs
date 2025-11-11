using Next_Future_ERP.Features.Accounts.Models;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public class OpeningBalanceValidationService
    {
        private readonly IReferenceDataService _referenceDataService;

        public OpeningBalanceValidationService(IReferenceDataService referenceDataService)
        {
            _referenceDataService = referenceDataService;
        }

        /// <summary>
        /// التحقق من صحة سطر الأرصدة الافتتاحية
        /// </summary>
        public async Task<ValidationResult> ValidateLineAsync(OpeningBalanceLine line, int companyId, int branchId)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                // التحقق من وجود الحساب وأنه فرعي
                if (line.AccountId <= 0)
                {
                    result.Errors.Add("يجب اختيار حساب");
                    result.IsValid = false;
                }
                else
                {
                    var account = await _referenceDataService.GetAccountDetailsAsync(line.AccountId);
                    if (account == null)
                    {
                        result.Errors.Add("الحساب المحدد غير موجود");
                        result.IsValid = false;
                    }
                    else if (account.AccountType != 2)
                    {
                        result.Errors.Add("يجب اختيار حساب فرعي فقط");
                        result.IsValid = false;
                    }
                    else
                    {
                        // التحقق من مركز التكلفة
                        if (account.UsesCostCenter == true)
                        {
                            if (!line.CostCenterId.HasValue)
                            {
                                result.Errors.Add("مركز التكلفة مطلوب لهذا الحساب");
                                result.IsValid = false;
                            }
                        }
                        else
                        {
                            if (line.CostCenterId.HasValue)
                            {
                                result.Warnings.Add("هذا الحساب لا يستخدم مراكز التكلفة");
                            }
                        }
                    }
                }

                // التحقق من العملة
                if (line.TransactionCurrencyId <= 0)
                {
                    result.Errors.Add("يجب اختيار عملة المعاملة");
                    result.IsValid = false;
                }

                if (line.CompanyCurrencyId <= 0)
                {
                    result.Errors.Add("عملة الشركة غير محددة");
                    result.IsValid = false;
                }

                // التحقق من المبالغ (جانب واحد فقط)
                if (line.TransactionDebit > 0 && line.TransactionCredit > 0)
                {
                    result.Errors.Add("لا يمكن إدخال مدين ودائن معاً");
                    result.IsValid = false;
                }

                if (line.TransactionDebit == 0 && line.TransactionCredit == 0)
                {
                    result.Errors.Add("يجب إدخال مبلغ في المدين أو الدائن");
                    result.IsValid = false;
                }

                if (line.TransactionDebit < 0 || line.TransactionCredit < 0)
                {
                    result.Errors.Add("المبالغ يجب أن تكون موجبة");
                    result.IsValid = false;
                }

                // التحقق من سعر الصرف
                if (line.ExchangeRate <= 0)
                {
                    result.Errors.Add("سعر الصرف يجب أن يكون أكبر من صفر");
                    result.IsValid = false;
                }

                // التحقق من تطابق العملات
                if (line.TransactionCurrencyId == line.CompanyCurrencyId)
                {
                    if (Math.Abs(line.ExchangeRate - 1) > 0.000001m)
                    {
                        result.Warnings.Add("سعر الصرف يجب أن يكون 1 عند تطابق العملتين");
                    }
                }

                // التحقق من دقة الحسابات
                var expectedCompanyDebit = Math.Round(line.TransactionDebit * line.ExchangeRate, 4);
                var expectedCompanyCredit = Math.Round(line.TransactionCredit * line.ExchangeRate, 4);

                if (Math.Abs(line.CompanyDebit - expectedCompanyDebit) > 0.0001m)
                {
                    result.Warnings.Add($"مبلغ المدين بعملة الشركة غير دقيق. المتوقع: {expectedCompanyDebit:N4}");
                }

                if (Math.Abs(line.CompanyCredit - expectedCompanyCredit) > 0.0001m)
                {
                    result.Warnings.Add($"مبلغ الدائن بعملة الشركة غير دقيق. المتوقع: {expectedCompanyCredit:N4}");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"خطأ في التحقق من السطر: {ex.Message}");
                result.IsValid = false;
            }

            return result;
        }

        /// <summary>
        /// التحقق من توازن الدفعة
        /// </summary>
        public ValidationResult ValidateBatchBalance(List<OpeningBalanceLine> lines)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                if (!lines.Any())
                {
                    result.Errors.Add("لا توجد سطور في الدفعة");
                    result.IsValid = false;
                    return result;
                }

                // حساب إجمالي المدين والدائن بعملة الشركة
                var totalCompanyDebit = lines.Sum(l => l.CompanyDebit);
                var totalCompanyCredit = lines.Sum(l => l.CompanyCredit);
                var difference = totalCompanyDebit - totalCompanyCredit;

                // التحقق من التوازن (بسماحية خطأ تقريب صغيرة)
                if (Math.Abs(difference) > 0.01m)
                {
                    result.Errors.Add($"الدفعة غير متوازنة. الفرق: {difference:N4} (المدين: {totalCompanyDebit:N4}, الدائن: {totalCompanyCredit:N4})");
                    result.IsValid = false;
                }
                else if (Math.Abs(difference) > 0.001m)
                {
                    result.Warnings.Add($"توجد فروقات تقريب صغيرة: {difference:N6}");
                }

                // التحقق من عدم وجود سطور مكررة
                var duplicateAccounts = lines
                    .GroupBy(l => new { l.AccountId, l.CostCenterId, l.TransactionCurrencyId })
                    .Where(g => g.Count() > 1)
                    .ToList();

                if (duplicateAccounts.Any())
                {
                    foreach (var group in duplicateAccounts)
                    {
                        result.Warnings.Add($"يوجد أكثر من سطر للحساب نفسه ومركز التكلفة والعملة");
                    }
                }

                // التحقق من الحد الأدنى والأقصى للمبالغ
                const decimal maxAmount = 999999999999.9999m;
                var oversizedLines = lines.Where(l => l.CompanyDebit > maxAmount || l.CompanyCredit > maxAmount).ToList();
                
                if (oversizedLines.Any())
                {
                    result.Errors.Add("توجد مبالغ تتجاوز الحد الأقصى المسموح");
                    result.IsValid = false;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"خطأ في التحقق من توازن الدفعة: {ex.Message}");
                result.IsValid = false;
            }

            return result;
        }

        /// <summary>
        /// التحقق من صحة بيانات رأس الدفعة
        /// </summary>
        public ValidationResult ValidateBatchHeader(OpeningBalanceBatch batch)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                // التحقق من البيانات الأساسية
                if (batch.CompanyId <= 0)
                {
                    result.Errors.Add("يجب تحديد الشركة");
                    result.IsValid = false;
                }

                if (batch.BranchId <= 0)
                {
                    result.Errors.Add("يجب تحديد الفرع");
                    result.IsValid = false;
                }

                if (batch.FiscalYear <= 0)
                {
                    result.Errors.Add("يجب تحديد السنة المالية");
                    result.IsValid = false;
                }
                else
                {
                    var currentYear = DateTime.Now.Year;
                    if (batch.FiscalYear < currentYear - 5 || batch.FiscalYear > currentYear + 2)
                    {
                        result.Warnings.Add($"السنة المالية {batch.FiscalYear} قد تكون غير صحيحة");
                    }
                }

                if (string.IsNullOrWhiteSpace(batch.Description))
                {
                    result.Errors.Add("وصف الدفعة مطلوب");
                    result.IsValid = false;
                }
                else if (batch.Description.Length > 200)
                {
                    result.Errors.Add("وصف الدفعة طويل جداً (الحد الأقصى 200 حرف)");
                    result.IsValid = false;
                }

                // التحقق من التاريخ
                if (batch.DocDate == default)
                {
                    result.Errors.Add("تاريخ المستند مطلوب");
                    result.IsValid = false;
                }
                else
                {
                    var yearDiff = Math.Abs(batch.DocDate.Year - batch.FiscalYear);
                    if (yearDiff > 1)
                    {
                        result.Warnings.Add("تاريخ المستند لا يتطابق مع السنة المالية");
                    }

                    if (batch.DocDate > DateTime.Today.AddDays(1))
                    {
                        result.Warnings.Add("تاريخ المستند في المستقبل");
                    }
                }

                // التحقق من رقم المستند
                if (!string.IsNullOrWhiteSpace(batch.DocNo))
                {
                    if (batch.DocNo.Length > 30)
                    {
                        result.Errors.Add("رقم المستند طويل جداً (الحد الأقصى 30 حرف)");
                        result.IsValid = false;
                    }
                }

                // التحقق من الحالة
                if (batch.Status != 0 && batch.Status != 1)
                {
                    result.Errors.Add("حالة الدفعة غير صحيحة");
                    result.IsValid = false;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"خطأ في التحقق من رأس الدفعة: {ex.Message}");
                result.IsValid = false;
            }

            return result;
        }

        /// <summary>
        /// التحقق الشامل من الدفعة
        /// </summary>
        public async Task<ValidationResult> ValidateCompleteBatchAsync(OpeningBalanceBatch batch, List<OpeningBalanceLine> lines)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                // التحقق من رأس الدفعة
                var headerValidation = ValidateBatchHeader(batch);
                result.Errors.AddRange(headerValidation.Errors);
                result.Warnings.AddRange(headerValidation.Warnings);
                if (!headerValidation.IsValid) result.IsValid = false;

                // التحقق من كل سطر
                foreach (var line in lines)
                {
                    var lineValidation = await ValidateLineAsync(line, batch.CompanyId, batch.BranchId);
                    
                    // إضافة رقم السطر للأخطاء والتحذيرات
                    var lineNumber = lines.IndexOf(line) + 1;
                    result.Errors.AddRange(lineValidation.Errors.Select(e => $"السطر {lineNumber}: {e}"));
                    result.Warnings.AddRange(lineValidation.Warnings.Select(w => $"السطر {lineNumber}: {w}"));
                    
                    if (!lineValidation.IsValid) result.IsValid = false;
                }

                // التحقق من التوازن
                var balanceValidation = ValidateBatchBalance(lines);
                result.Errors.AddRange(balanceValidation.Errors);
                result.Warnings.AddRange(balanceValidation.Warnings);
                if (!balanceValidation.IsValid) result.IsValid = false;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"خطأ في التحقق الشامل: {ex.Message}");
                result.IsValid = false;
            }

            return result;
        }
    }
}
