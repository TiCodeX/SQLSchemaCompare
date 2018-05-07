using System.Collections.Generic;

namespace SQLCompare.Core.Entities
{
    /// <summary>
    /// User configurable settings of the application
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Gets the recently opened projects
        /// </summary>
        public List<string> RecentProjects { get; } = new List<string>();
    }
}
