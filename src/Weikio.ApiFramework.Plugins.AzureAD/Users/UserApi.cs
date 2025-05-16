using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;

namespace Weikio.ApiFramework.Plugins.AzureAD.Users
{
    /// <summary>
    /// Requires: User.ReadWrite.All, User.Invite.All
    /// </summary>
    public class UserApi
    {
        public AzureAdOptions Configuration { get; set; }

        public async Task<ActionResult<UserDto>> GetUser(string user)
        {
            if (string.IsNullOrWhiteSpace(user))
            {
                return new BadRequestResult();
            }

            try
            {
                var userService = new UserService(Configuration);
                var userDetails = await userService.GetUser(user);

                var result = new UserDto(new Guid(userDetails.Id), userDetails.UserPrincipalName, userDetails.Mail, userDetails.DisplayName,
                    userDetails.GivenName,
                    userDetails.Surname, userDetails.JobTitle, userDetails.Department);

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
            if (string.IsNullOrWhiteSpace(email))
            {
                return new BadRequestResult();
            }

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

                var userService = new UserService(Configuration);
                var userDetails = await userService.GetUser(user.Id);

                var result = new UserDto(new Guid(userDetails.Id), userDetails.UserPrincipalName, userDetails.Mail, userDetails.DisplayName,
                    userDetails.GivenName,
                    userDetails.Surname, userDetails.JobTitle, userDetails.Department);

                return result;
            }
            catch (ServiceException e)
            {
                var contentResult = new ContentResult { Content = e.Error?.Message, StatusCode = (int)e.StatusCode };

                return contentResult;
            }
        }

        public async Task<ActionResult<UserDto>> UpdateUserDepartment(string user, string department)
        {
            if (string.IsNullOrWhiteSpace(user))
            {
                return new BadRequestResult();
            }

            try
            {
                var userService = new UserService(Configuration);

                var changedValues = new User();

                if (!string.IsNullOrEmpty(department))
                {
                    changedValues.Department = department;
                }
                else
                {
                    // this is the only way to clear string values: 
                    // https://stackoverflow.com/questions/38249131/how-to-clear-a-field-using-using-microsoft-graph-net-client-library/38704088#38704088

                    changedValues.AdditionalData = new Dictionary<string, object>()
                    {
                        { "department", null }
                    };
                }

                await userService.UpdateUser(user, changedValues);

                var updatedUser = await userService.GetUser(user);

                var result = new UserDto(new Guid(updatedUser.Id), updatedUser.UserPrincipalName, updatedUser.Mail, updatedUser.DisplayName,
                        updatedUser.GivenName,
                        updatedUser.Surname, updatedUser.JobTitle, updatedUser.Department);

                return result;
            }
            catch (ServiceException e)
            {
                var contentResult = new ContentResult { Content = e.Error?.Message, StatusCode = (int)e.StatusCode };

                return contentResult;
            }
        }

        public async Task<ActionResult<UserDto>> UpdateUserGivenNameSurname(string user, string givenName, string surname)
        {
            if (string.IsNullOrWhiteSpace(user))
            {
                return new BadRequestResult();
            }

            try
            {
                var userService = new UserService(Configuration);

                var changedValues = new User();

                var additionalData = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(givenName))
                {
                    changedValues.GivenName = givenName;
                }
                else
                {
                    additionalData["givenName"] = null;
                }

                if (!string.IsNullOrEmpty(surname))
                {
                    changedValues.Surname = surname;
                }
                else
                {
                    additionalData["surname"] = null;
                }

                if (additionalData.Any())
                {
                    changedValues.AdditionalData = additionalData;
                }

                await userService.UpdateUser(user, changedValues);

                var updatedUser = await userService.GetUser(user);

                var result = new UserDto(new Guid(updatedUser.Id), updatedUser.UserPrincipalName, updatedUser.Mail, updatedUser.DisplayName,
                        updatedUser.GivenName,
                        updatedUser.Surname, updatedUser.JobTitle, updatedUser.Department);

                return result;
            }
            catch (ServiceException e)
            {
                var contentResult = new ContentResult { Content = e.Error?.Message, StatusCode = (int)e.StatusCode };

                return contentResult;
            }
        }

        public async Task<ActionResult<UserDto>> CreateInvititation(string email, bool sendInvitationMessage,
            string inviteRedirectUrl, string invitationLanguage, string customInvitationMessage)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(inviteRedirectUrl))
            {
                return new BadRequestResult();
            }

            try
            {
                var client = await GraphServiceClientFactory.GetGraphClient(Configuration);

                var invitation = new Invitation()
                {
                    InvitedUserEmailAddress = email, InviteRedirectUrl = inviteRedirectUrl, SendInvitationMessage = sendInvitationMessage,
                    InvitedUserMessageInfo = new InvitedUserMessageInfo()
                };

                if (!string.IsNullOrWhiteSpace(invitationLanguage))
                {
                    invitation.InvitedUserMessageInfo.MessageLanguage = invitationLanguage;
                }

                if (!string.IsNullOrWhiteSpace(customInvitationMessage))
                {
                    invitation.InvitedUserMessageInfo.CustomizedMessageBody = customInvitationMessage;
                }

                var invitationResult = await client.Invitations.Request().AddAsync(invitation);

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

        [HttpPut]
        public async Task<ActionResult<ExtensionPropertyValue<string>>> SetExtensionPropertyString(string user, string propertyName, string value)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(propertyName))
            {
                return new BadRequestResult();
            }

            try
            {
                var userService = new UserService(Configuration);

                var propertyValue = await userService.SetExtensionPropertyValue(user, propertyName, value);

                return propertyValue.ToStringValue();
            }
            catch (ServiceException e)
            {
                var contentResult = new ContentResult { Content = e.Error?.Message, StatusCode = (int)e.StatusCode };

                return contentResult;
            }
        }

        public async Task<ActionResult<ExtensionPropertyValue<string>>> GetExtensionPropertyString(string user, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(propertyName))
            {
                return new BadRequestResult();
            }

            try
            {
                var userService = new UserService(Configuration);

                var propertyValue = await userService.GetExtensionPropertyValue(user, propertyName);

                return propertyValue.ToStringValue();
            }
            catch (ServiceException e)
            {
                var contentResult = new ContentResult { Content = e.Error?.Message, StatusCode = (int)e.StatusCode };

                return contentResult;
            }
        }
    }
}
