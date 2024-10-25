using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudySpace.Service.Helper
{
    public class PayOSSignatureVerifier
    {
        private readonly string _checkSum;

        public PayOSSignatureVerifier(string checksumKey)
        {
            _checkSum = checksumKey;
        }

        public bool IsValidSignature(string transactionData, string transactionSignature)
        {
            try
            {
                var jsonObject = JsonDocument.Parse(transactionData).RootElement;
                var sortedKeys = jsonObject.EnumerateObject()
                    .Select(property => property.Name)
                    .OrderBy(key => key)
                    .ToList();

                var transactionStr = new StringBuilder();
                foreach (var key in sortedKeys)
                {
                    string value = jsonObject.GetProperty(key).ToString();
                    transactionStr.Append($"{key}={value}");
                    if (key != sortedKeys.Last())
                    {
                        transactionStr.Append("&");
                    }
                }

                var signature = ComputeHmacSha256(transactionStr.ToString(), _checkSum);

                return signature.Equals(transactionSignature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating signature: {ex.Message}");
                return false;
            }
        }

        private string ComputeHmacSha256(string message, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hash = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }

}

