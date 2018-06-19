using SQLCompare.Core.Entities.Database;

namespace SQLCompare.Core.Interfaces.Services
{
    /// <summary>
    /// Defines a class that provides the mechanisms to compare two database instances
    /// </summary>
    public interface IDatabaseCompareService
    {
        /// <summary>
        /// Compares two tables
        /// </summary>
        /// <param name="source">The source table</param>
        /// <param name="target">The target table</param>
        /// <returns>Returns whether the tables are equal</returns>
        bool CompareTable(ABaseDbTable source, ABaseDbTable target);
    }
}