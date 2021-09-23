using System;
using System.Collections.Generic;

namespace Weikio.ApiFramework.Plugins.AzureAD.Applications
{
    public class ApplicationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid AppId { get; set; }
        public string Description { get; set; }
        public List<ApplicationRoleDto> ApplicationRoles { get; set; }

    }
}
