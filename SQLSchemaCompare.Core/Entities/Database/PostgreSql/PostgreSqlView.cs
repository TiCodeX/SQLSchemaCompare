namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.PostgreSql
{
    /// <summary>
    /// Specific PostgreSql view definition
    /// </summary>
    public class PostgreSqlView : ABaseDbView
    {
        /// <summary>
        /// Gets or sets the view's check option
        /// </summary>
        public string CheckOption { get; set; }
    }
}