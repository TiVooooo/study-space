using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class BookedRoomDetailUserModel
    {
        public string RoomName { get; set; }
        public int? RoomId { get; set; }
        public TimeSpan? Start { get; set; }
        public TimeSpan? End { get; set; }
        public double Fee { get; set; }
        public DateTime? BookingDate { get; set; }
        public string? PaymentMethod { get; set; }
        public bool? CheckIn {  get; set; }
        public string? Note {  get; set; }

        public List<string> ImageUrl { get; set; }

    }
}
