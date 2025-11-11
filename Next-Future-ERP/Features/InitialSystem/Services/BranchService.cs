using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.InitialSystem.Models;
using System;
using System.Threading.Tasks;

namespace Next_Future_ERP.Data.Services
{
    public class BranchService
    {
        private readonly AppDbContext _context;

        public BranchService()
        {
            _context = DbContextFactory.Create();
        }

        public async Task SaveBranchAsync(BranchModel branch)
        {
            if (string.IsNullOrWhiteSpace(branch.BranchName))
                throw new ArgumentException("اسم الفرع لا يمكن أن يكون فارغاً");

            // Check if branch already exists
            var existing = await _context.Branches.FirstOrDefaultAsync();
            var existingCompany = await _context.CompanyInfo.FirstOrDefaultAsync();
            if (existing != null)
            {

                // Update existing
                existing.BranchName = branch.BranchName;
                existing.Location = branch.Location;
                existing.ComiId = existingCompany!.CompId;
            }
            else
            {
                branch.IsActive = branch.IsActive ?? true;
                branch.ComiId = existingCompany!.CompId;
                _context.Branches.Add(branch);

                await _context.SaveChangesAsync();
            }
               
        }
    }
} 