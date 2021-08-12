using System.Threading.Tasks;
using Microsoft.Graph;

namespace Weikio.ApiFramework.Plugins.AzureAD.Users
{
    public class UserService
    {
        public async Task<User> GetUser(string user, AzureAdOptions options)
        {
            var graphServiceClient = await GraphServiceClientFactory.GetGraphClient(options);

            // This can automatically retrieve user by Id (guid) or by user principal name (mm.aa@test.com. email must be urlencoded by the requester) 
            var result = await graphServiceClient.Users[user].Request().GetAsync();

            return result;
        }
    }
}
