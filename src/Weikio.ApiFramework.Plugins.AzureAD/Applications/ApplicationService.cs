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

            var apps = await graphServiceClient.Applications
                .Request()
                .GetAsync();

            var result = new List<ApplicationDto>();

            while (apps.Count > 0)
            {
                foreach (var app in apps)
                {
                    var dto = ToDto<ApplicationDto>(app);

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

        public async Task<ApplicationDetailsDto> GetApplicationDetails(string applicationId)
        {
            var graphServiceClient = await GraphServiceClientFactory.GetGraphClient(_configuration);

            var foundApps = await graphServiceClient.Applications
                .Request()
                .Filter($"appId eq '{applicationId}'")
                .GetAsync();

            var appDto = ToDto<ApplicationDetailsDto>(foundApps.FirstOrDefault());

            if (appDto != null)
            {
                var extensionProperties = await graphServiceClient.Applications[appDto.Id.ToString()].ExtensionProperties
                    .Request()
                    .GetAsync();

                if (extensionProperties != null)
                {
                    appDto.ExtensionProperties = extensionProperties
                        .Select(p => new ExtensionPropertyDto()
                        {
                            Id = p.Id,
                            Name = p.Name,
                            TargetObjects = p.TargetObjects?.ToList()
                        })
                        .ToList();
                }
            }

            return appDto;
        }

        private static T ToDto<T>(Microsoft.Graph.Application app) where T : ApplicationDto
        {
            if (app == null)
            {
                return null;
            }

            var dto = Activator.CreateInstance<T>();

            dto.Id = new Guid(app.Id);
            dto.AppId = new Guid(app.AppId);
            dto.Name = app.DisplayName;
            dto.Description = app.Description;
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

            return dto;
        }
    }
}
