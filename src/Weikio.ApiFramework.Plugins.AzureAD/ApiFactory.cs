using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;

namespace Weikio.ApiFramework.Plugins.AzureAD
{
    public class ApiFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ApiFactory> _logger;

        public ApiFactory(ILoggerFactory loggerFactory, ILogger<ApiFactory> logger)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<Type>> Create(AzureAdOptions configuration)
        {
            var app = ConfidentialClientApplicationBuilder.Create("144cd1e7-827c-4bd3-a097-90acb77d4568")
                .WithClientSecret("")
                .WithAuthority(new Uri("https://login.microsoftonline.com/37e55da6-fb62-456a-8d8e-f6f5b649092f"))
                .Build();

            var scopes = new string[] { $"https://graph.microsoft.com/.default" };

            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            var accessToken = result.AccessToken;

            var httpClient = new HttpClient();
            var defaultRequestHeaders = httpClient.DefaultRequestHeaders;

            if (defaultRequestHeaders.Accept == null || defaultRequestHeaders.Accept.All(m => m.MediaType != "application/json"))
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var res = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/users");

            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                var objs = JObject.Parse(json);
            }

            return new List<Type>() { };
        }
    }

    public class AzureAdOptions
    {
    }
}
