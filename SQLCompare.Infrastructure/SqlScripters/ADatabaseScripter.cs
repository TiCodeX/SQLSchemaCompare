using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQLCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Implement base database scripting functionality
    /// </summary>
    public abstract class ADatabaseScripter : IDatabaseScripter
    {
        /// <summary>
        /// The indenation value
        /// </summary>
        public const string Indent = "    "; // 4 spaces

        /// <summary>
        /// Generates the create table script
        /// </summary>
        /// <param name="table">The table to be scripted</param>
        /// <returns>The create script</returns>
        public abstract string ScriptCreateTable(ABaseDbTable table);
    }
}
