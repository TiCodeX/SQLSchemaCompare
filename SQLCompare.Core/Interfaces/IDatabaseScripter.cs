using SQLCompare.Core.Entities.Database;

namespace SQLCompare.Core.Interfaces
{
    /// <summary>
    /// Defines a class that provides the mechanisms to script the database
    /// </summary>
    public interface IDatabaseScripter
    {
        /// <summary>
        /// Generates the a database create table script with indexes and constraints
        /// </summary>
        /// <param name="table">The table that must be scripted</param>
        /// <returns>The generated create script</returns>
        string GenerateCreateTableScript(ABaseDbTable table);
    }
}