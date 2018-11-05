using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SQLCompare.Core.Interfaces;
using SQLCompare.Core.Interfaces.Repository;
using SQLCompare.Core.Interfaces.Services;
using SQLCompare.Infrastructure.DatabaseProviders;
using SQLCompare.Infrastructure.Repository;
using SQLCompare.Infrastructure.SqlScripters;
using SQLCompare.Services;
using SQLCompare.UI.Extensions;
using SQLCompare.UI.Middlewares;

namespace SQLCompare.UI.WebServer
{
    /// <summary>
    /// WebServer configuration class used during the startup
    /// </summary>
    public class WebServerStartup
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
        [Obfuscation(Exclude = true)]
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

            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

            services.AddSingleton<IAppGlobals, AppGlobals>();
            services.AddSingleton<ILocalizationService, LocalizationService>();
            services.AddSingleton<IAppSettingsService, AppSettingsService>();
            services.AddSingleton<IProjectService, ProjectService>();
            services.AddSingleton<ITaskService, TaskService>();
            services.AddSingleton<IAccountService, CustomerAccountService>();
            services.AddSingleton<ICipherService, CipherService>();

            // Repository
            services.AddTransient<IAppSettingsRepository, AppSettingsRepository>();
            services.AddTransient<IProjectRepository, ProjectRepository>();

            // Service
            services.AddTransient<IDatabaseService, DatabaseService>();
            services.AddTransient<IDatabaseCompareService, DatabaseCompareService>();

            // Factory
            services.AddTransient<IDatabaseProviderFactory, DatabaseProviderFactory>();
            services.AddTransient<IDatabaseScripterFactory, DatabaseScripterFactory>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The WebHost application builder</param>
        /// <param name="appGlobals">The application globals</param>
        /// <param name="localizationService">The localization service</param>
        /// <param name="appSettingsService">The app settings service</param>
        /// <param name="loggerFactory">The injected logger factory</param>
        [Obfuscation(Exclude = true)]
        public void Configure(
            IApplicationBuilder app,
            IAppGlobals appGlobals,
            ILocalizationService localizationService,
            IAppSettingsService appSettingsService,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger(nameof(WebServerStartup));

            var appSettings = appSettingsService.GetAppSettings();
            Utility.SetLoggingLevel(appSettings.LogLevel);
            localizationService.SetLanguage(appSettings.Language);

            logger.LogDebug("Configuring WebHost...");
            logger.LogDebug($"LogLevel => {appSettings.LogLevel}");
            logger.LogDebug($"Language => {appSettings.Language}");

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
                    FileProvider = new HyphenFriendlyEmbeddedFileProvider(
                        new EmbeddedFileProvider(Assembly.GetExecutingAssembly(), "SQLCompare.UI.wwwroot"),
                        logger)
                });
                app.UseRequestValidator();
            }

            app.UseMvc();
        }
    }
}
