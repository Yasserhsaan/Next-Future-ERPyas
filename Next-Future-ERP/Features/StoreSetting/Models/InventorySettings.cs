
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.StoreSetting.Models
{
    public class InventorySetting
    {
        public int InventorySettingId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }

        public ValuationMethod? ValuationMethod { get; set; }
        public bool AllowNegativeStock { get; set; }
        public string? PricePolicy { get; set; }

        public bool EnableExpiryTracking { get; set; }
        public bool EnableSerialTracking { get; set; }
        public string? DefaultWarehouse { get; set; }
        public int? StockAlertThreshold { get; set; }
        public bool UnitConversion { get; set; }
        public bool BatchManagement { get; set; }
        public bool AutoStockAdjustment { get; set; }
        public int? ItemNumberLength { get; set; }
        public DateDisplayMode? HowDisplayDate { get; set; }

        public SequencePolicy? InboundOrderSequence { get; set; }
        public SequencePolicy? TransferOrderSequence { get; set; }
        public int? CountInvortySequence { get; set; } // إبقاء الاسم كما في الجدول
        public SequencePolicy? OutboundOrderSequence { get; set; }

        public int? MultiCostCenterInbound { get; set; }
        public int? MultiCostCenterOutbound { get; set; }
        public int? MultiCostCenterTransfer { get; set; }

        public bool? MultiInventoryUser { get; set; }
        public bool? UsingACompositeItem { get; set; }
        public bool? DisplayingItemPackage { get; set; }
        public bool? ItemBarcodesIsBatchNumbers { get; set; }
        public bool? ItemBarcodesIsSerialNumber { get; set; }
        public bool? DuplicateItemsInInventory { get; set; }

        public bool? AllowingUnitModInVouchers { get; set; }
        public bool? ReferenceNumberMandatory { get; set; }
        public bool? StatementEntryMandatory { get; set; }
        public bool? AllowingTranscetiontOfFinishedGoods { get; set; }

        public bool? UsingAWeightSystem { get; set; }
        public bool? UsingAColorSystem { get; set; }
        public bool? AllowingModifiyQuantityInTransfers { get; set; }
        public bool? IncludingTheGroup { get; set; }
        public bool? UsingExpensesInReceiving { get; set; }
        public bool? UsingItemComponents { get; set; }
        public bool? AutomaticCreatingTheItemNumber { get; set; }

        // في الجدول كانت bit، لكن منطقيًا "عدد أعمدة" = رقم. سنحترم الجدول الآن ونبقيها bool?،
        // ويمكنك لاحقًا تعديل العمود إلى int إذا رغبت.
        public bool? NumberOfColumnsInBatchNumbers { get; set; }
    }
}
