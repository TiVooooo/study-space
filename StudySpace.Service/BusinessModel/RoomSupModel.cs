using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Data.Models
{
    public class RoomSupModel
    {
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

        public List<AmitiesInRoom> AmitiesInRoom { get; set; }
    }

    public class AmitiesInRoom
    {
        public int? Id { get; set; }

        public string? Name { get; set; }

        public string? Type { get; set; }

        public bool? Status { get; set; }

        public int? Quantity { get; set; }

        public string? Description { get; set; }
    }
}
