using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class AmitiesDetailsResponse
    {
        public int AmityId { get; set; }
        public string AmityName { get; set; }
        public string AmityType { get; set; }
        public string AmityStatus { get; set; }
        public int? Quantity { get; set; }
        public string Description { get; set; }
    }
}
