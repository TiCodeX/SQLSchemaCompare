namespace TiCodeX.SQLSchemaCompare.Infrastructure.Extensions;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions for the IServiceCollection
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Registers all services.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

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

        return services;
    }
}
