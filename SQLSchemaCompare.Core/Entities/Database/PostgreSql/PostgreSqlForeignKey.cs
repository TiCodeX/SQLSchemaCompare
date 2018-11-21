namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.PostgreSql
{
    /// <summary>
    /// Specific PostgreSql foreign key definition
    /// </summary>
    public class PostgreSqlForeignKey : ABaseDbForeignKey
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

        /// <summary>
        /// Gets or sets a value indicating whether the foreign key is deferrable
        /// </summary>
        public bool IsDeferrable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the foreign key is initially deferred
        /// </summary>
        public bool IsInitiallyDeferred { get; set; }
    }
}
