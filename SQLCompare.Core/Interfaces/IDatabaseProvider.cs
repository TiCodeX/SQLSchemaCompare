using SQLCompare.Core.Entities.Database;
using System.Collections.Generic;

namespace SQLCompare.Core.Interfaces
{
    /// <summary>
    /// Defines a class that provides the mechanisms to retrieve various information of a database
    /// </summary>
    public interface IDatabaseProvider
    {
        /// <summary>
        /// Gets the database structure
        /// </summary>
        /// <returns>The database structure</returns>
        BaseDb GetDatabase();

        /// <summary>
        /// Gets the list of available database
        /// </summary>
        /// <returns>The list of database names</returns>
        List<string> GetDatabaseList();
    }
}