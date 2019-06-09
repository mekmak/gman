using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace Mekmak.Gman.Diamond
{
    public class GmailServiceFactory
    {
        public async Task<GmailService> BuildAsync(GmailServiceConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            string credentialsFilePath = config.CredentialsFilePath ?? throw new ArgumentNullException(nameof(config.CredentialsFilePath));
            string tokenDirectory = config.TokenDirectory ?? throw new ArgumentNullException(nameof(config.TokenDirectory));
            string[] scopes = config.ApiScopes ?? throw new ArgumentNullException(nameof(config.ApiScopes));
            string appName = config.AppName ?? throw new ArgumentNullException(nameof(config.AppName));

            if (!File.Exists(credentialsFilePath))
            {
                throw new FileNotFoundException($"Cannot find credentials file '{credentialsFilePath}'");
            }

            if (!Directory.Exists(tokenDirectory))
            {
                throw new DirectoryNotFoundException($"Cannot find token directory '{tokenDirectory}'");
            }

            UserCredential credential;
            using (var stream = new FileStream(credentialsFilePath, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes, 
                    "user", 
                    CancellationToken.None, 
                    new FileDataStore(tokenDirectory));
            }

            // Create Gmail API service.
            var service = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = appName,
            });

            return service;
        }
    }
}
