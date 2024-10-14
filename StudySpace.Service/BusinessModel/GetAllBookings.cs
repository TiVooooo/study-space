using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class GetAllBookings
    {
        public int BookingId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserAddress { get; set; }
        public string Gender { get; set; }
        public string Avatar { get; set; }
        public string? BookingDate { get; set; }
        public string? BookingTime { get; set; }
        public bool? Checkin { get; set; }
        public int? RoomId { get; set; }
        public string RoomName { get; set; }
        public string StoreName { get; set; }
        public int? Capacity { get; set; }
        public double? PricePerHour { get; set; }
        public string Description { get; set; }
        public bool? Status { get; set; }
        public double? Area { get; set; }
        public string RoomType { get; set; }
        public string SpaceType { get; set; }
        public string Image { get; set; }
        public string Address { get; set; }
    }
}
