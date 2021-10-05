using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Weikio.ApiFramework.Plugins.AzureAD.Users;

namespace Weikio.ApiFramework.Plugins.AzureAD.Groups
{
    /// <summary>
    /// Requires: GroupMember.ReadWrite.All, User.Read.All
    /// </summary>
    public class UserGroupApi
    {
        public AzureAdOptions Configuration { get; set; }

        [HttpPost]
        public async Task<ActionResult> AddUserToGroup(string user, string group)
        {
            try
            {
                var userService = new UserService(Configuration);
                var userDetails = await userService.GetUser(user);
                
                var groupDetails = await GetGroup(group);

                var client = await GraphServiceClientFactory.GetGraphClient(Configuration);

                await client.Groups[groupDetails.Id].Members.References.Request().AddAsync(userDetails);
            }
            catch (ServiceException e)
            {     
                var contentResult = new ContentResult { Content = e.Error?.Message, StatusCode = (int)e.StatusCode };

                return contentResult;
            }

            return new OkResult();
        }
        
        [HttpDelete]
        public async Task<ActionResult> RemoveUserFromGroup(string user, string group)
        {
            try
            {
                var userService = new UserService(Configuration);
                var userDetails = await userService.GetUser(user);
                
                var groupDetails = await GetGroup(group);

                var client = await GraphServiceClientFactory.GetGraphClient(Configuration);

                await client.Groups[groupDetails.Id].Members[userDetails.Id].Reference.Request().DeleteAsync();
            }
            catch (ServiceException e)
            {     
                var contentResult = new ContentResult { Content = e.Error?.Message, StatusCode = (int)e.StatusCode };

                return contentResult;
            }

            return new OkResult();
        }
        
        public async Task<ActionResult<List<GroupDto>>> GetUserGroups(string user)
        {
            try
            {
                var result = await GetGroups(user);

                return result;
            }
            catch (ServiceException e)
            {
                var contentResult = new ContentResult { Content = e.Error?.Message, StatusCode = (int)e.StatusCode };

                return contentResult;
            }
        }
        
        public async Task<ActionResult<List<GroupDto>>> GetUserSecurityGroups(string user)
        {
            try
            {
                var allGroups = await GetGroups(user);

                var result = allGroups.Where(x => x.IsSecurity).ToList();

                return result;
            }
            catch (ServiceException e)
            {
                var contentResult = new ContentResult { Content = e.Error?.Message, StatusCode = (int)e.StatusCode };

                return contentResult;
            }
        }

        private async Task<List<GroupDto>> GetGroups(string user)
        {
            var graphServiceClient = await GraphServiceClientFactory.GetGraphClient(Configuration);

            var result = new List<GroupDto>();
            var groups = await graphServiceClient.Users[user].MemberOf.Request().GetAsync();

            while (groups.Count > 0)
            {
                foreach (var grp in groups.OfType<Group>())
                {
                    result.Add(new GroupDto(new Guid(grp.Id), grp.DisplayName, grp.SecurityEnabled.GetValueOrDefault()));
                }
                if (groups.NextPageRequest != null)
                {
                    groups = await groups.NextPageRequest.GetAsync();
                }
                else
                {
                    break;
                }
            }
            
            return result;
        }
        
        private async Task<Group> GetGroup(string group)
        {
            var graphServiceClient = await GraphServiceClientFactory.GetGraphClient(Configuration);

            Group result;

            if (Guid.TryParse(group, out _))
            {
                // Get the group by guid (the default)
                result = await graphServiceClient.Groups[@group].Request().GetAsync();
            }
            else
            {
                var req = await graphServiceClient.Groups.Request().Filter($"displayName eq '{group}'").GetResponseAsync();
                
                var objs = await req.GetResponseObjectAsync();
                
                result = objs.Value.FirstOrDefault();
            }

            if (result == null)
            {
                throw new ServiceException(new Error(), null, HttpStatusCode.NotFound);
            }

            return result;
        }
    }
}
