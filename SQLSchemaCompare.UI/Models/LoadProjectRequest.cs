namespace SQLCompare.UI.Models
{
    /// <summary>
    /// Represent a load project request
    /// </summary>
    public class LoadProjectRequest
    {
        /// <summary>
        /// Gets or sets a value indicating whether to ignore the dirty state
        /// </summary>
        public bool IgnoreDirty { get; set; }

        /// <summary>
        /// Gets or sets the filename to load
        /// </summary>
        public string Filename { get; set; }
    }
}
