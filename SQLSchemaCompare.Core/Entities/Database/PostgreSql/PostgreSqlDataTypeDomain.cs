namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.PostgreSql
{
    /// <summary>
    /// Specific PostgreSql Domain data type definition
    /// </summary>
    public class PostgreSqlDataTypeDomain : PostgreSqlDataType
    {
        /// <summary>
        /// Gets or sets the base type identifier
        /// </summary>
        public uint BaseTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the type is not nullable
        /// </summary>
        public bool NotNull { get; set; }

        /// <summary>
        /// Gets or sets the constraint name
        /// </summary>
        public string ConstraintName { get; set; }

        /// <summary>
        /// Gets or sets the constraint definition
        /// </summary>
        public string ConstraintDefinition { get; set; }
    }
}
