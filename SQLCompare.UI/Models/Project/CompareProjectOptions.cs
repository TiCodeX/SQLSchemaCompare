using SQLCompare.Core.Enums;
using SQLCompare.UI.Enums;

namespace SQLCompare.UI.Models.Project
{
    /// <summary>
    /// Model class for the CompareProject page
    /// </summary>
    public class CompareProjectOptions
    {
        /// <summary>
        /// Gets or sets the compare diraction
        /// </summary>
        public CompareDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets the source database type
        /// </summary>
        public DatabaseType SourceDatabaseType { get; set; }

        /// <summary>
        /// Gets or sets the source hostname
        /// </summary>
        public string SourceHostname { get; set; }

        /// <summary>
        /// Gets or sets the source username
        /// </summary>
        public string SourceUsername { get; set; }

        /// <summary>
        /// Gets or sets the source password
        /// </summary>
        public string SourcePassword { get; set; }

        /// <summary>
        /// Gets or sets the source database
        /// </summary>
        public string SourceDatabase { get; set; }

        /// <summary>
        /// Gets or sets the target database type
        /// </summary>
        public DatabaseType TargetDatabaseType { get; set; }

        /// <summary>
        /// Gets or sets the target hostname
        /// </summary>
        public string TargetHostname { get; set; }

        /// <summary>
        /// Gets or sets the target username
        /// </summary>
        public string TargetUsername { get; set; }

        /// <summary>
        /// Gets or sets the target password
        /// </summary>
        public string TargetPassword { get; set; }

        /// <summary>
        /// Gets or sets the target database
        /// </summary>
        public string TargetDatabase { get; set; }
    }
}
