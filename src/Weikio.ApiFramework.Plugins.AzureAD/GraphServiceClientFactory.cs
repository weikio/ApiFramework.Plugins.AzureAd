using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Weikio.ApiFramework.Plugins.AzureAD
{
    public class GraphServiceClientFactory
    {
        private static GraphServiceClient _graphServiceClient;

        private static readonly SemaphoreSlim _clientLock = new SemaphoreSlim(1, 1);

        public static async Task<GraphServiceClient> GetGraphClient(AzureAdOptions options)
        {
            if (_graphServiceClient != null)
            {
                return _graphServiceClient;
            }

            try
            {
                await _clientLock.WaitAsync(TimeSpan.FromSeconds(15));

                if (_graphServiceClient != null)
                {
                    return _graphServiceClient;
                }

                var d = new ClientSecretCredential(options.Tenant, options.ClientId, options.ClientSecret, new TokenCredentialOptions()
                {
                    AuthorityHost = new Uri(options.Authority)
                });
                var scopes = new[] { $"{options.ApiUrl}.default" };

                _graphServiceClient = new GraphServiceClient(d);

                return _graphServiceClient;
                // _graphServiceClient = new GraphServiceClient($"{options.ApiUrl}{options.Version}/", new DelegateAuthenticationProvider((async request =>
                // {
                //     // The GraphServiceClient has a built-in retry. This is executed again if we get a 401
                //     var app = ConfidentialClientApplicationBuilder.Create(options.ClientId)
                //         .WithClientSecret(options.ClientSecret)
                //         .WithAuthority(new Uri(options.Authority))
                //         .Build();
                //
                //     var scopes = new[] { $"{options.ApiUrl}.default" };
                //
                //     var tokenResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();
                //     var accessToken = tokenResult.AccessToken;
                //
                //     request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                // })));
                //
                // return _graphServiceClient;
            }
            finally
            {
                _clientLock.Release();
            }
        }
    }
}
