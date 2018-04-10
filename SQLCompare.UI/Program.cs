using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace SQLCompare.UI
{
    public static class Program
    {
        private const short WebServerPort = 5000;

        public static void Main()
        {
            try
            {
                //Logger.LogMessage("Starting WebServer...");

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
                    .ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.AddConsole();
                        logging.AddDebug();
                    })
                    .UseStartup<WebServerStartup>()
                    .Build();

                host.Run();
            }
            catch (Exception)
            {
                //Logger.LogError("An error occurred while initializing the Webserver : " + ex.Message);
            }
        }

    }
}
