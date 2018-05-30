using SQLCompare.Core.Entities.Database;
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
        public ABaseDb RetrievedSourceDatabase { get; set; }

        /// <summary>
        /// Gets or sets the retrieved target database
        /// </summary>
        public ABaseDb RetrievedTargetDatabase { get; set; }

        // public int _Compareresult { get; set; }
    }
}