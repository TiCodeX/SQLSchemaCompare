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

        /// <summary>
        /// Compares two views
        /// </summary>
        /// <param name="sourceItem">The source view</param>
        /// <param name="targetItem">The target view</param>
        /// <returns>Returns whether the views are equal</returns>
        bool CompareView(ABaseDbView sourceItem, ABaseDbView targetItem);
    }
}