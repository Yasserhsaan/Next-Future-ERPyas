// PosSetting.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Models
{
    // PosSetting.cs
    public class PosSetting
    {
        public int PosSettingId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int BranchId { get; set; }

        public bool? EnableEInvoice { get; set; } = false;

        public bool? QrCodeOnInvoice { get; set; } = false;

        public string? EInvoiceFormat { get; set; }

        public string? VatRegistrationNumber { get; set; }

        public bool? ShowVatBreakdown { get; set; } = false;

        public decimal? MaxDiscountLimit { get; set; }

        public string? DefaultPosUser { get; set; }

        public bool? PrintInvoiceAuto { get; set; } = false;

        public string? PricingLevel { get; set; }

        public bool? SearchForItems { get; set; } = true;

        public int? Itempricing { get; set; }

        public int? Rounduptheinvoiceamount { get; set; }

        public int? Paymentmethods { get; set; }

        public bool? CheckTheAvailableQuantity { get; set; }

        public bool? AllowPriceModification { get; set; }

        public bool? AllowTheUseOfCurrencies { get; set; }

        public int? ReturnPeriod { get; set; }

        public int? ExchangePeriod { get; set; }

        public string? InvoiceSequenceType { get; set; }

        public int? DelayOrAdvanceTime { get; set; }

        public int? NumberOfWorkingHours { get; set; }

        public TimeSpan? FromPeriod { get; set; }

        public TimeSpan? ToPeriod { get; set; }

        public int? NumberOfSerialNumberDigits { get; set; }

        public bool? GroupDuplicateItems { get; set; }

        public bool? UseTheDELETEKey { get; set; }

        public bool? AllowUnitModification { get; set; }

        public bool? SaleOnlyByBarcode { get; set; }

        public bool? PrintTheInvoiceAfterSaving { get; set; }

        public string? ReturnAndExchangeFundCalculation { get; set; }

        public string? ExchangeAndReturnCalculation { get; set; }

        public string? CouponCalculation { get; set; }

        public string? SalesLiquidationMethod { get; set; }

        public bool? ActivateCreditForReturns { get; set; }

        public bool? RequireMobileNumberForPayments { get; set; }

        public string? CurrentWalletBrokerAccount { get; set; }

        public string? SurplusAccountNumber { get; set; }

        public string? DeficitAccountNumber { get; set; }

        public bool? UseOfSevenPointPaymentCards { get; set; }

        public bool? ActivateVAT { get; set; }

        public bool? AddCashCustomerDataFromTheInvoiceScreen { get; set; }
    }
}