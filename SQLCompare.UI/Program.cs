using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using SQLCompare.UI.WebServer;

namespace SQLCompare.UI
{
    public static class Program
    {
        private const short WebServerPort = 5000;

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
                //NLog: catch setup errors
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
                                //other settings???
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
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();

            List<int> usedPorts = tcpEndPoints.Select(p => p.Port).ToList<int>();

            for (int port = start; port < end; port++)
            {
                if (!usedPorts.Contains(port))
                    return port;
            }
            return null;
        }
    }
}
