namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.MicrosoftSql
{
    /// <summary>
    /// Specific MicrosoftSql data type definition
    /// </summary>
    public class MicrosoftSqlDataType : ABaseDbDataType
    {
        /// <summary>
        /// Gets or sets the type id
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// Gets or sets the system type id
        /// </summary>
        public int SystemTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the type is nullable
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Gets or sets the precision
        /// </summary>
        public int Precision { get; set; }

        /// <summary>
        /// Gets or sets the scale
        /// </summary>
        public int Scale { get; set; }

        /// <summary>
        /// Gets or sets the maximum length
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the referenced system type
        /// </summary>
        public MicrosoftSqlDataType SystemType { get; set; }
    }
}
