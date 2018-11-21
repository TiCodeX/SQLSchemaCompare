using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using TiCodeX.SQLSchemaCompare.Core.Entities.Compare;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
using TiCodeX.SQLSchemaCompare.Core.Entities.Project;
using TiCodeX.SQLSchemaCompare.Core.Interfaces;
using TiCodeX.SQLSchemaCompare.Services;

namespace TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters
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
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "TODO")]
        public string GenerateFullCreateScript(ABaseDb database)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            var sb = new StringBuilder();

            // User-Defined Types
            var userDefinedDataTypes = database.DataTypes.Where(x => x.IsUserDefined).ToList();
            if (userDefinedDataTypes.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelUserDefinedTypes));
                foreach (var userDataType in userDefinedDataTypes.OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.AppendLine(this.ScriptCreateType(userDataType, database.DataTypes));
                }

                sb.AppendLine();
            }

            // Sequences
            if (database.Sequences.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelSequences));
                foreach (var sequence in database.Sequences.OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.AppendLine(this.ScriptCreateSequence(sequence));
                }

                sb.AppendLine();
            }

            // Tables with PK, FK, Constraints
            if (database.Tables.Count > 0)
            {
                var sortedTables = this.GetSortedTables(database.Tables, false).ToList();

                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelTables));
                foreach (var table in sortedTables)
                {
                    sb.AppendLine(this.ScriptCreateTable(table));
                }

                sb.AppendLine();

                // Primary Keys
                if (sortedTables.Any(x => x.PrimaryKeys.Count > 0))
                {
                    sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelPrimaryKeys));
                    foreach (var table in sortedTables)
                    {
                        sb.Append(this.ScriptAlterTableAddPrimaryKeys(table));
                    }

                    sb.AppendLine();
                }

                // Foreign Keys
                if (sortedTables.Any(x => x.ForeignKeys.Count > 0))
                {
                    sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelForeignKeys));
                    foreach (var table in sortedTables)
                    {
                        sb.Append(this.ScriptAlterTableAddForeignKeys(table));
                    }

                    sb.AppendLine();
                }

                // Constraints
                if (sortedTables.Any(x => x.Constraints.Count > 0))
                {
                    sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelConstraints));
                    foreach (var table in sortedTables)
                    {
                        sb.Append(this.ScriptAlterTableAddConstraints(table));
                    }

                    sb.AppendLine();
                }
            }

            // Functions
            if (database.Functions.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelFunctions));
                foreach (var function in this.GetSortedFunctions(database.Functions, false))
                {
                    sb.Append(this.ScriptHelper.ScriptCommitTransaction());
                    sb.Append(this.ScriptCreateFunction(function, database.DataTypes));
                    sb.AppendLine();
                }

                sb.AppendLine();
            }

            // Stored Procedures
            if (database.StoredProcedures.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelStoredProcedures));
                foreach (var storedProcedure in database.StoredProcedures.OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.Append(this.ScriptHelper.ScriptCommitTransaction());
                    sb.Append(this.ScriptCreateStoredProcedure(storedProcedure));
                    sb.AppendLine();
                }

                sb.AppendLine();
            }

            // Views
            if (database.Views.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelViews));
                foreach (var view in database.Views.OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.Append(this.ScriptHelper.ScriptCommitTransaction());
                    sb.AppendLine(this.ScriptCreateView(view));
                }

                sb.AppendLine();
            }

            // Indexes
            if (database.Indexes.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelIndexes));
                foreach (var index in this.GetSortedIndexes(database.Indexes))
                {
                    sb.Append(this.ScriptCreateIndex(index));
                }

                sb.AppendLine();
            }

            // Triggers
            if (database.Triggers.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelTriggers));
                foreach (var trigger in database.Triggers.OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.Append(this.ScriptHelper.ScriptCommitTransaction());
                    sb.AppendLine(this.ScriptCreateTrigger(trigger));
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        public string GenerateFullAlterScript(List<ABaseCompareResultItem> differentItems, ABaseDb onlySourceItems, ABaseDb onlyTargetItems)
        {
            var sb = new StringBuilder();

            // Drop all items only on target
            sb.Append(this.GenerateFullDropScript(onlyTargetItems));

            // Alter the different items
            foreach (var trigger in differentItems.OfType<CompareResultItem<ABaseDbTrigger>>().OrderBy(x => x.SourceItem.TableSchema)
                .ThenBy(x => x.SourceItem.TableName).ThenBy(x => x.SourceItem.Schema).ThenBy(x => x.SourceItem.Name))
            {
                sb.Append(this.GenerateAlterTriggerScript(trigger.SourceItem, trigger.TargetItem));
            }

            foreach (var table in differentItems.OfType<CompareResultItem<ABaseDbTable>>().OrderBy(x => x.SourceItem.Schema).ThenBy(x => x.SourceItem.Name))
            {
                // sb.Append(this.GenerateAlterTableScript(table.SourceItem, table.TargetItem));
                // Columns
                // ForeignKeys
                // ReferencingForeignKeys
                // PrimaryKeys
                // Indexes
                // Constraints
            }

            foreach (var view in differentItems.OfType<CompareResultItem<ABaseDbView>>().OrderBy(x => x.SourceItem.Schema).ThenBy(x => x.SourceItem.Name))
            {
                sb.Append(this.GenerateAlterViewScript(view.SourceItem, view.TargetItem));
            }

            foreach (var function in differentItems.OfType<CompareResultItem<ABaseDbFunction>>().OrderBy(x => x.SourceItem.Schema).ThenBy(x => x.SourceItem.Name))
            {
                sb.Append(this.GenerateAlterFunctionScript(function.SourceItem, onlySourceItems.DataTypes, function.TargetItem, onlyTargetItems.DataTypes));
            }

            foreach (var storedProcedure in differentItems.OfType<CompareResultItem<ABaseDbStoredProcedure>>().OrderBy(x => x.SourceItem.Schema).ThenBy(x => x.SourceItem.Name))
            {
                sb.Append(this.GenerateAlterStoredProcedureScript(storedProcedure.SourceItem, storedProcedure.TargetItem));
            }

            foreach (var sequence in differentItems.OfType<CompareResultItem<ABaseDbSequence>>().OrderBy(x => x.SourceItem.Schema).ThenBy(x => x.SourceItem.Name))
            {
                sb.Append(this.GenerateAlterSequenceScript(sequence.SourceItem, sequence.TargetItem));
            }

            foreach (var type in differentItems.OfType<CompareResultItem<ABaseDbDataType>>().OrderBy(x => x.SourceItem.Schema).ThenBy(x => x.SourceItem.Name))
            {
                sb.Append(this.GenerateAlterTypeScript(type.SourceItem, onlySourceItems.DataTypes, type.TargetItem, onlyTargetItems.DataTypes));
            }

            // Create all items only on source
            sb.Append(this.GenerateFullCreateScript(onlySourceItems));

            return sb.ToString();
        }

        /// <inheritdoc/>
        public string GenerateFullDropScript(ABaseDb database)
        {
            var sb = new StringBuilder();

            // Triggers
            if (database.Triggers.Any())
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelTriggers));
                foreach (var trigger in database.Triggers.OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.Append(this.ScriptDropTrigger(trigger));
                }

                sb.AppendLine();
            }

            // Indexes
            if (database.Indexes.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelIndexes));
                foreach (var index in database.Indexes.OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.Append(this.ScriptDropIndex(index));
                }
            }

            // Views
            if (database.Views.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelViews));
                foreach (var view in database.Views.OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.Append(this.ScriptDropView(view));
                }

                sb.AppendLine();
            }

            // Functions
            if (database.Functions.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelFunctions));
                foreach (var function in this.GetSortedFunctions(database.Functions, true))
                {
                    sb.Append(this.ScriptDropFunction(function, database.DataTypes));
                }

                sb.AppendLine();
            }

            // Stored Procedures
            if (database.StoredProcedures.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelStoredProcedures));
                foreach (var storedProcedure in database.StoredProcedures.OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.Append(this.ScriptDropStoredProcedure(storedProcedure));
                }

                sb.AppendLine();
            }

            if (database.Tables.Count > 0)
            {
                var sortedTables = this.GetSortedTables(database.Tables, true).ToList();

                // Foreign Keys
                if (sortedTables.Any(x => x.ForeignKeys.Count > 0))
                {
                    sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelForeignKeys));
                    foreach (var table in sortedTables)
                    {
                        sb.Append(this.ScriptAlterTableDropForeignKeys(table));
                    }

                    sb.AppendLine();
                }

                // Constraints
                if (sortedTables.Any(x => x.Constraints.Count > 0))
                {
                    sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelConstraints));
                    foreach (var table in sortedTables)
                    {
                        sb.Append(this.ScriptAlterTableDropConstraints(table));
                    }

                    sb.AppendLine();
                }

                // Tables
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelTables));
                foreach (var table in sortedTables)
                {
                    sb.Append(this.ScriptDropTable(table));
                }

                sb.AppendLine();
            }

            // User-Defined Types
            var userDefinedDataTypes = database.DataTypes.Where(x => x.IsUserDefined).ToList();
            if (userDefinedDataTypes.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelUserDefinedTypes));
                foreach (var userDataType in userDefinedDataTypes.OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.Append(this.ScriptDropType(userDataType));
                }

                sb.AppendLine();
            }

            // Sequences
            if (database.Sequences.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelSequences));
                foreach (var sequence in database.Sequences.OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.Append(this.ScriptDropSequence(sequence));
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        public string GenerateCreateTableScript(ABaseDbTable table, ABaseDbTable referenceTable = null)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            var sb = new StringBuilder();

            sb.Append(this.ScriptCreateTable(table, referenceTable));

            var additionalEmptyLine = true;
            if (table.PrimaryKeys.Count > 0)
            {
                sb.AppendLine();
                if (additionalEmptyLine)
                {
                    sb.AppendLine();
                    additionalEmptyLine = false;
                }

                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelPrimaryKeys));
                sb.Append(this.ScriptAlterTableAddPrimaryKeys(table));
            }

            if (table.ForeignKeys.Count > 0)
            {
                sb.AppendLine();
                if (additionalEmptyLine)
                {
                    sb.AppendLine();
                    additionalEmptyLine = false;
                }

                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelForeignKeys));
                sb.Append(this.ScriptAlterTableAddForeignKeys(table));
            }

            if (table.Constraints.Count > 0)
            {
                sb.AppendLine();
                if (additionalEmptyLine)
                {
                    sb.AppendLine();
                    additionalEmptyLine = false;
                }

                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelConstraints));
                sb.Append(this.ScriptAlterTableAddConstraints(table));
            }

            if (table.Triggers.Count > 0)
            {
                sb.AppendLine();
                if (additionalEmptyLine)
                {
                    sb.AppendLine();
                    additionalEmptyLine = false;
                }

                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelTriggers));
                foreach (var trigger in table.Triggers.OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.AppendLine(this.ScriptCreateTrigger(trigger));
                }
            }

            if (table.Indexes.Count > 0)
            {
                sb.AppendLine();
                if (additionalEmptyLine)
                {
                    sb.AppendLine();
                    additionalEmptyLine = false;
                }

                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelIndexes));
                foreach (var index in this.GetSortedIndexes(table.Indexes))
                {
                    sb.Append(this.ScriptCreateIndex(index));
                }
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        public string GenerateDropTableScript(ABaseDbTable table)
        {
            var sb = new StringBuilder();

            if (table.Indexes.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelIndexes));
                foreach (var index in table.Indexes)
                {
                    sb.Append(this.ScriptDropIndex(index));
                }

                sb.AppendLine();
            }

            if (table.ReferencingForeignKeys.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelReferencingForeignKeys));
                sb.Append(this.ScriptAlterTableDropReferencingForeignKeys(table));
                sb.AppendLine();
            }

            if (table.PrimaryKeys.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelPrimaryKeys));
                sb.Append(this.ScriptAlterTableDropPrimaryKeys(table));
                sb.AppendLine();
            }

            if (table.ForeignKeys.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelForeignKeys));
                sb.Append(this.ScriptAlterTableDropForeignKeys(table));
                sb.AppendLine();
            }

            if (table.Constraints.Count > 0 || table.Columns.Any(x => !string.IsNullOrWhiteSpace(x.DefaultConstraintName)))
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelConstraints));
                sb.Append(this.ScriptAlterTableDropConstraints(table));
                sb.AppendLine();
            }

            if (table.Triggers.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelTriggers));
                foreach (var trigger in table.Triggers)
                {
                    sb.Append(this.ScriptDropTrigger(trigger));
                }

                sb.AppendLine();
            }

            sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelTable));
            sb.Append(this.ScriptDropTable(table));

            return sb.ToString();
        }

        /// <inheritdoc/>
        public string GenerateAlterTableScript(ABaseDbTable sourceTable, ABaseDbTable targetTable)
        {
            if (sourceTable == null && targetTable == null)
            {
                throw new ArgumentNullException(nameof(sourceTable));
            }

            if (targetTable == null)
            {
                return this.GenerateCreateTableScript(sourceTable);
            }

            if (sourceTable == null)
            {
                return this.GenerateDropTableScript(targetTable);
            }

            var sb = new StringBuilder();
            sb.AppendLine("TODO: Alter Table Script");

            return sb.ToString();
        }

        /// <inheritdoc/>
        public string GenerateCreateIndexScript(ABaseDbIndex index)
        {
            if (index == null)
            {
                throw new ArgumentNullException(nameof(index));
            }

            var sb = new StringBuilder();
            sb.Append(this.ScriptCreateIndex(index));
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
                foreach (var index in this.GetSortedIndexes(view.Indexes))
                {
                    sb.Append(this.ScriptCreateIndex(index));
                }
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        public string GenerateDropViewScript(ABaseDbView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            var sb = new StringBuilder();

            foreach (var index in view.Indexes)
            {
                sb.Append(this.ScriptDropIndex(index));
            }

            sb.Append(this.ScriptDropView(view));

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
                this.GenerateDropViewScript(targetView) :
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
        public string GenerateAlterFunctionScript(ABaseDbFunction sourceFunction, IReadOnlyList<ABaseDbDataType> sourceDataTypes, ABaseDbFunction targetFunction, IReadOnlyList<ABaseDbDataType> targetDataTypes)
        {
            if (sourceFunction == null && targetFunction == null)
            {
                throw new ArgumentNullException(nameof(sourceFunction));
            }

            if (targetFunction == null)
            {
                return this.GenerateCreateFunctionScript(sourceFunction, sourceDataTypes);
            }

            return sourceFunction == null ?
                this.ScriptDropFunction(targetFunction, sourceDataTypes) :
                this.ScriptAlterFunction(sourceFunction, targetFunction, targetDataTypes);
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
        public string GenerateAlterStoredProcedureScript(ABaseDbStoredProcedure sourceStoredProcedure, ABaseDbStoredProcedure targetStoredProcedure)
        {
            if (sourceStoredProcedure == null && targetStoredProcedure == null)
            {
                throw new ArgumentNullException(nameof(sourceStoredProcedure));
            }

            if (targetStoredProcedure == null)
            {
                return this.GenerateCreateStoredProcedureScript(sourceStoredProcedure);
            }

            return sourceStoredProcedure == null ?
                this.ScriptDropStoredProcedure(targetStoredProcedure) :
                this.ScriptAlterStoredProcedure(sourceStoredProcedure, targetStoredProcedure);
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
        public string GenerateAlterTriggerScript(ABaseDbTrigger sourceTrigger, ABaseDbTrigger targetTrigger)
        {
            if (sourceTrigger == null && targetTrigger == null)
            {
                throw new ArgumentNullException(nameof(sourceTrigger));
            }

            if (targetTrigger == null)
            {
                return this.GenerateCreateTriggerScript(sourceTrigger);
            }

            return sourceTrigger == null ?
                this.ScriptDropTrigger(targetTrigger) :
                this.ScriptAlterTrigger(sourceTrigger, targetTrigger);
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
        public string GenerateAlterSequenceScript(ABaseDbSequence sourceSequence, ABaseDbSequence targetSequence)
        {
            if (sourceSequence == null && targetSequence == null)
            {
                throw new ArgumentNullException(nameof(sourceSequence));
            }

            if (targetSequence == null)
            {
                return this.GenerateCreateSequenceScript(sourceSequence);
            }

            return sourceSequence == null ?
                this.ScriptDropSequence(targetSequence) :
                this.ScriptAlterSequence(sourceSequence, targetSequence);
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

        /// <inheritdoc/>
        public string GenerateAlterTypeScript(ABaseDbDataType sourceType, IReadOnlyList<ABaseDbDataType> sourceDataTypes, ABaseDbDataType targetType, IReadOnlyList<ABaseDbDataType> targetDataTypes)
        {
            if (sourceType == null && targetType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            if (targetType == null)
            {
                return this.GenerateCreateTypeScript(sourceType, sourceDataTypes);
            }

            return sourceType == null ?
                this.ScriptDropType(targetType) :
                this.ScriptAlterType(sourceType, targetType, sourceDataTypes);
        }

        /// <summary>
        /// Generates the create table script
        /// </summary>
        /// <param name="table">The table to script</param>
        /// <param name="referenceTable">The reference table for comparison, used for column order</param>
        /// <returns>The create table script</returns>
        protected abstract string ScriptCreateTable(ABaseDbTable table, ABaseDbTable referenceTable = null);

        /// <summary>
        /// Generates the drop table script
        /// </summary>
        /// <param name="table">The table to drop</param>
        /// <returns>The drop table script</returns>
        protected abstract string ScriptDropTable(ABaseDbTable table);

        /// <summary>
        /// Generates the alter table for adding the primary keys to the table
        /// </summary>
        /// <param name="table">The table to alter</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTableAddPrimaryKeys(ABaseDbTable table);

        /// <summary>
        /// Generates the alter table for dropping the primary keys from the table
        /// </summary>
        /// <param name="table">The table to alter</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTableDropPrimaryKeys(ABaseDbTable table);

        /// <summary>
        /// Generates the alter table for adding foreign keys to the table
        /// </summary>
        /// <param name="table">The table to alter</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTableAddForeignKeys(ABaseDbTable table);

        /// <summary>
        /// Generates the alter table for dropping the foreign keys from the table
        /// </summary>
        /// <param name="table">The table to alter</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTableDropForeignKeys(ABaseDbTable table);

        /// <summary>
        /// Generates the alter table for dropping the foreign keys referencing the table
        /// </summary>
        /// <param name="table">The table to alter</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTableDropReferencingForeignKeys(ABaseDbTable table);

        /// <summary>
        /// Generates the alter table for adding the constraints to the table
        /// </summary>
        /// <param name="table">The table to alter</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTableAddConstraints(ABaseDbTable table);

        /// <summary>
        /// Generates the alter table for dropping the constraints from the table
        /// </summary>
        /// <param name="table">The table to alter</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTableDropConstraints(ABaseDbTable table);

        /// <summary>
        /// Generates the create index script
        /// </summary>
        /// <param name="index">The index to script</param>
        /// <returns>The create index script</returns>
        protected abstract string ScriptCreateIndex(ABaseDbIndex index);

        /// <summary>
        /// Generates the drop index script
        /// </summary>
        /// <param name="index">The index to script</param>
        /// <returns>The drop index scripts</returns>
        protected abstract string ScriptDropIndex(ABaseDbIndex index);

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
        /// Generates the drop function script
        /// </summary>
        /// <param name="sqlFunction">The function to script</param>
        /// <param name="dataTypes">The list of database data types</param>
        /// <returns>The drop function script</returns>
        protected abstract string ScriptDropFunction(ABaseDbFunction sqlFunction, IReadOnlyList<ABaseDbDataType> dataTypes);

        /// <summary>
        /// Generates the alter function script
        /// </summary>
        /// <param name="sourceFunction">The source function</param>
        /// <param name="targetFunction">The target function</param>
        /// <param name="dataTypes">The list of database data types</param>
        /// <returns>The alter function script</returns>
        protected abstract string ScriptAlterFunction(ABaseDbFunction sourceFunction, ABaseDbFunction targetFunction, IReadOnlyList<ABaseDbDataType> dataTypes);

        /// <summary>
        /// Generates the create stored procedure script
        /// </summary>
        /// <param name="storedProcedure">The stored procedure to script</param>
        /// <returns>The create stored procedure script</returns>
        protected abstract string ScriptCreateStoredProcedure(ABaseDbStoredProcedure storedProcedure);

        /// <summary>
        /// Generates the drop stored procedure script
        /// </summary>
        /// <param name="storedProcedure">The stored procedure</param>
        /// <returns>The drop stored procedure script</returns>
        protected abstract string ScriptDropStoredProcedure(ABaseDbStoredProcedure storedProcedure);

        /// <summary>
        /// Generates the alter stored procedure script
        /// </summary>
        /// <param name="sourceStoredProcedure">The source stored procedure</param>
        /// <param name="targetStoredProcedure">The target stored procedure</param>
        /// <returns>The alter stored procedure script</returns>
        protected abstract string ScriptAlterStoredProcedure(ABaseDbStoredProcedure sourceStoredProcedure, ABaseDbStoredProcedure targetStoredProcedure);

        /// <summary>
        /// Generates the create trigger script
        /// </summary>
        /// <param name="trigger">The trigger to script</param>
        /// <returns>The create trigger script</returns>
        protected abstract string ScriptCreateTrigger(ABaseDbTrigger trigger);

        /// <summary>
        /// Generates the drop trigger script
        /// </summary>
        /// <param name="trigger">The trigger</param>
        /// <returns>The drop trigger script</returns>
        protected abstract string ScriptDropTrigger(ABaseDbTrigger trigger);

        /// <summary>
        /// Generates the alter trigger script
        /// </summary>
        /// <param name="sourceTrigger">The source trigger</param>
        /// <param name="targetTrigger">The target trigger</param>
        /// <returns>The alter trigger script</returns>
        protected abstract string ScriptAlterTrigger(ABaseDbTrigger sourceTrigger, ABaseDbTrigger targetTrigger);

        /// <summary>
        /// Generates the create sequence script
        /// </summary>
        /// <param name="sequence">The sequence to script</param>
        /// <returns>The create sequence script</returns>
        protected abstract string ScriptCreateSequence(ABaseDbSequence sequence);

        /// <summary>
        /// Generates the drop sequence script
        /// </summary>
        /// <param name="sequence">The sequence</param>
        /// <returns>The drop sequence script</returns>
        protected abstract string ScriptDropSequence(ABaseDbSequence sequence);

        /// <summary>
        /// Generates the alter sequence script
        /// </summary>
        /// <param name="sourceSequence">The source sequence</param>
        /// <param name="targetSequence">The target sequence</param>
        /// <returns>The alter sequence script</returns>
        protected abstract string ScriptAlterSequence(ABaseDbSequence sourceSequence, ABaseDbSequence targetSequence);

        /// <summary>
        /// Generates the create type script
        /// </summary>
        /// <param name="type">The type to script</param>
        /// <param name="dataTypes">The list of database data types</param>
        /// <returns>The create type script</returns>
        protected abstract string ScriptCreateType(ABaseDbDataType type, IReadOnlyList<ABaseDbDataType> dataTypes);

        /// <summary>
        /// Generates the drop type script
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The drop type script</returns>
        protected abstract string ScriptDropType(ABaseDbDataType type);

        /// <summary>
        /// Generates the alter type script
        /// </summary>
        /// <param name="sourceType">The source type</param>
        /// <param name="targetType">The target type</param>
        /// <param name="dataTypes">The list of database data types</param>
        /// <returns>The alter type script</returns>
        protected abstract string ScriptAlterType(ABaseDbDataType sourceType, ABaseDbDataType targetType, IReadOnlyList<ABaseDbDataType> dataTypes);

        /// <summary>
        /// Gets the tables sorted depending on the options
        /// </summary>
        /// <param name="tables">The tables</param>
        /// <param name="dropOrder">Whether to sort the tables for dropping them</param>
        /// <returns>The sorted tables</returns>
        protected abstract IEnumerable<ABaseDbTable> GetSortedTables(List<ABaseDbTable> tables, bool dropOrder);

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

        /// <summary>
        /// Gets the functions sorted depending on the options
        /// </summary>
        /// <param name="functions">The functions</param>
        /// <param name="dropOrder">Whether to sort the functions for dropping them</param>
        /// <returns>The sorted functions</returns>
        protected abstract IEnumerable<ABaseDbFunction> GetSortedFunctions(List<ABaseDbFunction> functions, bool dropOrder);

        /// <summary>
        /// Gets the indexes sorted depending on the options
        /// </summary>
        /// <param name="indexes">The indexes</param>
        /// <returns>The sorted indexes</returns>
        protected abstract IEnumerable<ABaseDbIndex> GetSortedIndexes(List<ABaseDbIndex> indexes);
    }
}
