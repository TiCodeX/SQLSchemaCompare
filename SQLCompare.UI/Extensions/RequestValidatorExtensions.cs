using Microsoft.AspNetCore.Builder;
using SQLCompare.UI.Middlewares;

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
