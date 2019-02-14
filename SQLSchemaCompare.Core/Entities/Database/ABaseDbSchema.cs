using TiCodeX.SQLSchemaCompare.Core.Enums;

namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database schema classes
    /// </summary>
    public class ABaseDbSchema : ABaseDbObject
    {
        /// <inheritdoc />
        public override DatabaseObjectType ObjectType { get; } = DatabaseObjectType.Schema;

        /// <summary>
        /// Gets or sets the owner
        /// </summary>
        public string Owner { get; set; }
    }
}
