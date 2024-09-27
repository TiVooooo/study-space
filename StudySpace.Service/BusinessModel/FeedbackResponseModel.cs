using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class FeedbackResponseModel
    {
        public string Avatar {  get; set; }


        public string? ReviewText { get; set; }


        public List<string>? Images { get; set; }

    }
}
