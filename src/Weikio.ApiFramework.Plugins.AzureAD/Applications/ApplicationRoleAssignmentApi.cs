using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Weikio.ApiFramework.Plugins.AzureAD.Users;

namespace Weikio.ApiFramework.Plugins.AzureAD.Applications
{
    /// <summary>
    /// Requires: AppRoleAssignment.ReadWrite.All
    /// </summary>
    public class ApplicationRoleAssignmentApi
    {
        public AzureAdOptions Configuration { get; set; }

        public async Task<ActionResult<System.Collections.Generic.List<ApplicationAssignmentDto>>> GetAssignments(string applicationId)
        {
            try
            {
                var client = await GraphServiceClientFactory.GetGraphClient(Configuration);

                var assignments = await client.ServicePrincipals[applicationId].AppRoleAssignedTo.Request().GetAsync();

                var result = new List<ApplicationAssignmentDto>();

                foreach (var assignment in assignments)
                {
                    var dto = new ApplicationAssignmentDto
                        {
                            Id = assignment.Id,
                            Role = assignment.ResourceDisplayName, ApplicationId = assignment.ResourceId, UserId = assignment.PrincipalId, User = assignment.PrincipalDisplayName
                        };
                    
                    dto.ApplicationId = assignment.ResourceId;

                    result.Add(dto);
                }

                return result;
            }
            catch (ServiceException e)
            {
                var contentResult = new ContentResult { Content = e.Error?.Message, StatusCode = (int)e.StatusCode };
        
                return contentResult;
            }
        }
        
        public async Task<ActionResult<ApplicationAssignmentDto>> GetAssignmentDetails(string applicationId, string userId)
        {
            try
            {
                var client = await GraphServiceClientFactory.GetGraphClient(Configuration);

                var assignments = await client.ServicePrincipals[applicationId].AppRoleAssignedTo.Request().GetAsync();

                foreach (var assignment in assignments.Where(x => string.Equals(userId, x.PrincipalId.GetValueOrDefault().ToString(), StringComparison.InvariantCultureIgnoreCase)))
                {
                    var dto = new ApplicationAssignmentDto
                    {
                        Id = assignment.Id,
                        Role = assignment.ResourceDisplayName, ApplicationId = assignment.ResourceId, UserId = assignment.PrincipalId, User = assignment.PrincipalDisplayName
                    };
                    
                    dto.ApplicationId = assignment.ResourceId;

                    return dto;
                }
                
                throw new ServiceException(new Error() { Message = "Assignment not found" }, null, HttpStatusCode.NotFound);
            }
            catch (ServiceException e)
            {
                var contentResult = new ContentResult { Content = e.Error?.Message, StatusCode = (int)e.StatusCode };
        
                return contentResult;
            }
        }
        
        [HttpPost]
        public async Task<ActionResult> AddUserToApplication(string user, string applicationId, string roleId)
        {
            try
            {
                var userService = new UserService();
                var userDetails = await userService.GetUser(user, Configuration);

                var appRoleAssignment = new AppRoleAssignment() { ResourceId = Guid.Parse(applicationId), PrincipalId = Guid.Parse(userDetails.Id) };

                if (string.IsNullOrWhiteSpace(roleId))
                {
                    appRoleAssignment.AppRoleId = Guid.Empty;
                }
                else
                {
                    appRoleAssignment.AppRoleId = Guid.Parse(roleId);
                }

                var client = await GraphServiceClientFactory.GetGraphClient(Configuration);
                await client.Users[userDetails.Id].AppRoleAssignments.Request().AddAsync(appRoleAssignment);

                return new OkResult();
            }
            catch (ServiceException e)
            {
                var contentResult = new ContentResult { Content = e.Error?.Message, StatusCode = (int)e.StatusCode };

                return contentResult;
            }
        }

        public async Task<ActionResult> RemoveUserFromApplication(string applicationId, string assignmentId)
        {
            try
            {
                var client = await GraphServiceClientFactory.GetGraphClient(Configuration);

                await client.ServicePrincipals[applicationId].AppRoleAssignments[assignmentId].Request().DeleteAsync();
                
                return new OkResult();
            }
            catch (ServiceException e)
            {
                var contentResult = new ContentResult { Content = e.Error?.Message, StatusCode = (int)e.StatusCode };

                return contentResult;
            }
        }
    }
}
