using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Items.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Items.Services
{
    public class InventoryBalanceService : IInventoryBalanceService
    {
        private readonly AppDbContext _context;

        public InventoryBalanceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryBalance>> GetByItemAsync(int itemId)
        {
            return await _context.InventoryBalances
                .Include(b => b.Warehouse)
                .Include(b => b.Batch)
                .Include(b => b.Unit)
                .Where(b => b.ItemID == itemId)
                .ToListAsync();
        }

        public async Task<List<InventoryBalance>> GetByWarehouseAsync(int warehouseId)
        {
            return await _context.InventoryBalances
                .Include(b => b.Item)
                .Include(b => b.Batch)
                .Include(b => b.Unit)
                .Where(b => b.WarehouseID == warehouseId)
                .ToListAsync();
        }

        public async Task<InventoryBalance> GetByItemAndWarehouseAsync(int itemId, int warehouseId)
        {
            return await _context.InventoryBalances
                .Include(b => b.Warehouse)
                .Include(b => b.Batch)
                .Include(b => b.Unit)
                .FirstOrDefaultAsync(b => b.ItemID == itemId && b.WarehouseID == warehouseId);
        }

        public async Task<InventoryBalance> GetByItemWarehouseAndBatchAsync(int itemId, int warehouseId, int? batchId)
        {
            return await _context.InventoryBalances
                .Include(b => b.Warehouse)
                .Include(b => b.Batch)
                .Include(b => b.Unit)
                .FirstOrDefaultAsync(b => b.ItemID == itemId && b.WarehouseID == warehouseId && b.BatchID == batchId);
        }

        public async Task<InventoryBalance> AddAsync(InventoryBalance balance)
        {
            balance.LastUpdate = DateTime.Now;
            _context.InventoryBalances.Add(balance);
            await _context.SaveChangesAsync();
            return balance;
        }

        public async Task<InventoryBalance> UpdateAsync(InventoryBalance balance)
        {
            balance.LastUpdate = DateTime.Now;
            _context.InventoryBalances.Update(balance);
            await _context.SaveChangesAsync();
            return balance;
        }

        public async Task DeleteAsync(long balanceId)
        {
            var balance = await _context.InventoryBalances.FindAsync(balanceId);
            if (balance != null)
            {
                _context.InventoryBalances.Remove(balance);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<InventoryBalance>> GetAllAsync()
        {
            return await _context.InventoryBalances
                .Include(b => b.Item)
                .Include(b => b.Warehouse)
                .Include(b => b.Batch)
                .Include(b => b.Unit)
                .ToListAsync();
        }
    }
}
