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
    public class StoreRepository : GenericRepository<Store>
    {
        public StoreRepository() { }
        public StoreRepository(EXE201_StudySpaceContext context)
        {
            _context = context;
        }

        public async Task<Store> GetByEmailAsync(string email)
        {
            return await _context.Stores.FirstOrDefaultAsync(e => e.Email == email);
        }
    }
}
