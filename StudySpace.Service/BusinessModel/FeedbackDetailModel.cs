using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class FeedbackDetailModel
    {
        public int FeedbackId {  get; set; }
        public bool? Status { get; set; }
        public int? Star { get; set; }
        public string UserName { get; set; }
        public string UserAvaUrl { get; set; }
        public string ReviewText { get; set; }
        public DateTime? BookingDate { get; set; }
        public List<string> FeedbackImages { get; set; }
    }
}
