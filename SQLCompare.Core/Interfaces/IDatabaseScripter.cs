using System.Collections.Generic;
using SQLCompare.Core.Entities.Database;

namespace SQLCompare.Core.Interfaces
{
    /// <summary>
    /// Defines a class that provides the mechanisms to script the database
    /// </summary>
    public interface IDatabaseScripter
    {
        /// <summary>
        /// Generates the create table script
        /// </summary>
        /// <param name="table">The table to be scripted</param>
        /// <param name="sourceTable">The source table for comparison, used for column order</param>
        /// <returns>The create script</returns>
        string GenerateCreateTableScript(ABaseDbTable table, ABaseDbTable sourceTable = null);

        /// <summary>
        /// Generates the create view script
        /// </summary>
        /// <param name="view">The view to be scripted</param>
        /// <returns>The create script</returns>
        string GenerateCreateViewScript(ABaseDbView view);

        /// <summary>
        /// Generates the create function script
        /// </summary>
        /// <param name="sqlFunction">The function to be scripted</param>
        /// <param name="dataTypes">The list of database data types</param>
        /// <returns>The create script</returns>
        string GenerateCreateFunctionScript(ABaseDbRoutine sqlFunction, IEnumerable<ABaseDbObject> dataTypes);

        /// <summary>
        /// Generates the create store procedure script
        /// </summary>
        /// <param name="storeProcedure">The store procedure to be scripted</param>
        /// <returns>The create script</returns>
        string GenerateCreateStoreProcedureScript(ABaseDbRoutine storeProcedure);
    }
}