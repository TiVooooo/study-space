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
    public class UserRoleRepository : GenericRepository<UserRole>
    {
        public UserRoleRepository() { }
        public UserRoleRepository(EXE201_StudySpaceContext context)
        {
            _context = context;
        }

        public async Task<UserRole> GetByNameAsync(string roleName)
        {
            return await _context.UserRoles.FirstOrDefaultAsync(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

    }
}
