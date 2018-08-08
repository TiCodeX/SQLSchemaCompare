using System;
using Microsoft.AspNetCore.Hosting;
using NLog;
using SQLCompare.UI.WebServer;

namespace SQLCompare.UI
{
    /// <summary>
    /// SQLCompare UI application, providing the WebServer
    /// </summary>
    public static class Program
    {
        // TODO: receive port from electron (args) or find an open port itself and give it back to electron
        private const short WebServerPort = 5000;

        /// <summary>
        /// Entry point of the SQLCompare UI application
        /// </summary>
        public static void Main()
        {
            Utility.ConfigureLogger();

            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                logger.Info(string.Empty);
                logger.Info("Starting WebHost Service...");
                CreateWebHostBuilder(null)
                    .Build()
                    .Run();
            }
            catch (Exception ex)
            {
                // NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                logger.Debug("Closing Application...");

                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return Utility.CreateWebHostBuilder(args, WebServerPort);
        }
    }
}
