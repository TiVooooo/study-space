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
    public class AccountRepository : GenericRepository<Account>
    {
        public AccountRepository() { }
        public AccountRepository(EXE201_StudySpaceContext context)
        {
            _context = context;
        }
    }
}
