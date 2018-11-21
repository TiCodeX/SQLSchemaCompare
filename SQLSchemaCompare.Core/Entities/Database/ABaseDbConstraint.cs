namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database constraint classes
    /// </summary>
    public class ABaseDbConstraint : ABaseDbObject
    {
        /// <summary>
        /// Gets or sets the table database
        /// </summary>
        public string TableDatabase { get; set; }

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
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets ordinal position
        /// </summary>
        public long OrdinalPosition { get; set; }

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