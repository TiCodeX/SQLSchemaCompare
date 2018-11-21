namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.MySql
{
    /// <summary>
    /// Specific MySql foreign key definition
    /// </summary>
    public class MySqlForeignKey : ABaseDbForeignKey
    {
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
