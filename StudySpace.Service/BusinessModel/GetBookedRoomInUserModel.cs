using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class GetBookedRoomInUserModel
    {

        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }

        public string StoreName { get; set; }

        public int Capacity { get; set; }

        public double PricePerHour { get; set; }

        public string Description { get; set; }

        public bool Status { get; set; }

        public double Area { get; set; }

        public string Type { get; set; }

        public string Image { get; set; }

        public string Address { get; set; }

        public bool? isOvernight { get; set; }

        public string TypeSpace { get; set; }
        public  DateTime? BookedDate { get; set; }
        public TimeSpan? BookedTime { get; set; }
        public string BookingStatus { get; set; }
        public TimeSpan? Start {  get; set; }
        public TimeSpan? End { get; set; }
        public bool CheckIn {  get; set; }
        public string PaymentMethod { get; set; }

        public bool IsFeedback { get; set; }
    }
}
