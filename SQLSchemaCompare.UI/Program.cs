namespace TiCodeX.SQLSchemaCompare.UI
{
    using System;
    using System.Globalization;
    using Microsoft.AspNetCore.Hosting;
    using NLog;
    using TiCodeX.SQLSchemaCompare.UI.WebServer;

    /// <summary>
    /// SQLSchemaCompare UI application, providing the WebServer
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Entry point of the SQLSchemaCompare UI application
        /// </summary>
        /// <param name="args">The command line arguments</param>
        public static void Main(string[] args)
        {
            Utility.ConfigureLogger();

            var logger = LogManager.GetLogger(nameof(Program));
            try
            {
                logger.Info("===============================================================", CultureInfo.InvariantCulture);
                logger.Info("Starting WebHost Service...", CultureInfo.InvariantCulture);
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
                logger.Debug("Closing Application...", CultureInfo.InvariantCulture);

                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        /// <summary>
        /// Create the web host builder
        /// </summary>
        /// <param name="args">The args</param>
        /// <returns>The web host builder</returns>
        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return Utility.CreateWebHostBuilder(args);
        }
    }
}
