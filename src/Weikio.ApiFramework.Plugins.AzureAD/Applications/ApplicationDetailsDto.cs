using System;
using System.Collections.Generic;

namespace Weikio.ApiFramework.Plugins.AzureAD.Applications
{
    public class ApplicationDetailsDto : ApplicationDto
    {
        public List<ExtensionPropertyDto> ExtensionProperties { get; set; }
    }
}
