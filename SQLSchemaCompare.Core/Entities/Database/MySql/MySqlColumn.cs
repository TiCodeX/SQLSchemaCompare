namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.MySql
{
    /// <summary>
    /// Specific MySql column definition
    /// </summary>
    public class MySqlColumn : ABaseDbColumn
    {
        /// <summary>
        /// Gets or sets a value indicating whether the column is nullable
        /// </summary>
        public string IsNullable { get; set; }

        /// <summary>
        /// Gets or sets the column generation expression
        /// </summary>
        public string GenerationExpression { get; set; }

        /// <summary>
        /// Gets or sets the column extra information
        /// </summary>
        public string Extra { get; set; }

        /// <summary>
        /// Gets or sets the column type
        /// </summary>
        public string ColumnType { get; set; }
    }
}