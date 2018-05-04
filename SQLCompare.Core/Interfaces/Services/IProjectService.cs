namespace SQLCompare.Core.Interfaces.Services
{
    /// <summary>
    /// Defines a class that provides the mechanisms to handle the comparison project.
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// Saves the project
        /// </summary>
        void SaveProject();

        /// <summary>
        /// Closes the project
        /// </summary>
        void CloseProject();

        /// <summary>
        /// Load a project from a file
        /// </summary>
        /// <param name="filename">The filename from which the project must be loaded</param>
        void LoadProject(string filename);

        /// <summary>
        /// Start the comparison process
        /// </summary>
        void Compare();
    }
}