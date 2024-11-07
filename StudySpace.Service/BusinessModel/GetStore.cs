using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class GetStore
    {
        public int Id { get; set; }

        public string ThumbnailUrl { get; set; }

        public double? Longitude { get; set; }

        public double? Latitude { get; set; }

        public string Description { get; set; }

        public bool? Status { get; set; }

        public bool? IsApproved { get; set; }

        public bool? IsPackaged {  get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }

        public DateTime? CreateDate { get; set; }

        public DateTime? OpenTime { get; set; }

        public DateTime? CloseTime { get; set; }

        public bool? IsOverNight { get; set; }

        public bool? IsActive { get; set; }

        public string TaxNumber { get; set; }

        public string PostalNumber { get; set; }

        public StoreWithPack StoreWithPack { get; set; }
    }

    public class StoreWithPack
    {
        public int? PackageID { get; set; }
        public string PackageName { get; set; }
        public double? Duration { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}
