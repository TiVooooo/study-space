using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class BookingInSupModel
    {
        public string RoomName { get; set; }
        public string UserName { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public bool? Status { get; set; }

        public double? Fee { get; set; }

        public DateTime? BookingDate { get; set; }

    }
}
