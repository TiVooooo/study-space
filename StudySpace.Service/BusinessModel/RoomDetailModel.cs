using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class RoomDetailModel
    {
        public string RoomName { get; set; }

        public string StoreName { get; set; }

        public int Capacity { get; set; }
        public double PricePerHour { get; set; }

        public string Description { get; set; }

        public bool Status { get; set; }

        public double Area { get; set; }

        public double? Longtitude
        {

            get; set;
        }

        public double? Latitude { get; set; }

        public string HouseRule { get; set; }

        public string TypeOfRoom { get; set; }

        public List<string> ListImages { get; set; }

        public List<string> Aminities { get; set; }

        public string Address { get; set; }



        public List<RoomModel> RelatedRoom { get; set; }

    }
}
