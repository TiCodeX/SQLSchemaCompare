﻿namespace TiCodeX.SQLSchemaCompare.UI.Middlewares
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces;

    /// <summary>
    /// Middleware that validate the header of each request
    /// </summary>
    public class RequestValidatorMiddleware
    {
        /// <summary>
        /// The next request
        /// </summary>
        private readonly RequestDelegate next;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The options
        /// </summary>
        private readonly RequestValidatorSettings options;

        /// <summary>
        /// The app globals
        /// </summary>
        private readonly IAppGlobals appGlobals;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestValidatorMiddleware"/> class.
        /// </summary>
        /// <param name="next">The RequestDelegate instance</param>
        /// <param name="loggerFactory">The injected Logger factory</param>
        /// <param name="options">Configuration options for the validation</param>
        /// <param name="appGlobals">The injected application global constants</param>
        public RequestValidatorMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IOptions<RequestValidatorSettings> options, IAppGlobals appGlobals)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.next = next;
            this.logger = loggerFactory.CreateLogger(nameof(RequestValidatorMiddleware));
            this.options = options.Value;
            this.appGlobals = appGlobals;
        }

        /// <summary>
        /// Method that gets invoked when the middleware is executed
        /// </summary>
        /// <param name="context">The HttpContext of the current request</param>
        /// <returns>The next task</returns>
        [Obfuscation(Exclude = true)]
        public Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return InvokeInternal(context);

            async Task InvokeInternal(HttpContext context)
            {
                string authToken = context.Request.Headers[this.appGlobals.AuthorizationHeaderName];
                string userAgent = context.Request.Headers["User-Agent"];

                if ((string.IsNullOrEmpty(this.options.AllowedRequestGuid) || authToken == this.options.AllowedRequestGuid) &&
                    (string.IsNullOrEmpty(this.options.AllowedRequestAgent) || userAgent == this.options.AllowedRequestAgent))
                {
                    await this.next.Invoke(context).ConfigureAwait(false);
                }
                else
                {
                    this.logger.LogError($"Request refused. Token:{authToken}; UserAgent:{userAgent}");
                    context.Response.Body = new MemoryStream(0);
                }
            }
        }
    }
}
