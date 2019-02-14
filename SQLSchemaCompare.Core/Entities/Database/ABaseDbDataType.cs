using TiCodeX.SQLSchemaCompare.Core.Enums;

namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database data type classes
    /// </summary>
    public class ABaseDbDataType : ABaseDbObject
    {
        /// <inheritdoc />
        public override DatabaseObjectType ObjectType { get; } = DatabaseObjectType.DataType;

        /// <summary>
        /// Gets or sets a value indicating whether the type is user defined
        /// </summary>
        public bool IsUserDefined { get; set; }
    }
}
