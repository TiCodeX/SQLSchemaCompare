using TiCodeX.SQLSchemaCompare.Core.Enums;

namespace TiCodeX.SQLSchemaCompare.UI.Models
{
    /// <summary>
    /// Represent a new project request
    /// </summary>
    public class NewProjectRequest
    {
        /// <summary>
        /// Gets or sets a value indicating whether to ignore the dirty state
        /// </summary>
        public bool IgnoreDirty { get; set; }

        /// <summary>
        /// Gets or sets the database type
        /// </summary>
        public DatabaseType? DatabaseType { get; set; }
    }
}
