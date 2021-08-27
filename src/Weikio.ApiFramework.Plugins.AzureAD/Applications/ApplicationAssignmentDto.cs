using System;

namespace Weikio.ApiFramework.Plugins.AzureAD.Applications
{
    public class ApplicationAssignmentDto
    {
        public string Id { get; set; }
        public Guid? ApplicationId { get; set; }
        public Guid? UserId { get; set; }
        public string User { get; set; }
        public string Role { get; set; }
    }
}