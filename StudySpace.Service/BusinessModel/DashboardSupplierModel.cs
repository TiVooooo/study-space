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

        public TotalInThisMonth TotalInThisMonth { get; set; }

        public List<MonthRevenue> MonthRevenue { get; set; }

        public List<PopularRoom> PopularRooms { get; set; }
    }

    public class MonthRevenue
    {
        public string Month { get; set; }
        public int TransactionInMonth { get; set; }
        public double RevenueInMonth { get; set; }
    }

    public class PopularRoom
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Image {  get; set; }
        public int TotalBooking { get; set; }
    }

    public class TotalInThisMonth
    {
        public int TotalBookingsInMonth { get; set; }
        public double TotalTransactionsInMonth { get; set; }
        public double TotalRevenueInMonth { get; set; }
    }
}
