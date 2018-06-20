using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using NLog.Web;
using SQLCompare.UI.WebServer;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using LoggingRule = NLog.Config.LoggingRule;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SQLCompare.UI
{
    /// <summary>
    /// SQLCompare UI application, providing the WebServer
    /// </summary>
    public static class Program
    {
        private const short WebServerPort = 5000;

        /// <summary>
        /// Entry point of the SQLCompare UI application
        /// </summary>
        public static void Main()
        {
            var appGlobals = new AppGlobals();

            // NLog: setup the logger first to catch all errors
            var loggerConfig = new LoggingConfiguration();

            // The targets to write to
            var consoleTarget = new ColoredConsoleTarget("Console")
            {
                Layout = appGlobals.LoggerLayout
            };
            loggerConfig.AddTarget(consoleTarget);

            var fileTarget = new FileTarget("File")
            {
                Layout = appGlobals.LoggerLayout,
                FileName = appGlobals.LoggerFile,
            };
            loggerConfig.AddTarget(fileTarget);

            // Rules to map from logger name to target
            var aspNetCoreRule = new LoggingRule
            {
                LoggerNamePattern = "Microsoft.AspNetCore.*",
                Final = true,
            };
            aspNetCoreRule.SetLoggingLevels(NLog.LogLevel.Trace, NLog.LogLevel.Debug);
            loggerConfig.LoggingRules.Add(aspNetCoreRule);

            var allRule = new LoggingRule
            {
                LoggerNamePattern = "*"
            };
            allRule.SetLoggingLevels(NLog.LogLevel.Trace, NLog.LogLevel.Fatal);
            allRule.Targets.Add(consoleTarget);
            allRule.Targets.Add(fileTarget);
            loggerConfig.LoggingRules.Add(allRule);

            InternalLogger.LogLevel = NLog.LogLevel.Info;
            InternalLogger.LogToConsole = true;

            LogManager.Configuration = loggerConfig;

            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
                BuildWebHost().Run();
            }
            catch (Exception ex)
            {
                // NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        private static IWebHost BuildWebHost()
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, WebServerPort, listenOptions =>
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
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog()
                .Build();
            return host;
        }

        private static int? GetFreePort(int start, int end)
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpEndPoints = properties.GetActiveTcpListeners();

            var usedPorts = tcpEndPoints.Select(p => p.Port).ToList();

            for (var port = start; port < end; port++)
            {
                if (!usedPorts.Contains(port))
                {
                    return port;
                }
            }

            return null;
        }
    }
}
