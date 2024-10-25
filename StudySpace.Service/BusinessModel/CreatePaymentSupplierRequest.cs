using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class CreatePaymentSupplierRequest
    {
        public int StoreID { get; set; }
        public int PackageID { get; set; }
        public int Duration {  get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
    }
}
