using SQLCompare.Core.Enums;

namespace SQLCompare.UI.Models.Project
{
    /// <summary>
    /// Model class for the CompareProject page
    /// </summary>
    public class CompareProjectOptions
    {
        /// <summary>
        /// Gets or sets the database type
        /// </summary>
        public DatabaseType SourceDatabaseType { get; set; }

        /// <summary>
        /// Gets or sets the hostname
        /// </summary>
        public string SourceHostname { get; set; }

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string SourceUsername { get; set; }

        /// <summary>
        /// Gets or sets the passowrd
        /// </summary>
        public string SourcePassword { get; set; }

        /// <summary>
        /// Gets or sets the database type
        /// </summary>
        public DatabaseType TargetDatabaseType { get; set; }

        /// <summary>
        /// Gets or sets the hostname
        /// </summary>
        public string TargetHostname { get; set; }

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string TargetUsername { get; set; }

        /// <summary>
        /// Gets or sets the passowrd
        /// </summary>
        public string TargetPassword { get; set; }
    }
}
