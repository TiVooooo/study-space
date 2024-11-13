using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class FeedbackInUI
    {
        public string AvatarURL { get; set; }
        public string UserName { get; set; }
        public string RoomName { get; set; }
        public int Rate { get; set; }
        public string FeedbackText { get; set; }
    }
}
