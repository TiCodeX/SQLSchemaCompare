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
        /// Generates the alter table script
        /// </summary>
        /// <param name="sourceTable">The source table</param>
        /// <param name="targetTable">The target table</param>
        /// <returns>The alter script</returns>
        string GenerateAlterTableScript(ABaseDbTable sourceTable, ABaseDbTable targetTable);

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
        /// Generates the alter view script
        /// </summary>
        /// <param name="sourceView">The source view</param>
        /// <param name="targetView">The target view</param>
        /// <returns>The alter script</returns>
        string GenerateAlterViewScript(ABaseDbView sourceView, ABaseDbView targetView);

        /// <summary>
        /// Generates the alter function script
        /// </summary>
        /// <param name="sourceFunction">The source function</param>
        /// <param name="sourceDataTypes">The list of source database data types</param>
        /// <param name="targetFunction">The target function</param>
        /// <param name="targetDataTypes">The list of target database data types</param>
        /// <returns>The alter script</returns>
        string GenerateAlterFunctionScript(ABaseDbFunction sourceFunction, IReadOnlyList<ABaseDbDataType> sourceDataTypes, ABaseDbFunction targetFunction, IReadOnlyList<ABaseDbDataType> targetDataTypes);

        /// <summary>
        /// Generates the alter stored procedure script
        /// </summary>
        /// <param name="sourceStoredProcedure">The source stored procedure</param>
        /// <param name="targetStoredProcedure">The target stored procedure</param>
        /// <returns>The alter script</returns>
        string GenerateAlterStoredProcedureScript(ABaseDbStoredProcedure sourceStoredProcedure, ABaseDbStoredProcedure targetStoredProcedure);

        /// <summary>
        /// Generates the alter trigger script
        /// </summary>
        /// <param name="sourceTrigger">The source trigger</param>
        /// <param name="targetTrigger">The target trigger</param>
        /// <returns>The alter script</returns>
        string GenerateAlterTriggerScript(ABaseDbTrigger sourceTrigger, ABaseDbTrigger targetTrigger);

        /// <summary>
        /// Generates the alter sequence script
        /// </summary>
        /// <param name="sourceSequence">The source sequence</param>
        /// <param name="targetSequence">The target sequence</param>
        /// <returns>The alter script</returns>
        string GenerateAlterSequenceScript(ABaseDbSequence sourceSequence, ABaseDbSequence targetSequence);

        /// <summary>
        /// Generates the alter type script
        /// </summary>
        /// <param name="sourceType">The source type</param>
        /// <param name="sourceDataTypes">The list of source database data types</param>
        /// <param name="targetType">The target type</param>
        /// <param name="targetDataTypes">The list of target database data types</param>
        /// <returns>The alter script</returns>
        string GenerateAlterTypeScript(ABaseDbDataType sourceType, IReadOnlyList<ABaseDbDataType> sourceDataTypes, ABaseDbDataType targetType, IReadOnlyList<ABaseDbDataType> targetDataTypes);

        /// <summary>
        /// Generates the create script for the given database object
        /// </summary>
        /// <param name="dbObject">The database object</param>
        /// <returns>The create script</returns>
        string GenerateCreateScript(ABaseDbObject dbObject);

        /// <summary>
        /// Generates the alter script for the given database object
        /// </summary>
        /// <param name="dbObject">The database object</param>
        /// <returns>The alter script</returns>
        string GenerateAlterScript(ABaseDbObject dbObject);
    }
}