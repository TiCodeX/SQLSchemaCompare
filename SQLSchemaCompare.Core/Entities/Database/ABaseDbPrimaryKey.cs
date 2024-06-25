namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database primary keys
    /// </summary>
    /// <seealso cref="TiCodeX.SQLSchemaCompare.Core.Entities.Database.ABaseDbConstraint" />
    public class ABaseDbPrimaryKey : ABaseDbIndex
    {
        /// <inheritdoc />
        public override DatabaseObjectType ObjectType { get; } = DatabaseObjectType.PrimaryKey;
    }
}
