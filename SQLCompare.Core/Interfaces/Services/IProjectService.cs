using SQLCompare.Core.Entities.Project;

namespace SQLCompare.Core.Interfaces.Services
{
    /// <summary>
    /// Defines a class that provides the mechanisms to handle the project.
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// Gets the current project
        /// </summary>
        CompareProject Project { get; }

        /// <summary>
        /// Create a new project
        /// </summary>
        /// <returns>Returns false whether there is a project open</returns>
        bool NewProject();

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
    }
}