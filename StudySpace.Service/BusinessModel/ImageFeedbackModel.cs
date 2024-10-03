using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class FeedbackBu
    {
        public double AverageStar {  get; set; }
        public List<ImageFeedbackModel> ImageFeedbackModels { get; set; }
    }
    public class ImageFeedbackModel
    {
        public int FeedbackId { get; set; }
        public int UserId { get; set; }
        public string UserAvatarUrl { get; set; }
        public string UserName { get; set; }
        public List<string> FeedbackImage { get; set; }
    }
}
