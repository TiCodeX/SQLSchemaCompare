using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SQLCompare.UI.Middleware
{
    public static class RequestValidatorExtensions
    {
        public static IApplicationBuilder UseRequestValidator(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestValidatorMiddleware>();

            return app;
        }
    }
}
