﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Logging;
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
            // NLog: setup the logger first to catch all errors
            var logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
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
                NLog.LogManager.Shutdown();
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

                            listenOptions.UseHttps(new HttpsConnectionAdapterOptions()
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
