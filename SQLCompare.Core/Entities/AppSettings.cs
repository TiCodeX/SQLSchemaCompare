using System.Collections.Generic;
using SQLCompare.Core.Enums;

namespace SQLCompare.Core.Entities
{
    /// <summary>
    /// User configurable settings of the application
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Gets or sets the language of the application
        /// </summary>
        public Language Language { get; set; } = Language.English;

        /// <summary>
        /// Gets or sets the minimum log level
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// Gets the recently opened projects
        /// </summary>
        public List<string> RecentProjects { get; } = new List<string>();
    }
}
