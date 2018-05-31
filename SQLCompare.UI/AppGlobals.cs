using SQLCompare.Core.Interfaces;
using System;
using System.IO;

namespace SQLCompare.UI
{
    /// <summary>
    /// Global configuration of the application
    /// </summary>
    internal class AppGlobals : IAppGlobals
    {
        /// <inheritdoc/>
        public string CompanyName => "Ticodex SA";

        /// <inheritdoc/>
        public string ProductName => "SQLCompare";

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
        public string AppSettingsFullFilename => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SqlCompare", "Config.conf");
    }
}
