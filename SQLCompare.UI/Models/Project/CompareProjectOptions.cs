﻿using SQLCompare.Core.Enums;
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
        /// Gets or sets a value indicating whether to use the windows authentication for the source
        /// </summary>
        public bool SourceUseWindowsAuthentication { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the SSL for the source
        /// </summary>
        public bool SourceUseSSL { get; set; }

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
        /// Gets or sets a value indicating whether to use the windows authentication for the target
        /// </summary>
        public bool TargetUseWindowsAuthentication { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the SSL for the target
        /// </summary>
        public bool TargetUseSSL { get; set; }

        /// <summary>
        /// Gets or sets the target database
        /// </summary>
        public string TargetDatabase { get; set; }

        /// <summary>
        /// Gets or sets the filename to save the project
        /// </summary>
        public string SaveProjectFilename { get; set; }
    }
}
