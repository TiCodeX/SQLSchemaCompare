namespace TiCodeX.SQLSchemaCompare.Core.Interfaces
{
    using System.Collections.Generic;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Compare;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Database;

    /// <summary>
    /// Defines a class that provides the mechanisms to script the database
    /// </summary>
    public interface IDatabaseScripter
    {
        /// <summary>
        /// Get the database object name
        /// </summary>
        /// <param name="dbObject">The database object</param>
        /// <returns>The normalized database object name</returns>
        string GenerateObjectName(ABaseDbObject dbObject);

        /// <summary>
        /// Generates the full script to create the whole database
        /// </summary>
        /// <param name="database">The database</param>
        /// <returns>The full create script</returns>
        string GenerateFullCreateScript(ABaseDb database);

        /// <summary>
        /// Generates the full alter script
        /// </summary>
        /// <param name="differentItems">The different items</param>
        /// <param name="onlySourceItems">The items only on source</param>
        /// <param name="onlyTargetItems">The items only target</param>
        /// <returns>
        /// The full alter script
        /// </returns>
        string GenerateFullAlterScript(List<ABaseCompareResultItem> differentItems, ABaseDb onlySourceItems, ABaseDb onlyTargetItems);

        /// <summary>
        /// Generates the full script to drop the whole database
        /// </summary>
        /// <param name="database">The database</param>
        /// <returns>The full drop script</returns>
        string GenerateFullDropScript(ABaseDb database);

        /// <summary>
        /// Generates the create script for the given database object
        /// </summary>
        /// <param name="dbObject">The database object</param>
        /// <param name="includeChildDbObjects">Whether to script also the child database objects</param>
        /// <returns>The create script</returns>
        string GenerateCreateScript(ABaseDbObject dbObject, bool includeChildDbObjects);

        /// <summary>
        /// Generates the drop script for the given database object
        /// </summary>
        /// <param name="dbObject">The database object</param>
        /// <param name="includeChildDbObjects">Whether to script also the child database objects</param>
        /// <returns>The drop script</returns>
        string GenerateDropScript(ABaseDbObject dbObject, bool includeChildDbObjects);

        /// <summary>
        /// Generates the alter script for the given database object
        /// </summary>
        /// <param name="dbObject">The database object</param>
        /// <param name="includeChildDbObjects">Whether to script also the child database objects</param>
        /// <returns>The alter script</returns>
        string GenerateAlterScript(ABaseDbObject dbObject, bool includeChildDbObjects);
    }
}
