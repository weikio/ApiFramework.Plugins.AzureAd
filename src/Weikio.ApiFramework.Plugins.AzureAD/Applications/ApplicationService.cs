using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Plugins.AzureAD.Applications
{
    public class ApplicationService
    {
        private AzureAdOptions _configuration;

        public ApplicationService(AzureAdOptions configuration)
        {
            _configuration = configuration;
        }

        public async Task<System.Collections.Generic.List<ApplicationDto>> GetApplications()
        {
            var graphServiceClient = await GraphServiceClientFactory.GetGraphClient(_configuration);

            var result = new List<ApplicationDto>();
            var apps = await graphServiceClient.ServicePrincipals.Request().GetAsync();
            
            while (apps.Count > 0)
            {
                foreach (var app in apps)
                {
                    var dto = new ApplicationDto()
                    {
                        Id = new Guid(app.Id), AppId = new Guid(app.AppId), Name = app.DisplayName, Description = app.Description,
                    };

                    dto.ApplicationRoles = new List<ApplicationRoleDto>();

                    if (app.AppRoles?.Any() == true)
                    {
                        foreach (var appRole in app.AppRoles)
                        {
                            var appRoleDto = new ApplicationRoleDto()
                            {
                                RoleId = appRole.Id.GetValueOrDefault(),
                                Name = appRole.DisplayName,
                                Description = appRole.Description,
                                Value = appRole.Value,
                                AllowedMemberTypes = new List<string>(appRole.AllowedMemberTypes),
                            };

                            dto.ApplicationRoles.Add(appRoleDto);
                        }
                    }

                    result.Add(dto);
                }

                if (apps.NextPageRequest != null)
                {
                    apps = await apps.NextPageRequest.GetAsync();
                }
                else
                {
                    break;
                }
            }

            return result;
        }
    }
}
