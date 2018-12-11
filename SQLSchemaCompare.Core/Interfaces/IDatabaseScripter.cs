using System.Collections.Generic;
using TiCodeX.SQLSchemaCompare.Core.Entities.Compare;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database;

namespace TiCodeX.SQLSchemaCompare.Core.Interfaces
{
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
        /// Generates the create table script and related keys/indexes
        /// </summary>
        /// <param name="table">The table to be scripted</param>
        /// <returns>The create script</returns>
        string GenerateCreateTableScript(ABaseDbTable table);

        /// <summary>
        /// Generates the drop table script and related keys/indexes
        /// </summary>
        /// <param name="table">The table to be scripted</param>
        /// <returns>The drop script</returns>
        string GenerateDropTableScript(ABaseDbTable table);

        /// <summary>
        /// Generates the create view script and related indexes
        /// </summary>
        /// <param name="view">The view to be scripted</param>
        /// <returns>The create script</returns>
        string GenerateCreateViewScript(ABaseDbView view);

        /// <summary>
        /// Generates the drop view script and related drop indexes
        /// </summary>
        /// <param name="view">The view</param>
        /// <returns>The drop script</returns>
        string GenerateDropViewScript(ABaseDbView view);

        /// <summary>
        /// Generates the create script for the given database object
        /// </summary>
        /// <param name="dbObject">The database object</param>
        /// <returns>The create script</returns>
        string GenerateCreateScript(ABaseDbObject dbObject);

        /// <summary>
        /// Generates the drop script for the given database object
        /// </summary>
        /// <param name="dbObject">The database object</param>
        /// <returns>The drop script</returns>
        string GenerateDropScript(ABaseDbObject dbObject);

        /// <summary>
        /// Generates the alter script for the given database object
        /// </summary>
        /// <param name="dbObject">The database object</param>
        /// <returns>The alter script</returns>
        string GenerateAlterScript(ABaseDbObject dbObject);
    }
}