namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Specific MySql foreign key definition
    /// </summary>
    public class MySqlForeignKey : MySqlIndex
    {
        /// <summary>
        /// Gets or sets the match option
        /// </summary>
        public string MatchOption { get; set; }

        /// <summary>
        /// Gets or sets the update rule
        /// </summary>
        public string UpdateRule { get; set; }

        /// <summary>
        /// Gets or sets the delete rule
        /// </summary>
        public string DeleteRule { get; set; }
    }
}
