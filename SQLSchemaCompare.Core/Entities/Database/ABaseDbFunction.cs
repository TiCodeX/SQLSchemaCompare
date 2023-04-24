namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database
{
    using TiCodeX.SQLSchemaCompare.Core.Enums;

    /// <summary>
    /// Provides generic information for database functions
    /// </summary>
    public abstract class ABaseDbFunction : ABaseDbObject
    {
        /// <inheritdoc />
        public override DatabaseObjectType ObjectType { get; } = DatabaseObjectType.Function;

        /// <summary>
        /// Gets or sets the database function definition script
        /// </summary>
        public string Definition { get; set; }
    }
}
