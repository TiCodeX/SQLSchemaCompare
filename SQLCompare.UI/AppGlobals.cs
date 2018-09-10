using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
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
        public string AppSettingsFullFilename
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return Path.Combine(
                        Directory.GetParent(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).FullName,
                        "Config.conf");
                }

                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "SqlCompare",
                    "Config.conf");
            }
        }

        /// <inheritdoc/>
        public string LoggerLayout =>
            "${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}";

        /// <inheritdoc/>
        public string LoggerFile
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return Path.Combine(
                        Directory.GetParent(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).FullName,
                        "log",
                        @"SqlCompare-${shortdate}-service.log");
                }

                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "SqlCompare",
                    "log",
                    @"SqlCompare-${shortdate}-service.log");
            }
        }

        /// <inheritdoc/>
        public int LoggerMaxArchiveFiles => 9;

        /// <inheritdoc/>
        public string ElectronAuthAppId => "queieimiugrepqueieimiucrap";

        /// <inheritdoc/>
        public string ProductCode => "SQLCMP";

#if DEBUG
        /// <inheritdoc/>
        public string LoginEndpoint => $"https://localhost:44349/login?culture={Localization.Culture.Name}&appId={this.ElectronAuthAppId}&product={this.ProductCode}";

        /// <inheritdoc/>
        public string VerifySessionEndpoint => "https://localhost:44349/api/VerifySession";
#else
        /// <inheritdoc/>
        public string LoginEndpoint => $"http://myaccount-ticodex.azurewebsites.net/login?culture={Localization.Culture.Name}&appId={this.ElectronAuthAppId}&product={this.ProductCode}";

        /// <inheritdoc/>
        public string VerifySessionEndpoint => "http://myaccount-ticodex.azurewebsites.net/api/VerifySession";
#endif
    }
}
