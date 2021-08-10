using System;

namespace Weikio.ApiFramework.Plugins.AzureAD.Groups
{
    public class GroupDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        
        public bool IsSecurity { get; set; }

        public GroupDto(Guid id, string displayName, bool isSecurity)
        {
            Id = id;
            DisplayName = displayName;
            IsSecurity = isSecurity;
        }

        public GroupDto()
        {
        }
    }
}
