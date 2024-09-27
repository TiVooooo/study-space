using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.Configuration
{
    public class ConfigurationModel
    {
        public class FirebaseConfiguration
        {
            public string? ApiKey { get; set; }
            public string? AuthEmail { get; set; }
            public string? AuthPassword { get; set; }
            public string? Bucket { get; set; }
            public string? AuthDomain { get; set; }
            public string? ProjectId { get; set; }
            public string? MessagingSenderId { get; set; }
            public string? AppId { get; set; }
            public string? MeasurementId { get; set; }
        }
        public class GoogleCloudConfiguration
        {
            public string Type { get; set; }
            public string ProjectId { get; set; }
            public string PrivateKeyId { get; set; }
            public string PrivateKey { get; set; }
            public string ClientEmail { get; set; }
            public string ClientId { get; set; }
            public string AuthUri { get; set; }
            public string TokenUri { get; set; }
            public string AuthProviderCertUrl { get; set; }
            public string ClientCertUrl { get; set; }
        }


    }
}