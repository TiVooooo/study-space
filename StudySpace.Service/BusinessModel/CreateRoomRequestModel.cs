using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class CreateRoomRequestModel
    {
        public int SpaceId { get; set; }

        public string RoomName { get; set; }

        public int StoreId { get; set; }

        public int Capacity { get; set; }

        public double PricePerHour { get; set; }

        public string Description { get; set; }

        public double Area { get; set; }

        public string HouseRule { get; set; }
        public List<IFormFile>? ImageRoom { get; set; }
    }
}
