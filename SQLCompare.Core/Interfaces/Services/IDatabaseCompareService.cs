using SQLCompare.Core.Entities.Database;

namespace SQLCompare.Core.Interfaces.Services
{
    /// <summary>
    /// Defines a class that provides the mechanisms to compare two database instances
    /// </summary>
    public interface IDatabaseCompareService
    {
        /// <summary>
        /// Compares two databases
        /// </summary>
        /// <param name="source">The source database</param>
        /// <param name="target">The target database</param>
        void Compare(BaseDb source, BaseDb target);
    }
}