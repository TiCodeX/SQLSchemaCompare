namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.MySql
{
    /// <summary>
    /// Specific MySql table definition
    /// </summary>
    public class MySqlTable : ABaseDbTable
    {
        /// <summary>
        /// Gets or sets the table engine
        /// </summary>
        public string Engine { get; set; }

        /// <summary>
        /// Gets or sets the table character set
        /// </summary>
        public string TableCharacterSet { get; set; }
    }
}