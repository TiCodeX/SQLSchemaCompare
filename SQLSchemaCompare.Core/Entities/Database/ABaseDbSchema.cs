namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides generic information for database schema classes
    /// </summary>
    public class ABaseDbSchema : ABaseDbObject
    {
        /// <summary>
        /// Gets or sets the owner
        /// </summary>
        public string Owner { get; set; }
    }
}
