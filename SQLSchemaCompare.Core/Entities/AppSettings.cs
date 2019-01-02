using System.Collections.Generic;
using TiCodeX.SQLSchemaCompare.Core.Enums;

namespace TiCodeX.SQLSchemaCompare.Core.Entities
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

        /// <summary>
        /// Gets or sets the saved login session
        /// </summary>
        public string Session { get; set; }

        /// <summary>
        /// Gets or sets the version of the application when the feedback has been sent
        /// </summary>
        public string FeedbackSent { get; set; }
    }
}
