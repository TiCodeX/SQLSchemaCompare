using System.Collections.Generic;

namespace SQLCompare.Core.Entities.Database.PostgreSql
{
    /// <summary>
    /// Specific PostgreSql Composite data type definition
    /// </summary>
    public class PostgreSqlDataTypeComposite : PostgreSqlDataType
    {
        /// <summary>
        /// Gets or sets the attribute names
        /// </summary>
        public IEnumerable<string> AttributeNames { get; set; }

        /// <summary>
        /// Gets or sets the attribute type ids
        /// </summary>
        public IEnumerable<uint> AttributeTypeIds { get; set; }
    }
}
