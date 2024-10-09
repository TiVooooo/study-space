using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Data.Models
{
    public class RoomSupModel
    {

        public int BookingId { get; set; }

        public string UserName { get; set; }

        public string Email {  get; set; }

        public string UserAddress { get; set; }
        public string Gender { get; set; }

        public string Avatar { get; set; }

        public DateTime? BookedDate { get; set; }

        public TimeSpan? BookedTime { get; set; }

        public bool Checkin { get; set; }

        public int RoomId { get; set; }
        public string RoomName { get; set; }

        public string StoreName { get; set; }

        public int Capacity { get; set; }

        public double PricePerHour { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public double Area { get; set; }

        public string RoomType { get; set; }

        public string SpaceType { get; set; }   

        public string Image { get; set; }

        public string Address { get; set; }

    }

    
}
