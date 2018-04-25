using Microsoft.AspNetCore.Builder;
using SQLCompare.UI.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SQLCompare.UI.Extensions

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
