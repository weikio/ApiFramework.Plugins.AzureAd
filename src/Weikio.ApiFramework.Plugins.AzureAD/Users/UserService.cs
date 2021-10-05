using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Weikio.ApiFramework.Plugins.AzureAD.Users
{
    public class UserService
    {
        private readonly AzureAdOptions _options;

        public UserService(AzureAdOptions options)
        {
            _options = options;
        }

        public async Task<User> GetUser(string user)
        {
            var graphServiceClient = await GraphServiceClientFactory.GetGraphClient(_options);

            // This can automatically retrieve user by Id (guid) or by user principal name (mm.aa@test.com. email must be urlencoded by the requester).
            // The default field set doesn't include department so fields must be listed explicitly.
            var result = await graphServiceClient.Users[user]
                .Request()
                .Select("id,userPrincipalName,mail,displayName,givenName,surname,jobTitle,department")
                .GetAsync();

            return result;
        }

        public async Task UpdateUser(string user, User changedValues)
        {
            var graphServiceClient = await GraphServiceClientFactory.GetGraphClient(_options);

            await graphServiceClient.Users[user].Request().UpdateAsync(changedValues);
        }

        public async Task<ExtensionPropertyValue<object>> GetExtensionPropertyValue(string user, string propertyName)
        {
            var graphServiceClient = await GraphServiceClientFactory.GetGraphClient(_options);

            var result = await graphServiceClient.Users[user]
                .Request()
                .Select($"id,{propertyName}")
                .GetAsync();

            result.AdditionalData.TryGetValue(propertyName, out var value);

            return new ExtensionPropertyValue<object>()
            { 
                ObjectType = "User",
                ObjectId = result.Id,
                PropertyName = propertyName,
                Value = value
            };
        }

        public async Task<ExtensionPropertyValue<object>> SetExtensionPropertyValue(string user, string propertyName, object value)
        {
            var changedValues = new User();

            changedValues.AdditionalData = new Dictionary<string, object>()
            {
                { propertyName, value }
            };

            await UpdateUser(user, changedValues);

            return await GetExtensionPropertyValue(user, propertyName);
        }
    }
}
