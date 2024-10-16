using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class GetAllTransactionModel
    {
        public int Id { get; set; }
        public DateTime? Date { get; set; }
        public string PaymentMethod { get; set; }

        public string Status { get; set; }

        public string Type { get; set; }

        public string PackageName { get; set; }

        public double? Fee { get; set; }
        public string RoomName { get; set; }

        public string UserName { get; set; }

        public string Avatar {  get; set; }

    }
}
