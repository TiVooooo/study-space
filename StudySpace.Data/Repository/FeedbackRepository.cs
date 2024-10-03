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
   
    public class FeedbackRepository : GenericRepository<Feedback>
    {
        public FeedbackRepository() { }
        public FeedbackRepository(EXE201_StudySpaceContext context)
        {
            _context = context;
        }

        public async Task<List<Feedback>> GetFeedbacksByRoomIdAsync(int roomId)
        {
            return await _context.Feedbacks
                .Include(f => f.User)  
                .Include(f => f.ImageFeedbacks) 
                .Where(f => f.Booking.RoomId == roomId && f.Status == true) 
                .ToListAsync();
        }

        public async Task<Feedback> GetFeedbackByIdAsync(int feedbackId)
        {
            return await _context.Feedbacks
                .Include(f => f.User) 
                .Include(f => f.Booking)  
                .Include(f => f.ImageFeedbacks) 
                .FirstOrDefaultAsync(f => f.Id == feedbackId);
        }

    }
}
