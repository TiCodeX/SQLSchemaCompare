namespace TiCodeX.SQLSchemaCompare.Core.Interfaces
{
    using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;

    /// <summary>
    /// Defines a class that creates a database provider
    /// </summary>
    public interface IDatabaseProviderFactory
    {
        /// <summary>
        /// Creates the database providers depending on the options
        /// </summary>
        /// <param name="dbpo">The database provider options</param>
        /// <returns>The specific database provider</returns>
        IDatabaseProvider Create(ADatabaseProviderOptions dbpo);
    }
}
