using System;
using System.Globalization;

namespace Weikio.ApiFramework.Plugins.AzureAD
{
    public class AzureAdOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Tenant { get; set; }
        public string Instance { get; set; } = "https://login.microsoftonline.com/{0}";
        public string ApiUrl { get; set; } = "https://graph.microsoft.com/";

        public string Version { get; set; } = "v1.0";

        private string _authority;

        public string Authority
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_authority))
                {
                    return string.Format(CultureInfo.InvariantCulture, Instance, Tenant);
                }

                return _authority;
            }
            set
            {
                _authority = value;
            }
        }
    }
}
