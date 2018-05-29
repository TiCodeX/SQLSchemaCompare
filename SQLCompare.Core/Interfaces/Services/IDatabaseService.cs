using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using System.Collections.Generic;

namespace SQLCompare.Core.Interfaces.Services
{
    /// <summary>
    /// Defines a class that provides the mechanisms to read information from a database
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        /// Returns a list of all available databases
        /// </summary>
        /// <param name="options">The options for the database</param>
        /// <returns>The list of database names</returns>
        List<string> ListDatabases(DatabaseProviderOptions options);

        /// <summary>
        /// Gets the database structure
        /// </summary>
        /// <param name="options">The options for the database</param>
        /// <returns>The database structure</returns>
        BaseDb GetDatabase(DatabaseProviderOptions options);
    }
}
