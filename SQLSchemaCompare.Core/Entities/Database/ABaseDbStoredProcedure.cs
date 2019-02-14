using TiCodeX.SQLSchemaCompare.Core.Enums;

namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database stored procedures
    /// </summary>
    public abstract class ABaseDbStoredProcedure : ABaseDbObject
    {
        /// <inheritdoc />
        public override DatabaseObjectType ObjectType { get; } = DatabaseObjectType.StoredProcedure;

        /// <summary>
        /// Gets or sets the database stored procedure definition script
        /// </summary>
        public string Definition { get; set; }
    }
}
