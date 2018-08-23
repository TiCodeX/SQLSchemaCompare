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
        /// Generates the full script to create the whole database
        /// </summary>
        /// <param name="database">The database</param>
        /// <returns>The full script</returns>
        string GenerateFullScript(ABaseDb database);

        /// <summary>
        /// Generates the create table script
        /// </summary>
        /// <param name="table">The table to be scripted</param>
        /// <param name="referenceTable">The reference table for comparison, used for column order</param>
        /// <returns>The create script</returns>
        string GenerateCreateTableScript(ABaseDbTable table, ABaseDbTable referenceTable = null);

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
        /// Generates the create stored procedure script
        /// </summary>
        /// <param name="storedProcedure">The stored procedure to be scripted</param>
        /// <returns>The create script</returns>
        string GenerateCreateStoredProcedureScript(ABaseDbRoutine storedProcedure);

        /// <summary>
        /// Generates the create sequence script
        /// </summary>
        /// <param name="sequence">The sequence to be scripted</param>
        /// <returns>The create script</returns>
        string GenerateCreateSequenceScript(ABaseDbSequence sequence);

        /// <summary>
        /// Generates the create type script
        /// </summary>
        /// <param name="type">The type to be scripted</param>
        /// <returns>The create script</returns>
        string GenerateCreateTypeScript(ABaseDbDataType type);
    }
}