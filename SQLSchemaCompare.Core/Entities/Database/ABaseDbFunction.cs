namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database functions
    /// </summary>
    public abstract class ABaseDbFunction : ABaseDbObject
    {
        /// <summary>
        /// Gets or sets the database function definition script
        /// </summary>
        public string Definition { get; set; }
    }
}