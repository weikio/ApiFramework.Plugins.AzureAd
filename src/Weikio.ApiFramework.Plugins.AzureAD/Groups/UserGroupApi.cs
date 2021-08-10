using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Weikio.ApiFramework.Plugins.AzureAD.Groups
{
    /// <summary>
    /// Requires: GroupMember.Read.All, User.Read.All
    /// </summary>
    public class UserGroupApi
    {
        public AzureAdOptions Configuration { get; set; }
        
        public async Task<List<GroupDto>> GetUserGroups(string user)
        {
            var result = await GetGroups(user);

            return result;
        }
        
        public async Task<List<GroupDto>> GetUserSecurityGroups(string user)
        {
            var allGroups = await GetGroups(user);

            var result = allGroups.Where(x => x.IsSecurity).ToList();

            return result;
        }

        private async Task<List<GroupDto>> GetGroups(string user)
        {
            var graphServiceClient = await GetGraphClient(Configuration);

            var result = new List<GroupDto>();
            var groups = await graphServiceClient.Users[user].MemberOf.Request().GetAsync();

            while (groups.Count > 0)
            {
                foreach (var grp in groups.OfType<Group>())
                {
                    result.Add(new GroupDto(new Guid(grp.Id), grp.DisplayName, grp.SecurityEnabled.GetValueOrDefault()));
                }
                if (groups.NextPageRequest != null)
                {
                    groups = await groups.NextPageRequest.GetAsync();
                }
                else
                {
                    break;
                }
            }
            
            return result;
        }
        
        private static async Task<GraphServiceClient> GetGraphClient(AzureAdOptions options)
        {
            var app = ConfidentialClientApplicationBuilder.Create(options.ClientId)
                .WithClientSecret(options.ClientSecret)
                .WithAuthority(new Uri(options.Authority))
                .Build();

            var scopes = new[] { $"{options.ApiUrl}.default" };

            var tokenResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            var accessToken = tokenResult.AccessToken;

            var httpClient = new HttpClient();
            var defaultRequestHeaders = httpClient.DefaultRequestHeaders;

            if (defaultRequestHeaders.Accept == null || defaultRequestHeaders.Accept.All(m => m.MediaType != "application/json"))
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var graphServiceClient = new GraphServiceClient($"{options.ApiUrl}{options.Version}/", new DelegateAuthenticationProvider((request =>
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                return Task.CompletedTask;
            })));

            return graphServiceClient;
        }
    }
}
