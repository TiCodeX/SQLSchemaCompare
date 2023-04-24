namespace TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Compare;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Project;
    using TiCodeX.SQLSchemaCompare.Core.Extensions;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces;
    using TiCodeX.SQLSchemaCompare.Services;

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

            // Schemas
            if (database.Schemas.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelSchemas));
                foreach (var schema in database.Schemas.OrderBy(x => x.Name))
                {
                    sb.Append(this.ScriptCreateSchema(schema));
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
            }

            // Primary Keys
            if (database.PrimaryKeys.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelPrimaryKeys));
                foreach (var primaryKey in database.PrimaryKeys.OrderBy(x => x.TableSchema).ThenBy(x => x.TableName).ThenBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.Append(this.ScriptAlterTableAddPrimaryKey(primaryKey));
                }

                sb.AppendLine();
            }

            // Foreign Keys
            if (database.ForeignKeys.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelForeignKeys));
                foreach (var foreignKey in database.ForeignKeys.OrderBy(x => x.TableSchema).ThenBy(x => x.TableName).ThenBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.Append(this.ScriptAlterTableAddForeignKey(foreignKey));
                }

                sb.AppendLine();
            }

            // Constraints
            if (database.Constraints.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelConstraints));
                foreach (var constraint in database.Constraints.OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.Append(this.ScriptAlterTableAddConstraint(constraint));
                }

                sb.AppendLine();
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
                    sb.AppendLine(this.ScriptCreateTrigger(trigger));
                }

                sb.AppendLine();
            }

            // Periods
            if (database.Tables.Any(x => x.HasPeriod))
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelPeriods));
                foreach (var table in database.Tables.Where(x => x.HasPeriod).OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.AppendLine(this.ScriptAlterTableAddPeriod(table));
                }
            }

            // Histories
            if (database.Tables.Any(x => x.HasHistoryTable))
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelHistories));
                foreach (var table in database.Tables.Where(x => x.HasHistoryTable).OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.AppendLine(this.ScriptAlterTableAddHistory(table));
                }
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        public string GenerateFullAlterScript(List<ABaseCompareResultItem> differentItems, ABaseDb onlySourceItems, ABaseDb onlyTargetItems)
        {
            var sb = new StringBuilder();

            // Drop all items only on target
            sb.Append(this.GenerateFullDropScript(onlyTargetItems));

            var items = new List<ABaseDbObject>();
            items.AddRange(differentItems.OfType<CompareResultItem<ABaseDbSchema>>().OrderBy(x => x.SourceItem.Name).Select(x => x.SourceItem ?? x.TargetItem));
            items.AddRange(differentItems.OfType<CompareResultItem<ABaseDbTrigger>>().OrderBy(x => x.SourceItem.TableSchema).ThenBy(x => x.SourceItem.TableName).ThenBy(x => x.SourceItem.Schema).ThenBy(x => x.SourceItem.Name).Select(x => x.SourceItem ?? x.TargetItem));
            items.AddRange(differentItems.OfType<CompareResultItem<ABaseDbTable>>().OrderBy(x => x.SourceItem.Schema).ThenBy(x => x.SourceItem.Name).Select(x => x.SourceItem ?? x.TargetItem));
            items.AddRange(differentItems.OfType<CompareResultItem<ABaseDbView>>().OrderBy(x => x.SourceItem.Schema).ThenBy(x => x.SourceItem.Name).Select(x => x.SourceItem ?? x.TargetItem));
            items.AddRange(differentItems.OfType<CompareResultItem<ABaseDbFunction>>().OrderBy(x => x.SourceItem.Schema).ThenBy(x => x.SourceItem.Name).Select(x => x.SourceItem ?? x.TargetItem));
            items.AddRange(differentItems.OfType<CompareResultItem<ABaseDbStoredProcedure>>().OrderBy(x => x.SourceItem.Schema).ThenBy(x => x.SourceItem.Name).Select(x => x.SourceItem ?? x.TargetItem));
            items.AddRange(differentItems.OfType<CompareResultItem<ABaseDbSequence>>().OrderBy(x => x.SourceItem.Schema).ThenBy(x => x.SourceItem.Name).Select(x => x.SourceItem ?? x.TargetItem));
            items.AddRange(differentItems.OfType<CompareResultItem<ABaseDbDataType>>().OrderBy(x => x.SourceItem.Schema).ThenBy(x => x.SourceItem.Name).Select(x => x.SourceItem ?? x.TargetItem));
            foreach (var item in items)
            {
                sb.Append(this.GenerateAlterScript(item, false));
            }

            // Create all items only on source
            sb.Append(this.GenerateFullCreateScript(onlySourceItems));

            return sb.ToString();
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "TODO")]
        public string GenerateFullDropScript(ABaseDb database)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            var sb = new StringBuilder();

            // Histories
            if (database.Tables.Any(x => x.HasHistoryTable))
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelHistories));
                foreach (var table in database.Tables.Where(x => x.HasHistoryTable).OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.AppendLine(this.ScriptAlterTableDropHistory(table));
                }
            }

            // Periods
            if (database.Tables.Any(x => x.HasPeriod))
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelPeriods));
                foreach (var table in database.Tables.Where(x => x.HasPeriod).OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.AppendLine(this.ScriptAlterTableDropPeriod(table));
                }
            }

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

            // ForeignKeys (must be before indexes because in MySQL you can not drop a index required by the foreign key)
            if (database.ForeignKeys.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelForeignKeys));
                foreach (var foreignKey in database.ForeignKeys.OrderBy(x => x.TableSchema).ThenBy(x => x.TableName).ThenBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.Append(this.ScriptAlterTableDropForeignKey(foreignKey));
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

                sb.AppendLine();
            }

            // PrimaryKeys
            if (database.PrimaryKeys.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelPrimaryKeys));
                foreach (var primaryKey in database.PrimaryKeys.OrderBy(x => x.TableSchema).ThenBy(x => x.TableName).ThenBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.Append(this.ScriptAlterTableDropPrimaryKey(primaryKey));
                }

                sb.AppendLine();
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

            // Constraints
            if (database.Constraints.Count > 0 || database.Tables.Any(y => y.Columns.Any(x => !string.IsNullOrWhiteSpace(x.DefaultConstraintName))))
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelConstraints));
                foreach (var constraint in database.Constraints)
                {
                    sb.Append(this.ScriptAlterTableDropConstraint(constraint));
                }

                foreach (var table in database.Tables.OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    foreach (var column in table.Columns.Where(x => !string.IsNullOrWhiteSpace(x.DefaultConstraintName)).OrderBy(x => x.DefaultConstraintName))
                    {
                        var constraint = new ABaseDbConstraint
                        {
                            TableSchema = table.Schema,
                            TableName = table.Name,
                            Name = column.DefaultConstraintName,
                        };

                        sb.Append(this.ScriptAlterTableDropConstraint(constraint));
                    }
                }

                sb.AppendLine();
            }

            if (database.Tables.Count > 0)
            {
                var sortedTables = this.GetSortedTables(database.Tables, true).ToList();

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
                foreach (var sequence in database.Sequences.Where(x => !x.IsAutoGenerated).OrderBy(x => x.Schema).ThenBy(x => x.Name))
                {
                    sb.Append(this.ScriptDropSequence(sequence));
                }

                sb.AppendLine();
            }

            // Schemas
            if (database.Schemas.Count > 0)
            {
                sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelSchemas));
                foreach (var schema in database.Schemas.OrderBy(x => x.Name))
                {
                    sb.Append(this.ScriptDropSchema(schema));
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        public string GenerateCreateScript(ABaseDbObject dbObject, bool includeChildDbObjects)
        {
            var scriptableObjects = new List<ObjectMap>();
            switch (dbObject)
            {
                case ABaseDbSchema s:
                    return this.ScriptCreateSchema(s);

                case ABaseDbTable t:
                    if (!includeChildDbObjects)
                    {
                        return this.ScriptCreateTable(t);
                    }

                    scriptableObjects.Add(new ObjectMap { DbObjects = new[] { t } });
                    scriptableObjects.Add(new ObjectMap { ObjectTitle = Localization.LabelPrimaryKeys, DbObjects = t.PrimaryKeys.OrderBy(x => x.Schema).ThenBy(x => x.Name) });
                    scriptableObjects.Add(new ObjectMap { ObjectTitle = Localization.LabelForeignKeys, DbObjects = t.ForeignKeys.OrderBy(x => x.Schema).ThenBy(x => x.Name) });
                    scriptableObjects.Add(new ObjectMap { ObjectTitle = Localization.LabelConstraints, DbObjects = t.Constraints.OrderBy(x => x.Schema).ThenBy(x => x.Name) });
                    scriptableObjects.Add(new ObjectMap { ObjectTitle = Localization.LabelTriggers, DbObjects = t.Triggers.OrderBy(x => x.Schema).ThenBy(x => x.Name) });
                    scriptableObjects.Add(new ObjectMap { ObjectTitle = Localization.LabelIndexes, DbObjects = this.GetSortedIndexes(t.Indexes) });

                    var sb = new StringBuilder();
                    sb.Append(GenerateObjectMapScript(scriptableObjects, this.GenerateCreateScript));

                    if (t.HasPeriod)
                    {
                        sb.AppendLineIfNotEmpty();
                        sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelPeriod));
                        sb.Append(this.ScriptAlterTableAddPeriod(t));
                    }

                    if (t.HasHistoryTable)
                    {
                        sb.AppendLineIfNotEmpty();
                        sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelHistory));
                        sb.Append(this.ScriptAlterTableAddHistory(t));
                    }

                    return sb.ToString();

                case ABaseDbView v:
                    if (!includeChildDbObjects)
                    {
                        return this.ScriptCreateView(v);
                    }

                    scriptableObjects.Add(new ObjectMap { DbObjects = new[] { v } });
                    scriptableObjects.Add(new ObjectMap { ObjectTitle = Localization.LabelIndexes, DbObjects = this.GetSortedIndexes(v.Indexes) });

                    return GenerateObjectMapScript(scriptableObjects, this.GenerateCreateScript);

                case ABaseDbPrimaryKey pk:
                    return this.ScriptAlterTableAddPrimaryKey(pk);
                case ABaseDbIndex i:
                    return this.ScriptCreateIndex(i);
                case ABaseDbForeignKey fk:
                    return this.ScriptAlterTableAddForeignKey(fk);
                case ABaseDbConstraint c:
                    return this.ScriptAlterTableAddConstraint(c);
                case ABaseDbFunction f:
                    return this.ScriptCreateFunction(f, f.Database.DataTypes);
                case ABaseDbSequence s:
                    return this.ScriptCreateSequence(s);
                case ABaseDbStoredProcedure st:
                    return this.ScriptCreateStoredProcedure(st);
                case ABaseDbTrigger tr:
                    return this.ScriptCreateTrigger(tr);
                case ABaseDbDataType dt:
                    return this.ScriptCreateType(dt, dt.Database.DataTypes);
                case ABaseDbColumn c:
                    return this.ScriptHelper.ScriptColumn(c);
                default:
                    throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        public string GenerateDropScript(ABaseDbObject dbObject, bool includeChildDbObjects)
        {
            var scriptableObjects = new List<ObjectMap>();
            switch (dbObject)
            {
                case ABaseDbSchema s:
                    return this.ScriptDropSchema(s);

                case ABaseDbTable t:
                    if (!includeChildDbObjects)
                    {
                        return this.ScriptDropTable(t);
                    }

                    scriptableObjects.Add(new ObjectMap { ObjectTitle = Localization.LabelIndexes, DbObjects = this.GetSortedIndexes(t.Indexes) });
                    scriptableObjects.Add(new ObjectMap { ObjectTitle = Localization.LabelReferencingForeignKeys, DbObjects = t.ReferencingForeignKeys.OrderBy(x => x.Schema).ThenBy(x => x.Name) });
                    scriptableObjects.Add(new ObjectMap { ObjectTitle = Localization.LabelPrimaryKeys, DbObjects = t.PrimaryKeys.OrderBy(x => x.Schema).ThenBy(x => x.Name) });
                    scriptableObjects.Add(new ObjectMap { ObjectTitle = Localization.LabelForeignKeys, DbObjects = t.ForeignKeys.OrderBy(x => x.Schema).ThenBy(x => x.Name) });

                    var defaultConstraints = new List<ABaseDbConstraint>();
                    if (t.Columns.Any(x => !string.IsNullOrWhiteSpace(x.DefaultConstraintName)))
                    {
                        foreach (var column in t.Columns.Where(x => !string.IsNullOrWhiteSpace(x.DefaultConstraintName)).OrderBy(x => x.DefaultConstraintName))
                        {
                            defaultConstraints.Add(new ABaseDbConstraint
                            {
                                TableSchema = t.Schema,
                                TableName = t.Name,
                                Name = column.DefaultConstraintName,
                            });
                        }
                    }

                    scriptableObjects.Add(new ObjectMap
                    {
                        ObjectTitle = Localization.LabelConstraints,
                        DbObjects = t.Constraints.OrderBy(x => x.Schema).ThenBy(x => x.Name).Concat(defaultConstraints.OrderBy(x => x.Schema).ThenBy(x => x.Name)),
                    });
                    scriptableObjects.Add(new ObjectMap { ObjectTitle = Localization.LabelTriggers, DbObjects = t.Triggers.OrderBy(x => x.Schema).ThenBy(x => x.Name) });
                    scriptableObjects.Add(new ObjectMap { DbObjects = new[] { t } });

                    var sb = new StringBuilder();

                    if (t.HasHistoryTable)
                    {
                        sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelHistory));
                        sb.AppendLine(this.ScriptAlterTableDropHistory(t));
                    }

                    if (t.HasPeriod)
                    {
                        sb.AppendLine(AScriptHelper.ScriptComment(Localization.LabelPeriod));
                        sb.AppendLine(this.ScriptAlterTableDropPeriod(t));
                    }

                    sb.Append(GenerateObjectMapScript(scriptableObjects, this.GenerateDropScript));

                    return sb.ToString();

                case ABaseDbView v:
                    if (!includeChildDbObjects)
                    {
                        return this.ScriptDropView(v);
                    }

                    scriptableObjects.Add(new ObjectMap { ObjectTitle = Localization.LabelIndexes, DbObjects = this.GetSortedIndexes(v.Indexes) });
                    scriptableObjects.Add(new ObjectMap { DbObjects = new[] { v } });

                    return GenerateObjectMapScript(scriptableObjects, this.GenerateDropScript);

                case ABaseDbPrimaryKey pk:
                    return this.ScriptAlterTableDropPrimaryKey(pk);
                case ABaseDbIndex i:
                    return this.ScriptDropIndex(i);
                case ABaseDbForeignKey fk:
                    return this.ScriptAlterTableDropForeignKey(fk);
                case ABaseDbConstraint c:
                    return this.ScriptAlterTableDropConstraint(c);
                case ABaseDbFunction f:
                    return this.ScriptDropFunction(f, f.Database.DataTypes);
                case ABaseDbSequence s:
                    return this.ScriptDropSequence(s);
                case ABaseDbStoredProcedure st:
                    return this.ScriptDropStoredProcedure(st);
                case ABaseDbTrigger tr:
                    return this.ScriptDropTrigger(tr);
                case ABaseDbDataType dt:
                    return this.ScriptDropType(dt);
                case ABaseDbColumn:
                    throw new NotImplementedException();
                default:
                    throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        public string GenerateAlterScript(ABaseDbObject dbObject, bool includeChildDbObjects)
        {
            if (dbObject == null)
            {
                throw new ArgumentNullException(nameof(dbObject));
            }

            if (dbObject.Database.Direction == Core.Enums.CompareDirection.Target)
            {
                return this.GenerateDropScript(dbObject, includeChildDbObjects);
            }

            if (dbObject.MappedDbObject == null)
            {
                return this.GenerateCreateScript(dbObject, includeChildDbObjects);
            }

            if (dbObject.CreateScript == dbObject.MappedDbObject.CreateScript)
            {
                return string.Empty;
            }

            switch (dbObject)
            {
                case ABaseDbSchema s:
                    return this.ScriptAlterSchema(s);

                case ABaseDbTable t:
                    return includeChildDbObjects ?
                        this.ScriptAlterTableAndChildDbObjects(t) :
                        this.ScriptAlterTable(t);

                case ABaseDbView v:
                    return this.ScriptAlterView(v, v.MappedDbObject as ABaseDbView);
                case ABaseDbPrimaryKey pk:
                    return this.ScriptAlterPrimaryKey(pk, pk.MappedDbObject as ABaseDbPrimaryKey);
                case ABaseDbIndex i:
                    return this.ScriptAlterIndex(i, i.MappedDbObject as ABaseDbIndex);
                case ABaseDbForeignKey fk:
                    return this.ScriptAlterForeignKey(fk, fk.MappedDbObject as ABaseDbForeignKey);
                case ABaseDbConstraint c:
                    return this.ScriptAlterConstraint(c, c.MappedDbObject as ABaseDbConstraint);
                case ABaseDbFunction f:
                    return this.ScriptAlterFunction(f, f.MappedDbObject as ABaseDbFunction, f.MappedDbObject?.Database.DataTypes);
                case ABaseDbSequence s:
                    return this.ScriptAlterSequence(s, s.MappedDbObject as ABaseDbSequence);
                case ABaseDbStoredProcedure st:
                    return this.ScriptAlterStoredProcedure(st, st.MappedDbObject as ABaseDbStoredProcedure);
                case ABaseDbTrigger tr:
                    return this.ScriptAlterTrigger(tr, tr.MappedDbObject as ABaseDbTrigger);
                case ABaseDbDataType dt:
                    return this.ScriptAlterType(dt, dt.MappedDbObject as ABaseDbDataType, dt.Database.DataTypes);
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Generates the create schema script
        /// </summary>
        /// <param name="schema">The schema to script</param>
        /// <returns>The create schema script</returns>
        protected abstract string ScriptCreateSchema(ABaseDbSchema schema);

        /// <summary>
        /// Generates the drop schema script
        /// </summary>
        /// <param name="schema">The schema to drop</param>
        /// <returns>The drop schema script</returns>
        protected abstract string ScriptDropSchema(ABaseDbSchema schema);

        /// <summary>
        /// Generates the alter schema script
        /// </summary>
        /// <param name="schema">the schema to alter</param>
        /// <returns>The alter schema script</returns>
        protected abstract string ScriptAlterSchema(ABaseDbSchema schema);

        /// <summary>
        /// Generates the create table script
        /// </summary>
        /// <param name="table">The table to script</param>
        /// <returns>The create table script</returns>
        protected abstract string ScriptCreateTable(ABaseDbTable table);

        /// <summary>
        /// Generates the drop table script
        /// </summary>
        /// <param name="table">The table to drop</param>
        /// <returns>The drop table script</returns>
        protected abstract string ScriptDropTable(ABaseDbTable table);

        /// <summary>
        /// Generates the alter table script
        /// </summary>
        /// <param name="t">the table to alter</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTable(ABaseDbTable t);

        /// <summary>
        /// Generates the alter table for adding the primary key to the table
        /// </summary>
        /// <param name="primaryKey">The primary key to script</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTableAddPrimaryKey(ABaseDbPrimaryKey primaryKey);

        /// <summary>
        /// Generates the alter table for dropping the primary key from the table
        /// </summary>
        /// <param name="primaryKey">The primary key to drop</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTableDropPrimaryKey(ABaseDbPrimaryKey primaryKey);

        /// <summary>
        /// Generates the alter table for altering the primary key from the table
        /// </summary>
        /// <param name="sourcePrimaryKey">The source primary key to add</param>
        /// <param name="targetPrimaryKey">The target primary key to drop</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterPrimaryKey(ABaseDbPrimaryKey sourcePrimaryKey, ABaseDbPrimaryKey targetPrimaryKey);

        /// <summary>
        /// Generates the alter table for adding the foreign key to the table
        /// </summary>
        /// <param name="foreignKey">The foreign key to script</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTableAddForeignKey(ABaseDbForeignKey foreignKey);

        /// <summary>
        /// Generates the alter table for dropping the foreign key from the table
        /// </summary>
        /// <param name="foreignKey">The foreign key to drop</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTableDropForeignKey(ABaseDbForeignKey foreignKey);

        /// <summary>
        /// Generates the alter table for altering the foreign key from the table
        /// </summary>
        /// <param name="sourceForeignKey">The source foreign key to add</param>
        /// <param name="targetForeignKey">The target foreign key to drop</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterForeignKey(ABaseDbForeignKey sourceForeignKey, ABaseDbForeignKey targetForeignKey);

        /// <summary>
        /// Generates the alter table for adding the constraint to the table
        /// </summary>
        /// <param name="constraint">The constraint to script</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTableAddConstraint(ABaseDbConstraint constraint);

        /// <summary>
        /// Generates the alter table for dropping the constraint from the table
        /// </summary>
        /// <param name="constraint">The constraint to drop</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTableDropConstraint(ABaseDbConstraint constraint);

        /// <summary>
        /// Generates the alter table for altering the constraint from the table
        /// </summary>
        /// <param name="sourceConstraint">The source constraint to add</param>
        /// <param name="targetConstraint">The target constraint to drop</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterConstraint(ABaseDbConstraint sourceConstraint, ABaseDbConstraint targetConstraint);

        /// <summary>
        /// Generates the alter table for adding the period
        /// </summary>
        /// <param name="table">The table</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTableAddPeriod(ABaseDbTable table);

        /// <summary>
        /// Generates the alter table for dropping the period
        /// </summary>
        /// <param name="table">The table</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTableDropPeriod(ABaseDbTable table);

        /// <summary>
        /// Generates the alter table for altering the period
        /// </summary>
        /// <param name="sourceTable">The source table</param>
        /// <param name="targetTable">The target table</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterPeriod(ABaseDbTable sourceTable, ABaseDbTable targetTable);

        /// <summary>
        /// Generates the alter table for adding the history
        /// </summary>
        /// <param name="table">The table</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTableAddHistory(ABaseDbTable table);

        /// <summary>
        /// Generates the alter table for dropping the history
        /// </summary>
        /// <param name="table">The table</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterTableDropHistory(ABaseDbTable table);

        /// <summary>
        /// Generates the alter table for altering the history
        /// </summary>
        /// <param name="sourceTable">The source table</param>
        /// <param name="targetTable">The target table</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterHistory(ABaseDbTable sourceTable, ABaseDbTable targetTable);

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
        /// Generates the alter table for altering the index from the table
        /// </summary>
        /// <param name="sourceIndex">The source index to add</param>
        /// <param name="targetIndex">The target index to drop</param>
        /// <returns>The alter table script</returns>
        protected abstract string ScriptAlterIndex(ABaseDbIndex sourceIndex, ABaseDbIndex targetIndex);

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
        /// <returns>The sorted columns</returns>
        protected IEnumerable<ABaseDbColumn> GetSortedTableColumns(ABaseDbTable table)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            // Order table columns alphabetically or by ordinal position
            var columns = this.Options.Scripting.OrderColumnAlphabetically ? table.Columns.OrderBy(x => x.Name).ToList() : this.OrderColumnsByOrdinalPosition(table).ToList();

            ABaseDbTable referenceTable = null;
            if (table.Database.Direction != Core.Enums.CompareDirection.Source)
            {
                referenceTable = table.MappedDbObject as ABaseDbTable;
            }

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

        /// <summary>
        /// Generate the script of the objects mapped
        /// </summary>
        /// <param name="scriptableObjects">The scriptable objects</param>
        /// <param name="scriptFunc">The script function</param>
        /// <returns>The script</returns>
        private static string GenerateObjectMapScript(IEnumerable<ObjectMap> scriptableObjects, Func<ABaseDbObject, bool, string> scriptFunc)
        {
            var sb = new StringBuilder();
            var additionalEmptyLine = false;
            foreach (var objectMap in scriptableObjects)
            {
                if (!objectMap.DbObjects.Any())
                {
                    continue;
                }

                if (additionalEmptyLine)
                {
                    sb.AppendLineIfNotEmpty();
                }

                if (!string.IsNullOrWhiteSpace(objectMap.ObjectTitle))
                {
                    sb.AppendLine(AScriptHelper.ScriptComment(objectMap.ObjectTitle));
                }

                foreach (var item in objectMap.DbObjects)
                {
                    sb.Append(scriptFunc(item, false));
                }

                additionalEmptyLine = true;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate the alter script of the table and the child objects
        /// </summary>
        /// <param name="t">The table</param>
        /// <returns>The script</returns>
        private string ScriptAlterTableAndChildDbObjects(ABaseDbTable t)
        {
            var targetTable = t.MappedDbObject as ABaseDbTable;
            var scriptableObjects = new List<ObjectMap>
            {
                // First drop what is not present anymore
                new ObjectMap { ObjectTitle = Localization.LabelPrimaryKeys, DbObjects = targetTable.PrimaryKeys.Where(x => x.MappedDbObject == null).OrderBy(x => x.Schema).ThenBy(x => x.Name) },
                new ObjectMap { ObjectTitle = Localization.LabelForeignKeys, DbObjects = targetTable.ForeignKeys.Where(x => x.MappedDbObject == null).OrderBy(x => x.Schema).ThenBy(x => x.Name) },
                new ObjectMap { ObjectTitle = Localization.LabelConstraints, DbObjects = targetTable.Constraints.Where(x => x.MappedDbObject == null).OrderBy(x => x.Schema).ThenBy(x => x.Name) },
                new ObjectMap { ObjectTitle = Localization.LabelTriggers, DbObjects = targetTable.Triggers.Where(x => x.MappedDbObject == null).OrderBy(x => x.Schema).ThenBy(x => x.Name) },
                new ObjectMap { ObjectTitle = Localization.LabelIndexes, DbObjects = this.GetSortedIndexes(targetTable.Indexes).Where(x => x.MappedDbObject == null) },

                // Do the changes to the table itself
                new ObjectMap { DbObjects = new[] { t } },

                // Finally create/alter the rest
                new ObjectMap { ObjectTitle = Localization.LabelPrimaryKeys, DbObjects = t.PrimaryKeys.Where(x => x.CreateScript != x.MappedDbObject?.CreateScript).OrderBy(x => x.Schema).ThenBy(x => x.Name) },
                new ObjectMap { ObjectTitle = Localization.LabelForeignKeys, DbObjects = t.ForeignKeys.Where(x => x.CreateScript != x.MappedDbObject?.CreateScript).OrderBy(x => x.Schema).ThenBy(x => x.Name) },
                new ObjectMap { ObjectTitle = Localization.LabelConstraints, DbObjects = t.Constraints.Where(x => x.CreateScript != x.MappedDbObject?.CreateScript).OrderBy(x => x.Schema).ThenBy(x => x.Name) },
                new ObjectMap { ObjectTitle = Localization.LabelTriggers, DbObjects = t.Triggers.Where(x => x.CreateScript != x.MappedDbObject?.CreateScript).OrderBy(x => x.Schema).ThenBy(x => x.Name) },
                new ObjectMap { ObjectTitle = Localization.LabelIndexes, DbObjects = this.GetSortedIndexes(t.Indexes).Where(x => x.CreateScript != x.MappedDbObject?.CreateScript) },
            };

            return GenerateObjectMapScript(scriptableObjects, this.GenerateAlterScript);
        }
    }
}
