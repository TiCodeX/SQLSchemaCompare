namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database table classes
    /// </summary>
    public abstract class ABaseDbView : ABaseDbObject
    {
        /// <summary>
        /// Gets or sets the database view definition script
        /// </summary>
        public string ViewDefinition { get; set; }
    }
}