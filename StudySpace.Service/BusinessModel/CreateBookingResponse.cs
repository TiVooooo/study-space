using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class CreateBookingResponse
    {
        public int? UserId { get; set; }
        public int? RoomId { get; set; }
        public string RoomName { get; set; }
        public string CheckInDate { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutDate { get; set; }
        public string CheckOutTime { get; set; }
        public double? Total { get; set; }
    }
}
