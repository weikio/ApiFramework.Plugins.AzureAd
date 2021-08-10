using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;

namespace Weikio.ApiFramework.Plugins.AzureAD
{
    public class ApiFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ApiFactory> _logger;

        public ApiFactory(ILoggerFactory loggerFactory, ILogger<ApiFactory> logger)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
        }

        public IEnumerable<Type> Create(AzureAdOptions configuration)
        {
            return new List<Type>() { };
        }
    }
}
