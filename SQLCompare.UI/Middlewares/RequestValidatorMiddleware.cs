using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SQLCompare.Core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SQLCompare.UI.Middlewares
{
    public class RequestValidatorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestValidatorMiddleware> _logger;
        private readonly RequestValidatorSettings _options;

        public RequestValidatorMiddleware(RequestDelegate next, ILogger<RequestValidatorMiddleware> logger, IOptions<RequestValidatorSettings> options)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            string authToken = context.Request.Headers[AppGlobal.AuthorizationHeaderName];
            string userAgent = context.Request.Headers["User-Agent"];
            if (string.Equals(authToken, _options.AllowedRequestGuid, StringComparison.Ordinal) &&
                string.Equals(userAgent, _options.AllowedRequestAgent, StringComparison.Ordinal))
            {

                await _next.Invoke(context).ConfigureAwait(false);
            }
            else
            {
                _logger.LogError($"Request refused. Token:{authToken}; UserAgent:{userAgent}");
                context.Response.Body = new MemoryStream(0);
            }


        }
    }
}
