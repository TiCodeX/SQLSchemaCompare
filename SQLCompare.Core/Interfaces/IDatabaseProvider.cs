using SQLCompare.Core.Entities.Database;

namespace SQLCompare.Core.Interfaces
{
    /// <summary>
    /// Defines a class that provides the mechanisms to retrieve the database
    /// </summary>
    public interface IDatabaseProvider
    {
        /// <summary>
        /// Gets the database structure
        /// </summary>
        /// <returns>The database structure</returns>
        BaseDb GetDatabase();
    }
}