using Microsoft.EntityFrameworkCore;
using StudySpace.Data.Base;
using StudySpace.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Data.Repository
{


    public class RoomRepository : GenericRepository<Room>
    {
        public RoomRepository() { }
        public RoomRepository(EXE201_StudySpaceContext context)
        {
            _context = context;
        }

        public async Task<List<Room>> GetAllRoomsAsync()
        {
            return await _context.Rooms
                .Include(r => r.Store)
                .Include(r => r.ImageRooms)
                .Include(r=>r.Space)
                .ToListAsync();
        }
        public IQueryable<Room> FindByConditionv2(Expression<Func<Room, bool>> expression)
        {
            return _context.Rooms.Where(expression);
        }

    }
}
