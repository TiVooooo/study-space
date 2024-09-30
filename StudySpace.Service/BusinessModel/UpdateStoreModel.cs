using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class UpdateStoreModel
    {
        public IFormFile? ThumbnailUrl { get; set; }

        //public double? Longitude { get; set; }

        //public double? Latitude { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }

        public DateTime? OpenTime { get; set; }

        public DateTime? CloseTime { get; set; }

        public bool? IsOverNight { get; set; }

    }
}
