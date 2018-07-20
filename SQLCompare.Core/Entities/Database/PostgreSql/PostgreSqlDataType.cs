namespace SQLCompare.Core.Entities.Database.PostgreSql
{
    /// <summary>
    /// Specific PostgreSql data type definition
    /// </summary>
    public class PostgreSqlDataType : ABaseDbObject
    {
        /// <summary>
        /// Gets or sets the data type id
        /// </summary>
        public uint TypeId { get; set; }

        /// <summary>
        /// Gets or sets the array data type id
        /// </summary>
        public uint ArrayTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the type is an array
        /// </summary>
        public bool IsArray { get; set; }

        /// <summary>
        /// Gets or sets the referenced type for arrays
        /// </summary>
        public PostgreSqlDataType ArrayType { get; set; }
    }
}
