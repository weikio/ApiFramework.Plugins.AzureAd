using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Weikio.ApiFramework.Plugins.AzureAD.Users
{

    public class ExtensionPropertyValue<T>
    {
        public string ObjectType { get; set; }

        public string ObjectId { get; set; }

        public string PropertyName { get; set; }

        public T Value { get; set; }

        public ExtensionPropertyValue<string> ToStringValue()
        {
            return new ExtensionPropertyValue<string>()
            {
                ObjectType = ObjectType,
                ObjectId = ObjectId,
                PropertyName = PropertyName,
                Value = Value?.ToString()
            };
        }
    }
}
