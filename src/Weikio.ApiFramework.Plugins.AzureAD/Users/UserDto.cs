using System;

namespace Weikio.ApiFramework.Plugins.AzureAD.Users
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string PrincipalName { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string SurName { get; set; }
        public string JobTitle { get; set; }
        public string Department { get; set; }

        public UserDto(Guid id, string principalName, string email, string displayName, string givenName, string surName, string jobTitle, string department)
        {
            Id = id;
            PrincipalName = principalName;
            Email = email;
            DisplayName = displayName;
            GivenName = givenName;
            SurName = surName;
            JobTitle = jobTitle;
            Department = department;
        }

        public UserDto()
        {
        }
    }
}
