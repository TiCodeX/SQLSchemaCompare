using TiCodeX.SQLSchemaCompare.Core.Entities.Project;
using TiCodeX.SQLSchemaCompare.Core.Enums;

namespace TiCodeX.SQLSchemaCompare.UI.Models.Project
{
    /// <summary>
    /// Model class for the Project page
    /// </summary>
    public class CompareProjectOptions
    {
        /// <summary>
        /// Gets or sets the compare direction
        /// </summary>
        public CompareDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets the database type
        /// </summary>
        public DatabaseType DatabaseType { get; set; }

        /// <summary>
        /// Gets or sets the source hostname
        /// </summary>
        public string SourceHostname { get; set; }

        /// <summary>
        /// Gets or sets the source port
        /// </summary>
        public short? SourcePort { get; set; }

        /// <summary>
        /// Gets or sets the source username
        /// </summary>
        public string SourceUsername { get; set; }

        /// <summary>
        /// Gets or sets the source password
        /// </summary>
        public string SourcePassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to save the source password
        /// </summary>
        public bool SourceSavePassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the windows authentication for the source
        /// </summary>
        public bool SourceUseWindowsAuthentication { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the azure authentication for the source
        /// </summary>
        public bool SourceUseAzureAuthentication { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the SSL for the source
        /// </summary>
        public bool SourceUseSSL { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore the server certificate for the source
        /// </summary>
        public bool SourceIgnoreServerCertificate { get; set; }

        /// <summary>
        /// Gets or sets the source database
        /// </summary>
        public string SourceDatabase { get; set; }

        /// <summary>
        /// Gets or sets the target hostname
        /// </summary>
        public string TargetHostname { get; set; }

        /// <summary>
        /// Gets or sets the target port
        /// </summary>
        public short? TargetPort { get; set; }

        /// <summary>
        /// Gets or sets the target username
        /// </summary>
        public string TargetUsername { get; set; }

        /// <summary>
        /// Gets or sets the target password
        /// </summary>
        public string TargetPassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to save the target password
        /// </summary>
        public bool TargetSavePassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the windows authentication for the target
        /// </summary>
        public bool TargetUseWindowsAuthentication { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the azure authentication for the target
        /// </summary>
        public bool TargetUseAzureAuthentication { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the SSL for the target
        /// </summary>
        public bool TargetUseSSL { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore the server certificate for the target
        /// </summary>
        public bool TargetIgnoreServerCertificate { get; set; }

        /// <summary>
        /// Gets or sets the target database
        /// </summary>
        public string TargetDatabase { get; set; }

        /// <summary>
        /// Gets or sets the project options
        /// </summary>
        public ProjectOptions ProjectOptions { get; set; }
    }
}
