namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database triggers
    /// </summary>
    public class ABaseDbTrigger : ABaseDbObject
    {
        /// <summary>
        /// Gets or sets the database trigger definition script
        /// </summary>
        public string Definition { get; set; }
    }
}
