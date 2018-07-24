using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Project;
using SQLCompare.Core.Interfaces;

namespace SQLCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Implement base database scripting functionality
    /// </summary>
    /// <typeparam name="TScriptHelper">The specific script helper class</typeparam>
    public abstract class ADatabaseScripter<TScriptHelper> : IDatabaseScripter
                where TScriptHelper : AScriptHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ADatabaseScripter{TScriptHelper}"/> class.
        /// </summary>
        /// <param name="logger">The injected logger</param>
        /// <param name="options">The project options used during scripting process</param>
        /// <param name="scriptHelper">The scripting helper</param>
        protected ADatabaseScripter(ILogger logger, ProjectOptions options, TScriptHelper scriptHelper)
        {
            this.Logger = logger;
            this.Options = options;
            this.ScriptHelper = scriptHelper;
        }

        /// <summary>
        /// Gets the indentation value
        /// </summary>
        protected string Indent => "    "; // 4 spaces

        /// <summary>
        /// Gets the logger
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the project options
        /// </summary>
        protected ProjectOptions Options { get; }

        /// <summary>
        /// Gets the scripting helper
        /// </summary>
        protected TScriptHelper ScriptHelper { get; }

        /// <inheritdoc/>
        public string GenerateCreateTableScript(ABaseDbTable table, ABaseDbTable sourceTable)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            var sb = new StringBuilder();

            // Script the CREATE TABLE
            sb.Append(this.ScriptCreateTable(table, sourceTable));
            sb.AppendLine();
            sb.AppendLine();

            // TODO: and indexes
            // Script the ALTER TABLE for primary keys and indexes
            sb.AppendLine(AScriptHelper.ScriptComment("Constraints and Indexes"));
            sb.AppendLine(this.ScriptPrimaryKeysAlterTable(table));

            // Script the ALTER TABLE for foreign keys
            sb.AppendLine(AScriptHelper.ScriptComment("Foreign keys"));
            sb.AppendLine(this.ScriptForeignKeysAlterTable(table));

            return sb.ToString();
        }

        /// <inheritdoc/>
        public string GenerateCreateViewScript(ABaseDbView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            return this.ScriptCreateView(view);
        }

        /// <inheritdoc/>
        public string GenerateCreateFunctionScript(ABaseDbRoutine sqlFunction, IEnumerable<ABaseDbObject> dataTypes)
        {
            if (sqlFunction == null)
            {
                throw new ArgumentNullException(nameof(sqlFunction));
            }

            if (dataTypes == null)
            {
                throw new ArgumentNullException(nameof(dataTypes));
            }

            return this.ScriptCreateFunction(sqlFunction, dataTypes);
        }

        /// <inheritdoc/>
        public string GenerateCreateStoreProcedureScript(ABaseDbRoutine storeProcedure)
        {
            if (storeProcedure == null)
            {
                throw new ArgumentNullException(nameof(storeProcedure));
            }

            return this.ScriptCreateStoreProcedure(storeProcedure);
        }

        /// <summary>
        /// Generates the create table script
        /// </summary>
        /// <param name="table">The table to script</param>
        /// <param name="sourceTable">The source table for comparison, used for column order</param>
        /// <returns>The create table script</returns>
        protected abstract string ScriptCreateTable(ABaseDbTable table, ABaseDbTable sourceTable);

        /// <summary>
        /// Generates the alter table for adding primary keys after create table
        /// </summary>
        /// <param name="table">The table to alter</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptPrimaryKeysAlterTable(ABaseDbTable table);

        /// <summary>
        /// Generates the alter table for adding foreign keys after create table
        /// </summary>
        /// <param name="table">The table to alter</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptForeignKeysAlterTable(ABaseDbTable table);

        /// <summary>
        /// Generates the alter table for adding indexes after create table
        /// </summary>
        /// <param name="table">The table to alter</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptIndexesAlterTable(ABaseDbTable table);

        /// <summary>
        /// Generates the create view script
        /// </summary>
        /// <param name="view">The view to script</param>
        /// <returns>The create view script</returns>
        protected abstract string ScriptCreateView(ABaseDbView view);

        /// <summary>
        /// Generates the create function script
        /// </summary>
        /// <param name="sqlFunction">The function to script</param>
        /// <param name="dataTypes">The list of database data types</param>
        /// <returns>The create function script</returns>
        protected abstract string ScriptCreateFunction(ABaseDbRoutine sqlFunction, IEnumerable<ABaseDbObject> dataTypes);

        /// <summary>
        /// Generates the create store procedure script
        /// </summary>
        /// <param name="storeProcedure">The store procedure to script</param>
        /// <returns>The create store procedure script</returns>
        protected abstract string ScriptCreateStoreProcedure(ABaseDbRoutine storeProcedure);

        /// <summary>
        /// Get the table columns sorted depending on options and source table
        /// </summary>
        /// <param name="table">The table with columns to script</param>
        /// <param name="sourceTable">The source table for comparison</param>
        /// <returns>The sorted columns</returns>
        protected IEnumerable<ABaseDbColumn> GetSortedTableColumns(ABaseDbTable table, ABaseDbTable sourceTable)
        {
            if (sourceTable != null && !this.Options.Scripting.IgnoreSourceTableColumnOrder)
            {
                var columns = table.Columns.ToList();
                var sourceColumns = sourceTable.Columns.AsEnumerable();
                if (this.Options.Scripting.OrderColumnAlphabetically)
                {
                    sourceColumns = sourceTable.Columns.OrderBy(x => x.Name);
                }

                var sortedColumns = new List<ABaseDbColumn>();

                foreach (var s in sourceColumns)
                {
                    foreach (var c in columns)
                    {
                        if (string.Equals(s.Name, c.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            sortedColumns.Add(c);
                            columns.Remove(c);
                            break;
                        }
                    }
                }

                sortedColumns.AddRange(columns);

                return sortedColumns;
            }
            else if (this.Options.Scripting.OrderColumnAlphabetically)
            {
                return table.Columns.OrderBy(x => x.Name);
            }
            else
            {
                return table.Columns;
            }
        }
    }
}
