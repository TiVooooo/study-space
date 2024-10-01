using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class UserModel
    {
        public string Name { get; set; }

        public string Email { get; set; }
        public string AvatarUrl { get; set; }

        public string Phone {  get; set; }
        public string RoleName { get; set; }

    }
}
