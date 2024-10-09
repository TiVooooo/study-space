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

    public class ImageRoomRepository : GenericRepository<ImageRoom>
    {
        public ImageRoomRepository() { }
        public ImageRoomRepository(EXE201_StudySpaceContext context)
        {
            _context = context;
        }

        public async Task<List<ImageRoom>> GetImageRoomAsync()
        {
            return await _context.ImageRooms
            .Include(r => r.Room)
            .ThenInclude(ro => ro.Store)
            .ToListAsync();
        }
    }
}
