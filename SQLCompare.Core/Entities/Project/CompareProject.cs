using SQLCompare.Core.Entities.Compare;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using System.Xml.Serialization;

namespace SQLCompare.Core.Entities.Project
{
    /// <summary>
    /// Defines a database comparison project
    /// </summary>
    [XmlInclude(typeof(ADatabaseProviderOptions))]
    [XmlInclude(typeof(MicrosoftSqlDatabaseProviderOptions))]
    [XmlInclude(typeof(MySqlDatabaseProviderOptions))]
    [XmlInclude(typeof(PostgreSqlDatabaseProviderOptions))]
    [XmlInclude(typeof(ProjectOptions))]
    public class CompareProject
    {
        /// <summary>
        /// Gets or sets the database provider options for the source database
        /// </summary>
        public ADatabaseProviderOptions SourceProviderOptions { get; set; }

        /// <summary>
        /// Gets or sets the database provider options for the target database
        /// </summary>
        public ADatabaseProviderOptions TargetProviderOptions { get; set; }

        /// <summary>
        /// Gets or sets the project options
        /// </summary>
        public ProjectOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the retrieved source database
        /// </summary>
        [XmlIgnore]
        public ABaseDb RetrievedSourceDatabase { get; set; }

        /// <summary>
        /// Gets or sets the retrieved target database
        /// </summary>
        [XmlIgnore]
        public ABaseDb RetrievedTargetDatabase { get; set; }

        /// <summary>
        /// Gets or sets the result of the comparison
        /// </summary>
        [XmlIgnore]
        public CompareResult Result { get; set; }
    }
}