using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class FilterModel
    {
        public string PriceSort { get; set; }
        public string RatingSort { get; set; }
        public List<string> SelectedUtilities { get; set; }
        public Double[]? PriceRange {get; set; }

    }
}
