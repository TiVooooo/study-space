using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class RoomSupModelVer1
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }

        public string StoreName { get; set; }

        public int Capacity { get; set; }

        public double PricePerHour { get; set; }

        public string Description { get; set; }

        public bool Status { get; set; }

        public double Area { get; set; }

        public string RoomType { get; set; }

        public string SpaceType { get; set; }

        public string Image { get; set; }

        public string Address { get; set; }

        public List<AmitiesInRoomVer1> AmitiesInRoom { get; set; }
    }

    public class AmitiesInRoomVer1
    {
        public int? Id { get; set; }

        public string? Name { get; set; }

        public string? Type { get; set; }

        public bool? Status { get; set; }

        public int? Quantity { get; set; }

        public string? Description { get; set; }
    }
}
