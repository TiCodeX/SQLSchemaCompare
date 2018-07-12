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
    }
}