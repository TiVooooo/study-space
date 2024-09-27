using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class GetDetailUserModel
    {
        public string RoleName { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string Gender { get; set; }

        public DateTime Dob { get; set; }

        public bool IsActive { get; set; }

        public double Wallet { get; set; }

        public string AvatarUrl { get; set; }
    }
}
