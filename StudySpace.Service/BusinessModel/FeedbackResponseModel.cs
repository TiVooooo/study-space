using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class FeedbackModel
    {
        public int TotalPage { get; set; }
        
        public int TodalFeedback { get; set; }
        public List<FeedbackResponseModel> FeedbackResponses { get; set; }
    }

    public class FeedbackResponseModel
    {
        public string Avatar {  get; set; }


        public string? ReviewText { get; set; }


        public List<string>? Images { get; set; }


        public DateTime? BookingDate { get; set; }

        public int? Star {  get; set; }  
        public string UserName { get; set; }

    }
}
