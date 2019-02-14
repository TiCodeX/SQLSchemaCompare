namespace TiCodeX.SQLSchemaCompare.Core.Entities.Project
{
    /// <summary>
    /// Configurable options of the project
    /// </summary>
    public class ProjectOptions
    {
        /// <summary>
        /// Gets or sets the scripting options
        /// </summary>
        public ScriptingOptions Scripting { get; set; } = new ScriptingOptions();

        /// <summary>
        /// Gets or sets the filtering options
        /// </summary>
        public FilteringOptions Filtering { get; set; } = new FilteringOptions();
    }
}