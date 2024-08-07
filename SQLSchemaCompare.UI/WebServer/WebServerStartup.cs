﻿namespace TiCodeX.SQLSchemaCompare.UI.WebServer
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// WebServer configuration class used during the startup
    /// </summary>
    public class WebServerStartup
    {
        /// <summary>
        /// The allowed request guid
        /// </summary>
        private const string AllowedRequestGuid = "d6e9b4c2-25d3-a625-e9a6-2135f3d2f809";

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
                options.AllowedRequestGuid = AllowedRequestGuid;
            });

            services.AddRazorPages().AddNewtonsoftJson(options =>
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

            // Utilities
            services.AddTransient<IDatabaseMapper, DatabaseMapper>();
            services.AddTransient<IDatabaseFilter, DatabaseFilter>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The WebHost application builder</param>
        /// <param name="appGlobals">The application globals</param>
        /// <param name="localizationService">The localization service</param>
        /// <param name="appSettingsService">The app settings service</param>
        /// <param name="loggerFactory">The injected logger factory</param>
        public static void Configure(
            IApplicationBuilder app,
            IAppGlobals appGlobals,
            ILocalizationService localizationService,
            IAppSettingsService appSettingsService,
            ILoggerFactory loggerFactory)
        {
            if (appGlobals == null)
            {
                throw new ArgumentNullException(nameof(appGlobals));
            }

            if (localizationService == null)
            {
                throw new ArgumentNullException(nameof(localizationService));
            }

            if (appSettingsService == null)
            {
                throw new ArgumentNullException(nameof(appSettingsService));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            var logger = loggerFactory.CreateLogger(nameof(WebServerStartup));

            var appSettings = appSettingsService.GetAppSettings();
            Utility.SetLoggingLevel(appSettings.LogLevel);
            localizationService.SetLanguage(appSettings.Language);

            logger.LogDebug("Configuring WebHost...");
            logger.LogDebug($"LogLevel => {appSettings.LogLevel}");
            logger.LogDebug($"Language => {appSettings.Language}");

            app.UseStaticFiles();
            app.UseExceptionHandler("/ErrorPage");
            app.UseRequestValidator();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
