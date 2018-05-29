using SQLCompare.Core.Enums;

namespace SQLCompare.UI.Models.Project
{
    /// <summary>
    /// Model class for the target provider options page
    /// </summary>
    public class TargetDatabaseProviderOptions
    {
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
