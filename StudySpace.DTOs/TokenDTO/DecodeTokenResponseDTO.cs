using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.DTOs.TokenDTO
{
    public class DecodeTokenResponseDTO
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string RoleName { get; set; }
        public string AvaURL { get; set; }
        public string IsPackaged { get; set; }
        // public DateTime Expiration { get; set; }
    }
}
