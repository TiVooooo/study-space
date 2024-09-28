using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class FeedbackRequestModel
    {
        public int UserId { get; set; }

        public int BookingId { get; set; }

        public int Rating { get; set; }

        public string? ReviewText { get; set; }

        public DateTime ReviewDate { get; set; }

        public List<IFormFile>? Files { get; set; }

    }
}
