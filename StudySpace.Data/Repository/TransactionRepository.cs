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
    public class TransactionRepository : GenericRepository<Transaction>
    {
        public TransactionRepository() { }
        public TransactionRepository(EXE201_StudySpaceContext context)
        {
            _context = context;
        }

        public List<Transaction> GetAllTransactions()
        {
            return _context.Transactions
                .Include(t => t.Booking)
                .Include(t => t.Booking.Room)
                .ToList();
        }
    }
}
