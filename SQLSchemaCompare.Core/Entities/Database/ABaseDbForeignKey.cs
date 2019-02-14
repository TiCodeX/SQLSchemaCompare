using System.Collections.Generic;
using TiCodeX.SQLSchemaCompare.Core.Enums;

namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database foreign keys
    /// </summary>
    public class ABaseDbForeignKey : ABaseDbConstraint
    {
        /// <inheritdoc />
        public override DatabaseObjectType ObjectType { get; } = DatabaseObjectType.ForeignKey;

        /// <summary>
        /// Gets or sets referenced table schema
        /// </summary>
        public string ReferencedTableSchema { get; set; }

        /// <summary>
        /// Gets or sets referenced table name
        /// </summary>
        public string ReferencedTableName { get; set; }

        /// <summary>
        /// Gets or sets referenced column name
        /// </summary>
        /// <remarks>Used only by the DatabaseProvider to group the constraints and fill the ReferencedColumnNames list</remarks>
        public string ReferencedColumnName { get; set; }

        /// <summary>
        /// Gets the column names already ordered
        /// </summary>
        public List<string> ReferencedColumnNames { get; } = new List<string>();
    }
}
