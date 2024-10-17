using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class CreateBookingRequestModel
    {
        public int? UserId { get; set; }

        public int? RoomId { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public double? Fee { get; set; }

        public string Note { get; set; }
    }
}
