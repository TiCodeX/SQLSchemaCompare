using System;
using System.IO;
using SQLCompare.Core.Interfaces;
using SQLCompare.Services;

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

        /// <inheritdoc/>
        public string ElectronAuthAppId => "queieimiugrepqueieimiucrap";

        /// <inheritdoc/>
        public string ProductCode => "SQLCMP";

        /// <inheritdoc/>
        public string LoginEndpoint => $"https://localhost:44349/login?culture={Localization.Culture.Name}&appId={this.ElectronAuthAppId}&product={this.ProductCode}";

        /// <inheritdoc/>
        public string VerifySessionEndpoint => "http://localhost:7071/api/VerifySession";
    }
}
