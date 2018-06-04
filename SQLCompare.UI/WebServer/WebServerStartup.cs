using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SQLCompare.Core.Interfaces;
using SQLCompare.Core.Interfaces.Repository;
using SQLCompare.Core.Interfaces.Services;
using SQLCompare.Infrastructure.DatabaseProviders;
using SQLCompare.Infrastructure.Repository;
using SQLCompare.Infrastructure.SqlScripters;
using SQLCompare.Services;
using SQLCompare.UI.Extensions;
using SQLCompare.UI.Middlewares;
using System.Reflection;

namespace SQLCompare.UI.WebServer
{
    /// <summary>
    /// WebServer configuration class used during the startup
    /// </summary>
    internal class WebServerStartup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebServerStartup"/> class.
        /// </summary>
        /// <param name="configuration">The WebHost configuration</param>
        public WebServerStartup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the WebHost configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The WebHost service collection</param>
        public static void ConfigureServices(IServiceCollection services)
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

            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

            services.AddSingleton<IAppGlobals, AppGlobals>();
            services.AddSingleton<IAppSettingsService, AppSettingsService>();
            services.AddSingleton<IProjectService, ProjectService>();

            // Repository
            services.AddTransient<IAppSettingsRepository, AppSettingsRepository>();
            services.AddTransient<IProjectRepository, ProjectRepository>();

            // Service
            services.AddTransient<IDatabaseService, DatabaseService>();

            // Factory
            services.AddTransient<IDatabaseProviderFactory, DatabaseProviderFactory>();
            services.AddTransient<IDatabaseScripterFactory, DatabaseScripterFactory>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The WebHost application builder</param>
        public void Configure(IApplicationBuilder app)
        {
            // As we don't have IAppGlobals injected yet, we instantiate it directly.
            AppGlobals appGlobals = new AppGlobals();
            if (appGlobals.IsDevelopment)
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
