using System;
using System.Collections.Generic;
using System.Text;

namespace SQLCompare.Core.Interfaces
{
    /// <summary>
    /// Defines a class that provides global constants
    /// </summary>
    public interface IAppGlobals
    {
        /// <summary>
        /// Gets a value indicating whether the solution configuration is in Debug
        /// </summary>
        bool IsDevelopment { get; }

        /// <summary>
        /// Gets the initial port for the range of the WebServer
        /// </summary>
        int StartPortRange { get; }

        /// <summary>
        /// Gets the final port for the range of the WebServer
        /// </summary>
        int EndPortRange { get; }

        /// <summary>
        /// Gets the header attribute name for the authentication
        /// </summary>
        string AuthorizationHeaderName { get; }

        /// <summary>
        /// Gets the full filename of the application settings file
        /// </summary>
        string AppSettingsFullFilename { get; }
    }
}
