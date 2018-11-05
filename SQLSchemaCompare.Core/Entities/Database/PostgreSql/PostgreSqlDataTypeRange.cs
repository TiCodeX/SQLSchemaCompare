namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.PostgreSql
{
    /// <summary>
    /// Specific PostgreSql Range data type definition
    /// </summary>
    public class PostgreSqlDataTypeRange : PostgreSqlDataType
    {
        /// <summary>
        /// Gets or sets the sub type identifier
        /// </summary>
        public uint SubTypeId { get; set; }

        /// <summary>
        /// Gets or sets the canonical
        /// </summary>
        public string Canonical { get; set; }

        /// <summary>
        /// Gets or sets the sub type difference
        /// </summary>
        public string SubTypeDiff { get; set; }
    }
}
