using System;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Weikio.ApiFramework.Plugins.AzureAD.Users
{
    public class UserService
    {
        public async Task<User> GetUser(string user, AzureAdOptions options)
        {
            var graphServiceClient = await GraphServiceClientFactory.GetGraphClient(options);

            // This can automatically retrieve user by Id (guid) or by user principal name (mm.aa@test.com. email must be urlencoded by the requester).
            // The default field set doesn't include department so fields must be listed explicitly.
            var result = await graphServiceClient.Users[user]
                .Request()
                .Select("id,userPrincipalName,mail,displayName,givenName,surname,jobTitle,department")
                .GetAsync();

            return result;
        }

        public async Task UpdateUser(string user, User changedValues, AzureAdOptions options)
        {
            var graphServiceClient = await GraphServiceClientFactory.GetGraphClient(options);

            await graphServiceClient.Users[user].Request().UpdateAsync(changedValues);
        }
    }
}
