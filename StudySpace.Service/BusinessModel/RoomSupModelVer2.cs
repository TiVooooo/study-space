using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class RoomSupModelVer2
    {
        public string RoomName { get; set; }

        public string StoreName { get; set; }

        public int Capacity { get; set; }

        public double PricePerHour { get; set; }

        public string Description { get; set; }

        public bool Status { get; set; }

        public string TypeSpace { get; set; }

        public string TypeRoom { get; set; }

        public double Area { get; set; }

        public double? Longtitude { get; set; }

        public double? Latitude { get; set; }

        public bool isOvernight { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public string[] HouseRule { get; set; }

        public ListImages ListImages { get; set; }

        public string Address { get; set; }

        public List<AmitiesInRoomVer2> AmitiesInRoom { get; set; }
    }

    public class AmitiesInRoomVer2
    {
        public int? Id { get; set; }

        public string? Name { get; set; }

        public string? Type { get; set; }

        public bool? Status { get; set; }

        public int? Quantity { get; set; }

        public string? Description { get; set; }
    }
}
