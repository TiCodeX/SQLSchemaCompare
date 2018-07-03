using SQLCompare.Core.Enums;
using System.Collections.Generic;

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
        /// Gets the recently opened projects
        /// </summary>
        public List<string> RecentProjects { get; } = new List<string>();
    }
}
