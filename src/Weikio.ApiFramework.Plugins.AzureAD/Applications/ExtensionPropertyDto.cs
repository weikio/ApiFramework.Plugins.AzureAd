using System;
using System.Collections.Generic;
using System.Text;

namespace Weikio.ApiFramework.Plugins.AzureAD.Applications
{
    public class ExtensionPropertyDto
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public List<string> TargetObjects { get; set; }
    }
}
