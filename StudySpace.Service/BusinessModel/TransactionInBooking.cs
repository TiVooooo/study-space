using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class TransactionInBooking
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string UserAddress { get; set; }
        public string Gender { get; set; }

        public string Avatar { get; set; }

        public string RoomName { get; set; }

        public DateTime? BookedDate { get; set; }

        public string CheckInDate { get; set; }
        public TimeSpan? Start { get; set; }
        public string CheckOutDate { get; set; }
        public TimeSpan? End { get; set; }
        public double? Fee { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; }
        public string? Hastag { get; set; }
    }
}
