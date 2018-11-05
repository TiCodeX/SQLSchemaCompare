namespace SQLCompare.Core.Entities.Database.MicrosoftSql
{
    /// <summary>
    /// Specific MicrosoftSql sequence definition
    /// </summary>
    public class MicrosoftSqlSequence : ABaseDbSequence
    {
        /// <summary>
        /// Gets or sets a value indicating whether is cached
        /// </summary>
        public bool IsCached { get; set; }
    }
}
