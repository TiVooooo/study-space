using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.Helper
{
    public static class OrderCodeHashHelper
    {
        public static int GenerateOrderCodeHash(int storeId, int packageId, DateTime startDate)
        {
            string input = $"{storeId}-{packageId}-{startDate:yyyyMMdd}";

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

                int orderCode = BitConverter.ToInt32(hashBytes, 0);

                return Math.Abs(orderCode);
            }
        }
    }
}
