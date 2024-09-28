using Microsoft.EntityFrameworkCore;
using StudySpace.Data.Base;
using StudySpace.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Data.Repository
{

    public class PackageRepository : GenericRepository<Package>
    {
        public PackageRepository() { }
        public PackageRepository(EXE201_StudySpaceContext context)
        {
            _context = context;
        }

        public async Task<List<Package>> GetAllPackagesAsync()
        {
            return await _context.Packages
                .Include(r => r.Transactions)
                .Include(r => r.StorePackages)
                .ToListAsync();
        }
    }
}
