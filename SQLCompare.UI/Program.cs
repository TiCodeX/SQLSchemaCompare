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
        /// <summary>
        /// Entry point of the SQLCompare UI application
        /// </summary>
        /// <param name="args">The command line arguments</param>
        public static void Main(string[] args)
        {
            Utility.ConfigureLogger();

            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                logger.Info(string.Empty);
                logger.Info("Starting WebHost Service...");
                CreateWebHostBuilder(args)
                    .Build()
                    .Run();
            }
            catch (Exception ex)
            {
                // NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
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
            return Utility.CreateWebHostBuilder(args);
        }
    }
}
