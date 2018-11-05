using SQLCompare.Core.Entities.Project;

namespace SQLCompare.Core.Interfaces.Repository
{
    /// <summary>
    /// Defines a class that provides the mechanism to store and retrieve the project configuration
    /// </summary>
    public interface IProjectRepository
    {
        /// <summary>
        /// Read the project configuration from a file
        /// </summary>
        /// <param name="filename">The file to read from</param>
        /// <returns>The project</returns>
        CompareProject Read(string filename);

        /// <summary>
        /// Write the project configuration to a file
        /// </summary>
        /// <param name="project">The project to save</param>
        /// <param name="filename">The file path</param>
        void Write(CompareProject project, string filename);
    }
}
