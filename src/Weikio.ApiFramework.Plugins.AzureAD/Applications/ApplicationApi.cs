using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;

namespace Weikio.ApiFramework.Plugins.AzureAD.Applications
{
    /// <summary>
    /// Requires: Application.Read.All
    /// </summary>
    public class ApplicationApi
    {
        public AzureAdOptions Configuration { get; set; }

        public async Task<ActionResult<System.Collections.Generic.List<ApplicationDto>>> GetApplications()
        {
            try
            {
                var service = new ApplicationService(Configuration);
                var result = await service.GetApplications();

                return result;
            }
            catch (ServiceException e)
            {
                var contentResult = new ContentResult { Content = e.Error?.Message, StatusCode = (int)e.StatusCode };

                return contentResult;
            }
        }
        
        public async Task<ActionResult<ApplicationDto>> GetApplicationDetails(string applicationId)
        {
            try
            {
                var service = new ApplicationService(Configuration);
                var apps = await service.GetApplications();

                var result = apps.FirstOrDefault(x => string.Equals(x.Id.ToString(), applicationId, StringComparison.InvariantCultureIgnoreCase) || string.Equals(x.AppId.ToString(), applicationId, StringComparison.InvariantCultureIgnoreCase));

                if (result == null)
                {
                    throw new ServiceException(new Error() { Message = "Application not found" }, null, HttpStatusCode.NotFound);
                }
                
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
