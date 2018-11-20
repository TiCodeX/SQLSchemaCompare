namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.PostgreSql
{
    /// <summary>
    /// Specific PostgreSql view definition
    /// </summary>
    public class PostgreSqlView : ABaseDbView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlView"/> class
        /// </summary>
        public PostgreSqlView()
        {
            this.AlterScriptSupported = false;
        }

        /// <summary>
        /// Gets or sets the view's check option
        /// </summary>
        public string CheckOption { get; set; }
    }
}