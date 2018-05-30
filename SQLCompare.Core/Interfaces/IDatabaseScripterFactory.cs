using SQLCompare.Core.Entities.Database;

namespace SQLCompare.Core.Interfaces
{
    /// <summary>
    /// Defines a class that creates a database scripter
    /// </summary>
    public interface IDatabaseScripterFactory
    {
        /// <summary>
        /// Creates the database scripter depending on the database
        /// </summary>
        /// <param name="database">The database that must be scripted</param>
        /// <returns>The specific database scripter</returns>
        IDatabaseScripter Create(ABaseDb database);
    }
}