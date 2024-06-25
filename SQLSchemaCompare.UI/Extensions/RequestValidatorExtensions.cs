namespace TiCodeX.SQLSchemaCompare.UI.Extensions
{
    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// Defines the RequestValidator extension of the IApplicationBuilder
    /// </summary>
    public static class RequestValidatorExtensions
    {
        /// <summary>
        /// Add the RequestValidator middleware to the application's request pipeline
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance</param>
        /// <returns>The same IApplicationBuilder instance</returns>
        public static IApplicationBuilder UseRequestValidator(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestValidatorMiddleware>();

            return app;
        }
    }
}
