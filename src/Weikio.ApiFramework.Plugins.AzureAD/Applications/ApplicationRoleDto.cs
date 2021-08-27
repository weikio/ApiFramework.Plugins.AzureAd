using System;
using System.Collections.Generic;

namespace Weikio.ApiFramework.Plugins.AzureAD.Applications
{
    public class ApplicationRoleDto
    {
        public Guid RoleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
        public List<string> AllowedMemberTypes { get; set; }
    }
}