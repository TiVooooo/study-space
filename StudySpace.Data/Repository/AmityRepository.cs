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

    public class AmityRepository : GenericRepository<Amity>
    {
        public AmityRepository() { }
        public AmityRepository(EXE201_StudySpaceContext context)
        {
            _context = context;
        }

        public async Task<List<Amity>> GetAllAmitiesByStoreId(int storeId)
        {
            return await _context.Amities
                .Where(ra => ra.StoreId == storeId)
                .Select(ra => new Amity
                {
                    Id = ra.Id,
                    Name = ra.Name,
                    Type = ra.Type,
                    Status = ra.Status,
                    Quantity = ra.Quantity,
                    Description = ra.Description,
                })
                .ToListAsync();
        }

    }
}
