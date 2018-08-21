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
        public string GenerateFullScript(ABaseDb database)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            var sb = new StringBuilder();

            // Script the CREATE SEQUENCE
            foreach (var sequence in database.Sequences)
            {
                sb.AppendLine(this.ScriptCreateSequence(sequence));
            }

            // Script the CREATE TABLE
            foreach (var table in database.Tables)
            {
                sb.Append(this.ScriptCreateTable(table));
            }

            sb.AppendLine();
            sb.AppendLine();

            // Script the functions
            sb.AppendLine(AScriptHelper.ScriptComment("Functions"));
            if (database.Functions.Count > 0)
            {
                sb.Append(this.ScriptHelper.ScriptCommitTransaction());
                foreach (var function in database.Functions)
                {
                    sb.Append(this.ScriptCreateFunction(function, database.DataTypes));
                }
            }

            sb.AppendLine();
            sb.AppendLine();

            // Script the stored procedures
            sb.AppendLine(AScriptHelper.ScriptComment("Stored Procedures"));
            if (database.StoredProcedures.Count > 0)
            {
                sb.Append(this.ScriptHelper.ScriptCommitTransaction());
                foreach (var storedProcedure in database.StoredProcedures)
                {
                    sb.Append(this.ScriptCreateStoredProcedure(storedProcedure));
                }
            }

            sb.AppendLine();
            sb.AppendLine();

            // Script the ALTER TABLE for primary keys and indexes
            sb.AppendLine(AScriptHelper.ScriptComment("Constraints and Indexes"));
            foreach (var table in database.Tables)
            {
                sb.Append(this.ScriptPrimaryKeysAlterTable(table));
                sb.Append(this.ScriptConstraintsAlterTable(table));
                sb.Append(this.ScriptCreateIndexes(table));
            }

            sb.AppendLine();
            sb.AppendLine();

            // Script the ALTER TABLE for foreign keys
            sb.AppendLine(AScriptHelper.ScriptComment("Foreign keys"));
            foreach (var table in database.Tables)
            {
                sb.AppendLine(this.ScriptForeignKeysAlterTable(table));
            }

            sb.AppendLine();
            sb.AppendLine();

            // Script the views
            sb.AppendLine(AScriptHelper.ScriptComment("Views"));
            if (database.Views.Count > 0)
            {
                sb.Append(this.ScriptHelper.ScriptCommitTransaction());
                foreach (var view in database.Views)
                {
                    var viewScript = this.ScriptCreateView(view);
                    sb.Append(viewScript);
                    if (!viewScript.EndsWith("\n", StringComparison.Ordinal))
                    {
                        sb.AppendLine();
                    }
                }
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        public string GenerateCreateTableScript(ABaseDbTable table, ABaseDbTable referenceTable)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            var sb = new StringBuilder();

            // Script the CREATE TABLE
            sb.Append(this.ScriptCreateTable(table, referenceTable));
            sb.AppendLine();
            sb.AppendLine();

            // Script the ALTER TABLE for primary keys and indexes
            sb.AppendLine(AScriptHelper.ScriptComment("Constraints and Indexes"));
            sb.Append(this.ScriptPrimaryKeysAlterTable(table));
            sb.Append(this.ScriptConstraintsAlterTable(table));
            sb.AppendLine(this.ScriptCreateIndexes(table));

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
        public string GenerateCreateStoredProcedureScript(ABaseDbRoutine storedProcedure)
        {
            if (storedProcedure == null)
            {
                throw new ArgumentNullException(nameof(storedProcedure));
            }

            return this.ScriptCreateStoredProcedure(storedProcedure);
        }

        /// <inheritdoc/>
        public string GenerateCreateSequenceScript(ABaseDbSequence sequence)
        {
            if (sequence == null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            return this.ScriptCreateSequence(sequence);
        }

        /// <summary>
        /// Generates the create table script
        /// </summary>
        /// <param name="table">The table to script</param>
        /// <param name="referenceTable">The reference table for comparison, used for column order</param>
        /// <returns>The create table script</returns>
        protected abstract string ScriptCreateTable(ABaseDbTable table, ABaseDbTable referenceTable = null);

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
        /// Generates the alter table for adding the constraints after create table
        /// </summary>
        /// <param name="table">The table to alter</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptConstraintsAlterTable(ABaseDbTable table);

        /// <summary>
        /// Generates the create index scripts for adding indexes after create table
        /// </summary>
        /// <param name="table">The table to create the indexes</param>
        /// <returns>The create index scripts</returns>
        protected abstract string ScriptCreateIndexes(ABaseDbTable table);

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
        /// Generates the create stored procedure script
        /// </summary>
        /// <param name="storedProcedure">The stored procedure to script</param>
        /// <returns>The create stored procedure script</returns>
        protected abstract string ScriptCreateStoredProcedure(ABaseDbRoutine storedProcedure);

        /// <summary>
        /// Generates the create sequence script
        /// </summary>
        /// <param name="sequence">The sequence to script</param>
        /// <returns>The create sequence script</returns>
        protected abstract string ScriptCreateSequence(ABaseDbSequence sequence);

        /// <summary>
        /// Get the table columns sorted depending on options and source table
        /// </summary>
        /// <param name="table">The table with columns to script</param>
        /// <param name="referenceTable">The reference table for comparison</param>
        /// <returns>The sorted columns</returns>
        protected IEnumerable<ABaseDbColumn> GetSortedTableColumns(ABaseDbTable table, ABaseDbTable referenceTable)
        {
            // Order table columns alphabetically or by ordinal position
            var columns = this.Options.Scripting.OrderColumnAlphabetically ? table.Columns.OrderBy(x => x.Name).ToList() : this.OrderColumnsByOrdinalPosition(table).ToList();

            // If there is no source table or ignore source table column order, returns the columns
            if (referenceTable == null || this.Options.Scripting.IgnoreReferenceTableColumnOrder)
            {
                return columns;
            }

            // If there is a source table and the option IgnoreSourceTableColumnOrder is set to false there are 2 sorting outcome:
            // 1) reference table is sorted alphabetically
            // 2) reference table is sorted by column ordinal position
            var referenceColumns = this.Options.Scripting.OrderColumnAlphabetically ?
                referenceTable.Columns.OrderBy(x => x.Name) :
                this.OrderColumnsByOrdinalPosition(referenceTable);

            var sortedColumns = new List<ABaseDbColumn>();

            // Navigate the referenceColumns sorted list
            foreach (var s in referenceColumns)
            {
                var c = columns.FirstOrDefault(x => string.Equals(s.Name, x.Name, StringComparison.OrdinalIgnoreCase));
                if (c != null)
                {
                    sortedColumns.Add(c);
                    columns.Remove(c);
                }
            }

            // Add remaining columns
            sortedColumns.AddRange(columns);

            return sortedColumns;
        }

        /// <summary>
        /// Order the table columns by their ordinal position
        /// </summary>
        /// <param name="table">The table with the columns to order</param>
        /// <returns>The list of columns ordered by ordinal position</returns>
        protected abstract IEnumerable<ABaseDbColumn> OrderColumnsByOrdinalPosition(ABaseDbTable table);
    }
}
