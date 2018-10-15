using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Project;
using SQLCompare.Core.Interfaces;
using SQLCompare.Services;

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
        public string GenerateObjectName(ABaseDbObject dbObject)
        {
            if (dbObject == null)
            {
                throw new ArgumentNullException(nameof(dbObject));
            }

            return this.ScriptHelper.ScriptObjectName(dbObject);
        }

        /// <inheritdoc/>
        public string GenerateFullScript(ABaseDb database)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            var sb = new StringBuilder();

            // Script the CREATE TYPE
            var userDefinedDataTypes = database.DataTypes.Where(x => x.IsUserDefined).ToList();
            if (userDefinedDataTypes.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelUserDefinedTypes));
                foreach (var userDataType in userDefinedDataTypes)
                {
                    sb.AppendLine(this.ScriptCreateType(userDataType, database.DataTypes));
                }

                sb.AppendLine();
            }

            // Script the CREATE SEQUENCE
            if (database.Sequences.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelSequences));
                foreach (var sequence in database.Sequences)
                {
                    sb.AppendLine(this.ScriptCreateSequence(sequence));
                }

                sb.AppendLine();
            }

            // Script the CREATE TABLE
            if (database.Tables.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelTables));
                foreach (var table in database.Tables)
                {
                    sb.AppendLine(this.ScriptCreateTable(table));
                }

                sb.AppendLine();
            }

            // Script the functions
            if (database.Functions.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelFunctions));
                foreach (var function in database.Functions)
                {
                    sb.Append(this.ScriptHelper.ScriptCommitTransaction());
                    sb.Append(this.ScriptCreateFunction(function, database.DataTypes));
                    sb.AppendLine();
                }

                sb.AppendLine();
            }

            // Script the stored procedures
            if (database.StoredProcedures.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelStoredProcedures));
                foreach (var storedProcedure in database.StoredProcedures)
                {
                    sb.Append(this.ScriptHelper.ScriptCommitTransaction());
                    sb.Append(this.ScriptCreateStoredProcedure(storedProcedure));
                    sb.AppendLine();
                }

                sb.AppendLine();
            }

            // Script the ALTER TABLE for primary keys and indexes
            var constraintsAndIndexes = database.Tables.Count > 0 &&
                                        database.Tables.Any(x => x.PrimaryKeys.Count > 0 ||
                                                                 x.Constraints.Count > 0 ||
                                                                 x.Indexes.Count > 0);
            if (constraintsAndIndexes)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelConstraintsAndIndexes));
                foreach (var table in database.Tables)
                {
                    sb.Append(this.ScriptPrimaryKeysAlterTable(table));
                    sb.Append(this.ScriptConstraintsAlterTable(table));
                    sb.Append(this.ScriptCreateIndexes(table, table.Indexes));
                }

                sb.AppendLine();
            }

            // Script the ALTER TABLE for foreign keys
            if (database.Tables.Count > 0 && database.Tables.Any(x => x.ForeignKeys.Count > 0))
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelForeignKeys));
                foreach (var table in database.Tables)
                {
                    sb.Append(this.ScriptForeignKeysAlterTable(table));
                }

                sb.AppendLine();
            }

            // Script the views
            if (database.Views.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelViews));
                foreach (var view in database.Views)
                {
                    sb.Append(this.ScriptHelper.ScriptCommitTransaction());
                    sb.AppendLine(this.ScriptCreateView(view));

                    if (view.Indexes.Count > 0)
                    {
                        sb.Append(this.ScriptCreateIndexes(view, view.Indexes));
                    }
                }

                sb.AppendLine();
            }

            // Script the triggers
            if (database.Triggers.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelTriggers));
                foreach (var trigger in database.Triggers)
                {
                    sb.Append(this.ScriptHelper.ScriptCommitTransaction());
                    sb.AppendLine(this.ScriptCreateTrigger(trigger));
                }

                sb.AppendLine();
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

            // Script the ALTER TABLE for primary keys and indexes
            var constraintsAndIndexes = table.PrimaryKeys.Count > 0 ||
                                         table.Constraints.Count > 0 ||
                                         table.Indexes.Count > 0;
            if (constraintsAndIndexes)
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelConstraintsAndIndexes));
                sb.Append(this.ScriptPrimaryKeysAlterTable(table));
                sb.Append(this.ScriptConstraintsAlterTable(table));
                sb.Append(this.ScriptCreateIndexes(table, table.Indexes));
            }

            // Script the ALTER TABLE for foreign keys
            if (table.ForeignKeys.Count > 0)
            {
                sb.AppendLine();
                if (!constraintsAndIndexes)
                {
                    sb.AppendLine();
                }

                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelForeignKeys));
                sb.Append(this.ScriptForeignKeysAlterTable(table));
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        public string GenerateCreateViewScript(ABaseDbView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            var sb = new StringBuilder();
            sb.Append(this.ScriptCreateView(view));

            if (view.Indexes.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelIndexes));
                sb.Append(this.ScriptCreateIndexes(view, view.Indexes));
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        public string GenerateAlterViewScript(ABaseDbView sourceView, ABaseDbView targetView)
        {
            if (sourceView == null && targetView == null)
            {
                throw new ArgumentNullException(nameof(sourceView));
            }

            if (targetView == null)
            {
                return this.GenerateCreateViewScript(sourceView);
            }

            return sourceView == null ?
                this.ScriptDropView(targetView) :
                this.ScriptAlterView(sourceView, targetView);
        }

        /// <inheritdoc/>
        public string GenerateCreateFunctionScript(ABaseDbFunction sqlFunction, IReadOnlyList<ABaseDbDataType> dataTypes)
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
        public string GenerateCreateStoredProcedureScript(ABaseDbStoredProcedure storedProcedure)
        {
            if (storedProcedure == null)
            {
                throw new ArgumentNullException(nameof(storedProcedure));
            }

            return this.ScriptCreateStoredProcedure(storedProcedure);
        }

        /// <inheritdoc/>
        public string GenerateCreateTriggerScript(ABaseDbTrigger trigger)
        {
            if (trigger == null)
            {
                throw new ArgumentNullException(nameof(trigger));
            }

            return this.ScriptCreateTrigger(trigger);
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

        /// <inheritdoc/>
        public string GenerateCreateTypeScript(ABaseDbDataType type, IReadOnlyList<ABaseDbDataType> dataTypes)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return this.ScriptCreateType(type, dataTypes);
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
        /// <param name="dbObject">The object to create the indexes</param>
        /// <param name="indexes">The list of indexes</param>
        /// <returns>The create index scripts</returns>
        protected abstract string ScriptCreateIndexes(ABaseDbObject dbObject, List<ABaseDbConstraint> indexes);

        /// <summary>
        /// Generates the create view script
        /// </summary>
        /// <param name="view">The view to script</param>
        /// <returns>The create view script</returns>
        protected abstract string ScriptCreateView(ABaseDbView view);

        /// <summary>
        /// Generates the drop view script
        /// </summary>
        /// <param name="view">The view to script</param>
        /// <returns>The drop view script</returns>
        protected abstract string ScriptDropView(ABaseDbView view);

        /// <summary>
        /// Generates the alter view script
        /// </summary>
        /// <param name="sourceView">The source view</param>
        /// <param name="targetView">The target view</param>
        /// <returns>The alter view script</returns>
        protected abstract string ScriptAlterView(ABaseDbView sourceView, ABaseDbView targetView);

        /// <summary>
        /// Generates the create function script
        /// </summary>
        /// <param name="sqlFunction">The function to script</param>
        /// <param name="dataTypes">The list of database data types</param>
        /// <returns>The create function script</returns>
        protected abstract string ScriptCreateFunction(ABaseDbFunction sqlFunction, IReadOnlyList<ABaseDbDataType> dataTypes);

        /// <summary>
        /// Generates the create stored procedure script
        /// </summary>
        /// <param name="storedProcedure">The stored procedure to script</param>
        /// <returns>The create stored procedure script</returns>
        protected abstract string ScriptCreateStoredProcedure(ABaseDbStoredProcedure storedProcedure);

        /// <summary>
        /// Generates the create trigger script
        /// </summary>
        /// <param name="trigger">The trigger to script</param>
        /// <returns>The create trigger script</returns>
        protected abstract string ScriptCreateTrigger(ABaseDbTrigger trigger);

        /// <summary>
        /// Generates the create sequence script
        /// </summary>
        /// <param name="sequence">The sequence to script</param>
        /// <returns>The create sequence script</returns>
        protected abstract string ScriptCreateSequence(ABaseDbSequence sequence);

        /// <summary>
        /// Generates the create type script
        /// </summary>
        /// <param name="type">The type to script</param>
        /// <param name="dataTypes">The list of database data types</param>
        /// <returns>The create type script</returns>
        protected abstract string ScriptCreateType(ABaseDbDataType type, IReadOnlyList<ABaseDbDataType> dataTypes);

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
