namespace TiCodeX.SQLSchemaCompare.UI.WebServer;

using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using NLog.Web;
using LogLevel = TiCodeX.SQLSchemaCompare.Core.Enums.LogLevel;

/// <summary>
/// Utility functions
/// </summary>
internal static class Utility
{
    /// <summary>
    /// Creates the web host builder
    /// </summary>
    /// <param name="args">The arguments</param>
    /// <returns>The web host builder</returns>
    internal static IHostBuilder CreateHostBuilder(string[] args)
    {
        if (args == null || args.Length < 1 || !int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var webPort))
        {
            throw new ArgumentException("Wrong command line arguments");
        }

        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(options =>
                {
                    options.ListenLocalhost(webPort, listenOptions =>
                    {
                        using var resourceStream = typeof(Utility).Assembly.GetManifestResourceStream("TiCodeX.SQLSchemaCompare.UI.certificate.pfx");
                        using var memoryStream = new MemoryStream();
                        resourceStream.CopyTo(memoryStream);

                        listenOptions.UseHttps(new HttpsConnectionAdapterOptions
                        {
                            ServerCertificate = X509CertificateLoader.LoadPkcs12(memoryStream.ToArray(), "test1234"),
                            SslProtocols = SslProtocols.Tls12,

                            // other settings???
                        });
                    });
                });

                webBuilder.UseStartup<WebServerStartup>();
            })
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            })
            .UseNLog();
    }

    /// <summary>
    /// Configures the logger
    /// </summary>
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Will be disposed by NLog")]
    internal static void ConfigureLogger()
    {
        // As we don't have IAppGlobals injected yet, we instantiate it directly
        var appGlobals = new AppGlobals();

        // NLog: setup the logger first to catch all errors
        var config = new LoggingConfiguration();

        // Ignore all Microsoft.AspNetCore low level logs
        var aspNetCoreRule = new LoggingRule
        {
            LoggerNamePattern = "Microsoft.AspNetCore.*",
            Final = true,
        };
        aspNetCoreRule.SetLoggingLevels(NLog.LogLevel.Trace, NLog.LogLevel.Info);
        config.LoggingRules.Add(aspNetCoreRule);

        // Set the NLog internal logger level
        InternalLogger.LogLevel = NLog.LogLevel.Info;

        // Configure the targets to write to
        config.AddTarget(new FileTarget("File")
        {
            Layout = appGlobals.LoggerLayout,
            FileName = appGlobals.LoggerFile,
            MaxArchiveFiles = appGlobals.LoggerMaxArchiveFiles,
        });
        config.AddTarget(new ColoredConsoleTarget("Console")
        {
            Layout = appGlobals.LoggerLayout,
        });

        // Set the NLog internal logger to log into the console
        InternalLogger.LogToConsole = true;

        // Configure the logging rule to log everything into all targets
        var allRule = new LoggingRule
        {
            LoggerNamePattern = "*",
        };
        allRule.SetLoggingLevels(NLog.LogLevel.Info, NLog.LogLevel.Fatal);
        foreach (var target in config.ConfiguredNamedTargets)
        {
            allRule.Targets.Add(target);
        }

        config.LoggingRules.Add(allRule);

        // Apply the configuration
        LogManager.Configuration = config;
    }

    /// <summary>
    /// Sets the NLog logging level
    /// </summary>
    /// <param name="logLevel">The desired log level</param>
    internal static void SetLoggingLevel(LogLevel logLevel)
    {
        if (LogManager.Configuration == null)
        {
            return;
        }

        var logConfig = LogManager.Configuration.LoggingRules.FirstOrDefault(x => x.LoggerNamePattern == "*");
        var nlogLevel = logLevel switch
        {
            LogLevel.Trace => NLog.LogLevel.Trace,
            LogLevel.Debug => NLog.LogLevel.Debug,
            LogLevel.Info => NLog.LogLevel.Info,
            LogLevel.Warning => NLog.LogLevel.Warn,
            LogLevel.Error => NLog.LogLevel.Error,
            LogLevel.Critical => NLog.LogLevel.Fatal,
            LogLevel.None => NLog.LogLevel.Off,
            _ => throw new NotSupportedException("Unknown log level"),
        };
        logConfig?.SetLoggingLevels(nlogLevel, NLog.LogLevel.Fatal);

        // Trigger the reconfiguration
        LogManager.ReconfigExistingLoggers();
    }
}
