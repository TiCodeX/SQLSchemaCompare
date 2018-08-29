namespace SQLCompare.UI.Models.Main
{
    /// <summary>
    /// Represent the result of a CreateScript request
    /// </summary>
    public class CreateScriptResult
    {
        /// <summary>
        /// Gets or sets the source SQL
        /// </summary>
        public string SourceSql { get; set; }

        /// <summary>
        /// Gets or sets the target SQL
        /// </summary>
        public string TargetSql { get; set; }
    }
}
