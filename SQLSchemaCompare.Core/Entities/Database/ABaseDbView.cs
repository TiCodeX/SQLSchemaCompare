using System.Collections.Generic;
using TiCodeX.SQLSchemaCompare.Core.Enums;

namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database table classes
    /// </summary>
    public abstract class ABaseDbView : ABaseDbObject
    {
        /// <inheritdoc />
        public override DatabaseObjectType ObjectType { get; } = DatabaseObjectType.View;

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