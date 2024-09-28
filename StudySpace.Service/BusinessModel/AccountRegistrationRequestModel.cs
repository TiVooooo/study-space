using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class AccountRegistrationRequestModel
    {
        public string Name { get; set; }

        public string RoleName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string Gender { get; set; }

        public DateTime? Dob { get; set; }

        public IFormFile? File { get; set; }
    }
}
