using SQLCompare.Core.Entities.DatabaseProvider;

namespace SQLCompare.Core.Entities.Project
{
    /// <summary>
    /// Defines a database comparison project
    /// </summary>
    public class CompareProject
    {
        /// <summary>
        /// Gets or sets the database provider options for the source database
        /// </summary>
        public DatabaseProviderOptions Source { get; set; }

        /// <summary>
        /// Gets or sets the database provider options for the target database
        /// </summary>
        public DatabaseProviderOptions Target { get; set; }

        /// <summary>
        /// Gets or sets the project options
        /// </summary>
        public ProjectOptions Options { get; set; }

        // private BaseDb _sourceDB;

        // private BaseDb _targetDB;

        // public int _Compareresult { get; set; }
    }
}