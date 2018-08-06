using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using NLog.Web;
using LogLevel = SQLCompare.Core.Enums.LogLevel;

namespace SQLCompare.UI.WebServer
{
    /// <summary>
    /// Utility functions
    /// </summary>
    internal static class Utility
    {
        /// <summary>
        /// Creates the web host builder
        /// </summary>
        /// <param name="args">The arguments</param>
        /// <param name="port">The port</param>
        /// <returns>The web host builder</returns>
        internal static IWebHostBuilder CreateWebHostBuilder(string[] args, int port)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, port, listenOptions =>
                    {
                        using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SQLCompare.UI.certificate.pfx"))
                        using (var memoryStream = new MemoryStream())
                        {
                            resourceStream.CopyTo(memoryStream);

                            listenOptions.UseHttps(new HttpsConnectionAdapterOptions
                            {
                                ServerCertificate = new X509Certificate2(memoryStream.ToArray(), "test1234"),
                                SslProtocols = SslProtocols.Tls12

                                // other settings???
                            });
                        }
                    });
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<WebServerStartup>()
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
            aspNetCoreRule.SetLoggingLevels(NLog.LogLevel.Trace, NLog.LogLevel.Debug);
            config.LoggingRules.Add(aspNetCoreRule);

            // Set the NLog internal logger level
            InternalLogger.LogLevel = NLog.LogLevel.Info;

            // Configure the targets to write to
            var fileTarget = new FileTarget("File")
            {
                Layout = appGlobals.LoggerLayout,
                FileName = appGlobals.LoggerFile,
            };
            config.AddTarget(fileTarget);

            if (appGlobals.IsDevelopment)
            {
                // Configure the console/debugger logger only for development
                var consoleTarget = new ColoredConsoleTarget("Console")
                {
                    Layout = appGlobals.LoggerLayout
                };
                config.AddTarget(consoleTarget);

                /*var debugTarget = new DebuggerTarget("Debugger")
                {
                    Layout = appGlobals.LoggerLayout
                };
                config.AddTarget(debugTarget);*/

                // Set the NLog internal logger to log into the console
                InternalLogger.LogToConsole = true;
            }

            // Configure the logging rule to log everything into all targets
            var allRule = new LoggingRule
            {
                LoggerNamePattern = "*"
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
            var logConfig = LogManager.Configuration.LoggingRules.First(x => x.LoggerNamePattern == "*");

            var nlogLevel = NLog.LogLevel.Info;
            switch (logLevel)
            {
                case LogLevel.Trace:
                    nlogLevel = NLog.LogLevel.Trace;
                    break;
                case LogLevel.Debug:
                    nlogLevel = NLog.LogLevel.Debug;
                    break;
                case LogLevel.Info:
                    nlogLevel = NLog.LogLevel.Info;
                    break;
                case LogLevel.Warning:
                    nlogLevel = NLog.LogLevel.Warn;
                    break;
                case LogLevel.Error:
                    nlogLevel = NLog.LogLevel.Error;
                    break;
                case LogLevel.Critical:
                    nlogLevel = NLog.LogLevel.Fatal;
                    break;
                case LogLevel.None:
                    nlogLevel = NLog.LogLevel.Off;
                    break;
            }

            logConfig.SetLoggingLevels(nlogLevel, NLog.LogLevel.Fatal);

            // Trigger the reconfiguration
            LogManager.ReconfigExistingLoggers();
        }
    }
}
