namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database triggers
    /// </summary>
    public class ABaseDbTrigger : ABaseDbObject
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
        /// Gets or sets the database trigger definition script
        /// </summary>
        public string Definition { get; set; }
    }
}
