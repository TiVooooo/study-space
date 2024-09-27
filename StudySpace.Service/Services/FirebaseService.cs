using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Storage;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using StudySpace.Service.Base;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static StudySpace.Service.Configuration.ConfigurationModel;

namespace StudySpace.Service
{
    public interface IFirebaseService
    {
        Task<string> UploadImageToFirebaseAsync(IFormFile imageFile, string basePath);
    }

    public class FirebaseService : IFirebaseService
    {
        private readonly FirebaseConfiguration _firebaseConfig;

        public FirebaseService(IOptions<FirebaseConfiguration> firebaseConfig)
        {
            _firebaseConfig = firebaseConfig.Value;
        }

        /*        public async Task<string> UploadImageToFirebaseAsync(IFormFile imageFile)
                {
                    if (imageFile.Length > 0)
                    {
                        // Generate a unique file name
                        var fileName = $"{Guid.NewGuid()}_{imageFile.FileName}";

                        // Define the bucket and the object name
                        var bucketName = _firebaseConfig.Bucket;

                        using (var stream = imageFile.OpenReadStream())
                        {
                            // Upload the file to Firebase Cloud Storage
                            await _storageClient.UploadObjectAsync(bucketName, $"images/{fileName}", null, stream);

                            // Get the public URL for the uploaded image
                            return $"https://storage.googleapis.com/{bucketName}/images/{fileName}";
                        }
                    }

                    return null;
                }*/

        public async Task<string> UploadImageToFirebaseAsync(IFormFile imageFile, string basePath)
        {
            if (imageFile.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}_{imageFile.FileName}";

                var config = new FirebaseAuthConfig
                {
                    ApiKey = _firebaseConfig.ApiKey,
                    AuthDomain = _firebaseConfig.AuthDomain,
                    Providers = new FirebaseAuthProvider[]
    {
                new EmailProvider(),
                new GoogleProvider()
    }
                };

                // Define the bucket and the object name
                var authClient = new FirebaseAuthClient(config);


                var userCens = await authClient.SignInWithEmailAndPasswordAsync(_firebaseConfig.AuthEmail, _firebaseConfig.AuthPassword);

                if (userCens == null) throw new FirebaseAuthException("Can not ss", new AuthErrorReason());

                var storage = new FirebaseStorage(
                    _firebaseConfig.Bucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = async () =>
                        {
                            var idToken = await userCens.User.GetIdTokenAsync();
                            return idToken;
                        },
                        ThrowOnCancel = true
                    });

                var stream = imageFile.OpenReadStream();
                var destinationPath = $"{basePath}/{imageFile.FileName}";

                var downloadUrl = await storage.Child(destinationPath).PutAsync(stream);
                return downloadUrl;
            }

            return null;
        }
    }
}
