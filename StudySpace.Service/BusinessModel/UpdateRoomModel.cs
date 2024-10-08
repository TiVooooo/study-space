using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class UpdateRoomModel
    {

        public double? PricePerHour { get; set; }

        public string? Description { get; set; }


        public List<string>? HouseRule { get; set; }
      

    }


}
