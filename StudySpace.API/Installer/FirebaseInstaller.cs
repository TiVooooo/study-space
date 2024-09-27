using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static StudySpace.Service.Configuration.ConfigurationModel;

namespace StudySpace.API.Installer
{
    public interface IInstaller
    {
        void InstallServices(IServiceCollection services, IConfiguration configuration);
    }
    public class FirebaseInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            var firebaseConfigSection = configuration.GetSection("Firebase");
            var firebaseConfig = firebaseConfigSection.Get<FirebaseConfiguration>();
            services.Configure<FirebaseConfiguration>(firebaseConfigSection);
            services.AddSingleton(firebaseConfig);

            // Initialize Firebase using the credentials (API key-based credential may need further config)
            GoogleCredential credential = GoogleCredential.GetApplicationDefault();

            services.AddSingleton(StorageClient.Create(credential));
        }
    }
}
