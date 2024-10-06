using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class DashboardSupplierModel
    {
        public string StoreName { get; set; }

        public Double TotalIncome { get; set; }

        public int TotalBooking { get; set; }
        public int TotalRoom { get; set; }

    }
}
