using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{


    public class TotalTransaction
    {
        public List<TransactionUserModel> Transaction { get; set; }

        public double TotalRevenue { get; set; }
        public double TotalCost
        {
            get; set;
        }
    }
        public class TransactionUserModel
        {
            public int Id { get; set; }
            public DateTime? Date { get; set; }
            public double? Fee { get; set; }
            public string? PaymentMethod { get; set; }
            public string? Status { get; set; }

        public string? Type { get; set; }
        }
    
}
