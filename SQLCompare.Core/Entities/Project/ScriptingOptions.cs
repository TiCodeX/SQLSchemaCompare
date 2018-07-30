namespace SQLCompare.Core.Entities.Project
{
    /// <summary>
    /// The scripting options of the project
    /// </summary>
    public class ScriptingOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether collate must be ignored
        /// </summary>
        public bool IgnoreCollate { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether column should be scripted in alphabetical order
        /// </summary>
        public bool OrderColumnAlphabetically { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the reference table columns order should be ignored when scripting columns
        /// </summary>
        public bool IgnoreReferenceTableColumnOrder { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether use schema name in scripting objects
        /// </summary>
        public bool UseSchemaName { get; set; } = true;
    }
}