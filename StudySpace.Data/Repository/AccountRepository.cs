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

        public async Task<Account> GetByEmailAsync(string email)
        {
            return await _context.Accounts.Include(a => a.Role).FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<Account> GetRole(string roleName)
        {
            return await _context.Accounts.Include(a => a.Role).FirstOrDefaultAsync(a => a.Role.RoleName.ToLower() == roleName.ToLower());
        }

        public IQueryable<Account> GetAllAccounts()
        {
            return _context.Accounts
                    .Include(a => a.Role);
        }
    }
}
