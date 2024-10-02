using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class UpdateAccountModel
    {

        public string? Name { get; set; }

        public string? Password { get; set; }

        public string? NewPassword { get; set; }

        public string? ConfirmPassword { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public IFormFile? AvatarUrl { get; set; }
    }
}
