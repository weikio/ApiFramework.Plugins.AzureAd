using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;

namespace Weikio.ApiFramework.Plugins.AzureAD.Users
{
    /// <summary>
    /// Requires: User.Read.All, User.Invite.All
    /// </summary>
    public class UserApi
    {
        public AzureAdOptions Configuration { get; set; }

        public async Task<ActionResult<UserDto>> GetUser(string user)
        {
            try
            {
                var userService = new UserService();
                var userDetails = await userService.GetUser(user, Configuration);

                var result = new UserDto(new Guid(userDetails.Id), userDetails.UserPrincipalName, userDetails.Mail, userDetails.DisplayName,
                    userDetails.GivenName,
                    userDetails.Surname, userDetails.JobTitle);

                return result;
            }
            catch (ServiceException e)
            {
                var contentResult = new ContentResult { Content = e.Error?.Message, StatusCode = (int)e.StatusCode };

                return contentResult;
            }
        }

        public async Task<ActionResult<UserDto>> GetUserByEmail(string email)
        {
            try
            {
                var client = await GraphServiceClientFactory.GetGraphClient(Configuration);

                var req = await client.Users.Request().Filter($"mail eq '{email}'")
                    .GetAsync();

                var user = req.CurrentPage?.FirstOrDefault();

                if (user == null)
                {
                    throw new ServiceException(new Error(), null, HttpStatusCode.NotFound);
                }

                var userService = new UserService();
                var userDetails = await userService.GetUser(user.Id, Configuration);

                var result = new UserDto(new Guid(userDetails.Id), userDetails.UserPrincipalName, userDetails.Mail, userDetails.DisplayName,
                    userDetails.GivenName,
                    userDetails.Surname, userDetails.JobTitle);

                return result;
            }
            catch (ServiceException e)
            {
                var contentResult = new ContentResult { Content = e.Error?.Message, StatusCode = (int)e.StatusCode };

                return contentResult;
            }
        }

        public async Task<ActionResult<UserDto>> CreateInvititation(string email, bool sendInvitationMessage, string inviteRedirectUrl)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(inviteRedirectUrl))
            {
                return new BadRequestResult();
            }

            try
            {
                var client = await GraphServiceClientFactory.GetGraphClient(Configuration);

                var invitationResult = await client.Invitations.Request().AddAsync(new Invitation()
                {
                    InvitedUserEmailAddress = email, InviteRedirectUrl = inviteRedirectUrl, SendInvitationMessage = sendInvitationMessage
                });

                if (invitationResult.InvitedUser == null)
                {
                    throw new ServiceException(new Error() { Message = invitationResult.Status }, null, HttpStatusCode.Conflict);
                }

                var result = new UserDto();
                result.Id = new Guid(invitationResult.InvitedUser.Id);
                result.Email = invitationResult.InvitedUser.Mail;

                return result;
            }
            catch (ServiceException e)
            {
                var contentResult = new ContentResult { Content = e.Error?.Message, StatusCode = (int)e.StatusCode };

                return contentResult;
            }
        }
    }
}
