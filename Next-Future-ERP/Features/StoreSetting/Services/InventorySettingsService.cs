// InventorySettingsEfService.cs
using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.StoreSetting.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.StoreSetting.Services
{
    public class InventorySettingsService : IInventorySettingsService
    {
        // key = $"{CompanyId}:{BranchId}"
        private static readonly ConcurrentDictionary<string, InventorySetting> _store = new();

        public Task<InventorySetting?> LoadAsync(int companyId, int branchId)
        {
            var key = $"{companyId}:{branchId}";
            _store.TryGetValue(key, out var value);
            return Task.FromResult(value);
        }

        public Task<InventorySetting> SaveAsync(InventorySetting settings)
        {
            var key = $"{settings.CompanyId}:{settings.BranchId}";
            if (!_store.ContainsKey(key))
            {
                settings.InventorySettingId = _store.Count + 1;
            }
            _store[key] = settings;
            return Task.FromResult(settings);
        }

        public Task<InventorySetting> ResetDefaultsAsync(int companyId, int branchId)
        {
            var defaults = new InventorySetting
            {
                InventorySettingId = 0,
                CompanyId = companyId,
                BranchId = branchId,
                ValuationMethod = Models.ValuationMethod.AverageCost,
                AllowNegativeStock = false,
                PricePolicy = "Retail",
                EnableExpiryTracking = true,
                EnableSerialTracking = false,
                DefaultWarehouse = "",
                StockAlertThreshold = 10,
                UnitConversion = true,
                BatchManagement = true,
                AutoStockAdjustment = true,
                ItemNumberLength = 8,
                HowDisplayDate = Models.DateDisplayMode.Gregorian,

                InboundOrderSequence = Models.SequencePolicy.Editable,
                TransferOrderSequence = Models.SequencePolicy.Locked,
                CountInvortySequence = 1,
                OutboundOrderSequence = Models.SequencePolicy.ManualPerDoc,

                MultiCostCenterInbound = 0,
                MultiCostCenterOutbound = 0,
                MultiCostCenterTransfer = 0,

                MultiInventoryUser = true,
                UsingACompositeItem = false,
                DisplayingItemPackage = true,
                ItemBarcodesIsBatchNumbers = false,
                ItemBarcodesIsSerialNumber = false,
                DuplicateItemsInInventory = false,

                AllowingUnitModInVouchers = true,
                ReferenceNumberMandatory = false,
                StatementEntryMandatory = false,
                AllowingTranscetiontOfFinishedGoods = true,

                UsingAWeightSystem = false,
                UsingAColorSystem = false,
                AllowingModifiyQuantityInTransfers = true,
                IncludingTheGroup = true,
                UsingExpensesInReceiving = true,
                UsingItemComponents = false,
                AutomaticCreatingTheItemNumber = true,

                NumberOfColumnsInBatchNumbers = false
            };

            var key = $"{companyId}:{branchId}";
            _store[key] = defaults;
            return Task.FromResult(defaults);
        }
    }
}
