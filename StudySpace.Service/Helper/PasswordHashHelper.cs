using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.Helper
{
    public static class PasswordHashHelper
    {
        public static string HashPassword(string password)
        {
            using(var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);

                byte[] hash = sha256.ComputeHash(bytes);

                return Convert.ToBase64String(hash);
            }
        }

        public static bool VerifyPassword(string inputPassword, string hashPassword)
        {
            string hashedInputPassword = HashPassword(inputPassword);

            return hashedInputPassword == hashPassword;
        }
    }
}
