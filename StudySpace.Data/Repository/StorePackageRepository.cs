using StudySpace.Data.Base;
using StudySpace.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Data.Repository
{

    public class StorePackageRepository : GenericRepository<StorePackage>
    {
        public StorePackageRepository() { }
        public StorePackageRepository(EXE201_StudySpaceContext context)
        {
            _context = context;
        }
    }
}
