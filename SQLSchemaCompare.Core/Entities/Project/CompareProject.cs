namespace TiCodeX.SQLSchemaCompare.Core.Entities.Project
{
    using System.Xml.Serialization;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Compare;
    using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;
    using TiCodeX.SQLSchemaCompare.Core.Enums;

    /// <summary>
    /// Defines a database comparison project
    /// </summary>
    [XmlInclude(typeof(ADatabaseProviderOptions))]
    [XmlInclude(typeof(MicrosoftSqlDatabaseProviderOptions))]
    [XmlInclude(typeof(MySqlDatabaseProviderOptions))]
    [XmlInclude(typeof(PostgreSqlDatabaseProviderOptions))]
    [XmlInclude(typeof(MariaDbDatabaseProviderOptions))]
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
        /// Gets or sets the project state
        /// </summary>
        [XmlIgnore]
        public ProjectState State { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the database type is editalbe
        /// </summary>
        [XmlIgnore]
        public bool EditableDatabaseType { get; set; }

        /// <summary>
        /// Gets or sets the result of the comparison
        /// </summary>
        [XmlIgnore]
        public CompareResult Result { get; set; }
    }
}
