using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class CreateAmityRequestModel
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public int? Quantity { get; set; }

        public string Description { get; set; }
    }
}
