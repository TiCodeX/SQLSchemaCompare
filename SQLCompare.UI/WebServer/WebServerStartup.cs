using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SQLCompare.Core;
using SQLCompare.UI.Extensions;
using SQLCompare.UI.Middlewares;
using System.Reflection;

namespace SQLCompare.UI.WebServer
{
    internal class WebServerStartup
    {
        public WebServerStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new RequireHttpsAttribute());
            });
            services.Configure<RequestValidatorSettings>(options =>
            {
                options.AllowedRequestGuid = "prova";
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (AppGlobal.IsDevelopment)
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseStaticFiles();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new HyphenFriendlyEmbeddedFileProvider(new EmbeddedFileProvider(Assembly.GetExecutingAssembly(), "SQLCompare.UI.wwwroot"))
                });
                app.UseRequestValidator();
            }

            app.UseMvc();
        }
    }
}
