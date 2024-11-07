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

        public IQueryable<Store> GetAllRooms()
        {
            return _context.Stores
                .Include(s => s.Rooms)
                    .ThenInclude(r => r.Bookings)
                .Include(s => s.Rooms)
                    .ThenInclude(r => r.Bookings)
                    .ThenInclude(b => b.Feedbacks)
                .Include(s => s.Rooms)
                .Include(s => s.Transactions)
                .Include(s => s.StorePackages)
                .Include(s => s.StorePackages)
                    .ThenInclude(p => p.Package);
        }

    }
}
