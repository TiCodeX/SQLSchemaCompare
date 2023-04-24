namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database
{
    using System.Collections.Generic;
    using TiCodeX.SQLSchemaCompare.Core.Enums;

    /// <summary>
    /// Provides generic information for database constraint classes
    /// </summary>
    public class ABaseDbConstraint : ABaseDbObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ABaseDbConstraint"/> class
        /// </summary>
        public ABaseDbConstraint()
        {
            this.AlterScriptSupported = false;
        }

        /// <inheritdoc />
        public override DatabaseObjectType ObjectType { get; } = DatabaseObjectType.Constraint;

        /// <summary>
        /// Gets or sets the table schema
        /// </summary>
        public string TableSchema { get; set; }

        /// <summary>
        /// Gets or sets the table name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the column name
        /// </summary>
        /// <remarks>Used only by the DatabaseProvider to group the constraints and fill the ColumnNames list</remarks>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets ordinal position
        /// </summary>
        /// <remarks>Used only by the DatabaseProvider to group the constraints and fill the ColumnNames list</remarks>
        public long OrdinalPosition { get; set; }

        /// <summary>
        /// Gets the column names already ordered
        /// </summary>
        public List<string> ColumnNames { get; } = new List<string>();

        /// <summary>
        /// Gets or sets the constraint type
        /// </summary>
        public string ConstraintType { get; set; }

        /// <summary>
        /// Gets or sets the constraint definition
        /// </summary>
        public string Definition { get; set; }
    }
}
