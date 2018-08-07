using System;
using System.IO;
using SQLCompare.Core.Interfaces;

namespace SQLCompare.UI
{
    /// <summary>
    /// Global configuration of the application
    /// </summary>
    internal class AppGlobals : IAppGlobals
    {
        /// <inheritdoc/>
        public string CompanyName => "TiCodeX SA";

        /// <inheritdoc/>
        public string ProductName => "SQL Compare";

        /// <inheritdoc/>
        public bool IsDevelopment
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        /// <inheritdoc/>
        public int StartPortRange => 5000;

        /// <inheritdoc/>
        public int EndPortRange => 6000;

        /// <inheritdoc/>
        public string AuthorizationHeaderName => "CustomAuthToken";

        /// <inheritdoc/>
        public string AppSettingsFullFilename => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "SqlCompare",
            "Config.conf");

        /// <inheritdoc/>
        public string LoggerLayout =>
            "${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}";

        /// <inheritdoc/>
        public string LoggerFile => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "SqlCompare",
            "log",
            @"SqlCompare-${shortdate}-service.log");

        /// <inheritdoc/>
        public int LoggerMaxArchiveFiles => 9;
    }
}
