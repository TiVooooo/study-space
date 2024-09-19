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
    }
}
