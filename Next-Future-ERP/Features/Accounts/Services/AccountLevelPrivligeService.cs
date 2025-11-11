using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.Accounts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public class AccountLevelPrivligeService
    {
        private readonly AppDbContext _db;
        public AccountLevelPrivligeService(AppDbContext db)
        {
            _db = DbContextFactory.Create();
        }
        public async Task<IReadOnlyList<AccountLevelPrivlige>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.AccountLevelPrivliges
                            .AsNoTracking()
                            .OrderBy(x => x.LevelId)
                            .ThenBy(x => x.AccountPrivligeAname)
                            .ToListAsync(ct);
        }
    }

}
