using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class RoomModel
    {
        public string RoomName { get; set; }

        public string StoreName { get; set; }

        public int? Capacity { get; set; }

        public decimal? PricePerHour { get; set; }

        public string Description { get; set; }

        public bool? Status { get; set; }

        public decimal? Area { get; set; }

        public string Type { get; set; }
    }
}
