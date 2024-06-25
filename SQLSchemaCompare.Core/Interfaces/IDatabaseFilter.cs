namespace TiCodeX.SQLSchemaCompare.Core.Interfaces
{
    /// <summary>
    /// Defines a class that provides the mechanism to filter the database objects
    /// </summary>
    public interface IDatabaseFilter
    {
        /// <summary>
        /// Performs the filtering of the database objects
        /// </summary>
        /// <param name="database">The database to filter</param>
        /// <param name="filteringOptions">The filtering options</param>
        void PerformFilter(ABaseDb database, FilteringOptions filteringOptions);
    }
}
