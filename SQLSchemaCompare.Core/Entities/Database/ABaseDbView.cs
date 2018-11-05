using System.Collections.Generic;

namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database table classes
    /// </summary>
    public abstract class ABaseDbView : ABaseDbObject
    {
        /// <summary>
        /// Gets or sets the database view definition script
        /// </summary>
        public string ViewDefinition { get; set; }

        /// <summary>
        /// Gets the database view's indexes
        /// </summary>
        public List<ABaseDbIndex> Indexes { get; } = new List<ABaseDbIndex>();
    }
}