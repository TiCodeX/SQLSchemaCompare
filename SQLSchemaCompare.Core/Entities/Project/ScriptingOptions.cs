namespace TiCodeX.SQLSchemaCompare.Core.Entities.Project
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
        public bool OrderColumnAlphabetically { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the reference table columns order should be ignored when scripting columns
        /// </summary>
        public bool IgnoreReferenceTableColumnOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to generate update script for new not null columns.
        /// </summary>
        public bool GenerateUpdateScriptForNewNotNullColumns { get; set; }
    }
}
