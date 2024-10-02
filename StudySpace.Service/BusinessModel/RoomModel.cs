using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{

    public class GetAllRoomModel
    {
        public List<RoomModel> Rooms { get; set; }
        public int TotalCount { get; set; }
    }

    public class RoomModel
    {
        public string RoomName { get; set; }

        public string StoreName { get; set; }

        public int Capacity { get; set; }

        public double PricePerHour { get; set; }

        public string Description { get; set; }

        public bool Status { get; set; }

        public double Area { get; set; }

        public string Type { get; set; }

        public string Image {  get; set; }

        public string Address { get; set; }
    }
}
