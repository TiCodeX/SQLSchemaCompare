using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database.MicrosoftSql;
using TiCodeX.SQLSchemaCompare.Core.Entities.Project;

namespace TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Sql scripter class specific for MicrosoftSql database
    /// </summary>
    internal class MicrosoftSqlScripter : ADatabaseScripter<MicrosoftSqlScriptHelper>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlScripter"/> class.
        /// </summary>
        /// <param name="logger">The injected logger instance</param>
        /// <param name="options">The project options</param>
        public MicrosoftSqlScripter(ILogger logger, ProjectOptions options)
            : base(logger, options, new MicrosoftSqlScriptHelper(options))
        {
        }

        /// <inheritdoc/>
        protected override string ScriptCreateSchema(ABaseDbSchema schema)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE SCHEMA {this.ScriptHelper.ScriptObjectName(schema.Name)} AUTHORIZATION {this.ScriptHelper.ScriptObjectName(schema.Owner)}");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropSchema(ABaseDbSchema schema)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP SCHEMA {this.ScriptHelper.ScriptObjectName(schema.Name)}");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterSchema(ABaseDbSchema schema)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ALTER AUTHORIZATION ON SCHEMA::{this.ScriptHelper.ScriptObjectName(schema.Name)} TO {this.ScriptHelper.ScriptObjectName(schema.Owner)}");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateTable(ABaseDbTable table)
        {
            var ncol = table.Columns.Count;

            var columns = this.GetSortedTableColumns(table);

            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE {this.ScriptHelper.ScriptObjectName(table)}(");

            var i = 0;
            foreach (var col in columns)
            {
                sb.Append($"{this.Indent}{this.ScriptHelper.ScriptColumn(col)}");
                sb.AppendLine(++i == ncol ? string.Empty : ",");
            }

            sb.AppendLine(")");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropTable(ABaseDbTable table)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP TABLE {this.ScriptHelper.ScriptObjectName(table)}");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTable(ABaseDbTable t)
        {
            var sb = new StringBuilder();

            var targetTable = t.MappedDbObject as ABaseDbTable;
            if (targetTable == null)
            {
                throw new ArgumentException($"{nameof(t.MappedDbObject)} is null");
            }

            // Remove columns
            foreach (var c in targetTable.Columns.Where(x => x.MappedDbObject == null))
            {
                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(t)} DROP COLUMN {this.ScriptHelper.ScriptObjectName(c.Name)}");
                sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            }

            // Alter columns
            foreach (var c in t.Columns.Where(x => x.MappedDbObject != null && x.CreateScript != x.MappedDbObject.CreateScript))
            {
                var targetColumn = (MicrosoftSqlColumn)c.MappedDbObject;
                var defaultIsMissingInSource = string.IsNullOrWhiteSpace(c.ColumnDefault) &&
                                             !string.IsNullOrWhiteSpace(targetColumn.ColumnDefault) &&
                                             !string.IsNullOrWhiteSpace(targetColumn.DefaultConstraintName);
                var defaultIsMissingInTarget = !string.IsNullOrWhiteSpace(c.ColumnDefault) &&
                                             string.IsNullOrWhiteSpace(targetColumn.ColumnDefault);
                var defaultIsDifferent = !string.IsNullOrWhiteSpace(c.ColumnDefault) && !string.IsNullOrWhiteSpace(targetColumn.ColumnDefault) && c.ColumnDefault != targetColumn.ColumnDefault;

                if (defaultIsMissingInSource || defaultIsDifferent)
                {
                    var constraint = new ABaseDbConstraint
                    {
                        TableSchema = targetTable.Schema,
                        TableName = targetTable.Name,
                        Name = ((MicrosoftSqlColumn)c.MappedDbObject).DefaultConstraintName,
                    };

                    sb.Append(this.ScriptAlterTableDropConstraint(constraint));
                }

                // Compare again the columns without the default constraint
                var columnScriptSource = this.ScriptHelper.ScriptColumn(c, false);
                var columnScriptTarget = this.ScriptHelper.ScriptColumn(targetColumn, false);
                if (columnScriptSource != columnScriptTarget)
                {
                    sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(t)} ALTER COLUMN {columnScriptSource}");
                    sb.Append(this.ScriptHelper.ScriptCommitTransaction());
                }

                if (defaultIsMissingInTarget || defaultIsDifferent)
                {
                    sb.Append($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(t)} ADD");
                    if (!string.IsNullOrWhiteSpace(c.DefaultConstraintName))
                    {
                        sb.Append($" CONSTRAINT {this.ScriptHelper.ScriptObjectName(c.DefaultConstraintName)}");
                    }

                    sb.AppendLine($" DEFAULT {c.ColumnDefault} FOR {this.ScriptHelper.ScriptObjectName(c.Name)}");
                    sb.Append(this.ScriptHelper.ScriptCommitTransaction());
                }
            }

            // Add columns
            foreach (var c in t.Columns.Where(x => x.MappedDbObject == null))
            {
                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(t)} ADD {this.ScriptHelper.ScriptColumn(c)}");
                sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableAddPrimaryKey(ABaseDbPrimaryKey primaryKey)
        {
            var key = (MicrosoftSqlPrimaryKey)primaryKey;

            var sb = new StringBuilder();

            var columnList = key.ColumnNames.Select(x => $"{this.ScriptHelper.ScriptObjectName(x)}");

            sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(key.TableSchema, key.TableName)} ADD CONSTRAINT {this.ScriptHelper.ScriptObjectName(key.Name)}");
            sb.AppendLine($"PRIMARY KEY {key.TypeDescription} ({string.Join(",", columnList)})");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableDropPrimaryKey(ABaseDbPrimaryKey primaryKey)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(primaryKey.TableSchema, primaryKey.TableName)} DROP CONSTRAINT {this.ScriptHelper.ScriptObjectName(primaryKey.Name)}");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string ScriptAlterPrimaryKey(ABaseDbPrimaryKey sourcePrimaryKey, ABaseDbPrimaryKey targetPrimaryKey)
        {
            var sb = new StringBuilder();
            sb.Append(this.ScriptAlterTableDropPrimaryKey(targetPrimaryKey));
            sb.Append(this.ScriptAlterTableAddPrimaryKey(sourcePrimaryKey));
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableAddForeignKey(ABaseDbForeignKey foreignKey)
        {
            var key = (MicrosoftSqlForeignKey)foreignKey;

            var sb = new StringBuilder();

            var columnList = key.ColumnNames.Select(x => $"{this.ScriptHelper.ScriptObjectName(x)}");
            var referencedColumnList = key.ReferencedColumnNames.Select(x => $"{this.ScriptHelper.ScriptObjectName(x)}");

            sb.Append($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(key.TableSchema, key.TableName)} WITH ");
            sb.Append(key.Disabled ? "NOCHECK" : "CHECK");
            sb.AppendLine($" ADD CONSTRAINT {this.ScriptHelper.ScriptObjectName(key.Name)}");

            sb.Append($"FOREIGN KEY ({string.Join(",", columnList)}) ");
            sb.AppendLine($"REFERENCES {this.ScriptHelper.ScriptObjectName(key.ReferencedTableSchema, key.ReferencedTableName)} ({string.Join(",", referencedColumnList)})");

            sb.AppendLine($"ON DELETE {MicrosoftSqlScriptHelper.ScriptForeignKeyAction(key.DeleteReferentialAction)}");
            sb.AppendLine($"ON UPDATE {MicrosoftSqlScriptHelper.ScriptForeignKeyAction(key.UpdateReferentialAction)}");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());

            sb.Append($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(key.TableSchema, key.TableName)} ");
            sb.Append(key.Disabled ? "NOCHECK" : "CHECK");
            sb.AppendLine($" CONSTRAINT {this.ScriptHelper.ScriptObjectName(key.Name)}");

            sb.Append(this.ScriptHelper.ScriptCommitTransaction());

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableDropForeignKey(ABaseDbForeignKey foreignKey)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(foreignKey.TableSchema, foreignKey.TableName)} DROP CONSTRAINT {this.ScriptHelper.ScriptObjectName(foreignKey.Name)}");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string ScriptAlterForeignKey(ABaseDbForeignKey sourceForeignKey, ABaseDbForeignKey targetForeignKey)
        {
            var sb = new StringBuilder();
            sb.Append(this.ScriptAlterTableDropForeignKey(targetForeignKey));
            sb.Append(this.ScriptAlterTableAddForeignKey(sourceForeignKey));
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableAddConstraint(ABaseDbConstraint constraint)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(constraint.TableSchema, constraint.TableName)} ADD CONSTRAINT {this.ScriptHelper.ScriptObjectName(constraint.Name)}");
            sb.AppendLine($"CHECK {constraint.Definition}");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableDropConstraint(ABaseDbConstraint constraint)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(constraint.TableSchema, constraint.TableName)} DROP CONSTRAINT {this.ScriptHelper.ScriptObjectName(constraint.Name)}");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string ScriptAlterConstraint(ABaseDbConstraint sourceConstraint, ABaseDbConstraint targetConstraint)
        {
            var sb = new StringBuilder();
            sb.Append(this.ScriptAlterTableDropConstraint(targetConstraint));
            sb.Append(this.ScriptAlterTableAddConstraint(sourceConstraint));
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateIndex(ABaseDbIndex index)
        {
            var indexMicrosoft = (MicrosoftSqlIndex)index;

            var sb = new StringBuilder();

            // If there is a column with descending order, specify the order on all columns
            var scriptOrder = index.ColumnDescending.Any(x => x);
            var columnList = index.ColumnNames.Select((x, i) => scriptOrder ?
                $"{this.ScriptHelper.ScriptObjectName(x)} {(index.ColumnDescending[i] ? "DESC" : "ASC")}" :
                $"{this.ScriptHelper.ScriptObjectName(x)}");

            sb.Append("CREATE ");
            switch (indexMicrosoft.Type)
            {
                case MicrosoftSqlIndex.IndexType.Clustered:
                case MicrosoftSqlIndex.IndexType.Nonclustered:

                    if (indexMicrosoft.IsUnique.HasValue && indexMicrosoft.IsUnique == true)
                    {
                        sb.Append("UNIQUE ");
                    }

                    // If CLUSTERED is not specified, a NONCLUSTERED index is created
                    sb.Append(indexMicrosoft.Type == MicrosoftSqlIndex.IndexType.Clustered ? "CLUSTERED " : "NONCLUSTERED ");

                    break;

                case MicrosoftSqlIndex.IndexType.XML:
                    sb.Append("XML ");
                    break;

                case MicrosoftSqlIndex.IndexType.Spatial:
                    sb.Append("SPATIAL ");
                    break;

                default:
                    throw new NotSupportedException($"Index of type '{indexMicrosoft.Type}' is not supported");
            }

            sb.AppendLine($"INDEX {this.ScriptHelper.ScriptObjectName(index.Name)} ON {this.ScriptHelper.ScriptObjectName(index.TableSchema, index.TableName)}({string.Join(",", columnList)})");
            if (!string.IsNullOrWhiteSpace(indexMicrosoft.FilterDefinition))
            {
                sb.AppendLine($"{this.Indent}WHERE {indexMicrosoft.FilterDefinition}");
            }

            sb.Append(this.ScriptHelper.ScriptCommitTransaction());

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropIndex(ABaseDbIndex index)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP INDEX {this.ScriptHelper.ScriptObjectName(index.Name)} ON {this.ScriptHelper.ScriptObjectName(index.TableSchema, index.TableName)}");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string ScriptAlterIndex(ABaseDbIndex sourceIndex, ABaseDbIndex targetIndex)
        {
            var sb = new StringBuilder();
            sb.Append(this.ScriptDropIndex(targetIndex));
            sb.Append(this.ScriptCreateIndex(sourceIndex));
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateView(ABaseDbView view)
        {
            var sb = new StringBuilder();
            sb.Append($"{view.ViewDefinition}");
            if (!view.ViewDefinition.EndsWith("\n", StringComparison.Ordinal))
            {
                sb.AppendLine();
            }

            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropView(ABaseDbView view)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP VIEW {this.ScriptHelper.ScriptObjectName(view)}");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterView(ABaseDbView sourceView, ABaseDbView targetView)
        {
            const string pattern = @"^(\s*)CREATE(\s+)(VIEW)(\s+)";
            const string replacement = @"$1ALTER$2$3$4";

            var sb = new StringBuilder();
            sb.Append(Regex.Replace(sourceView.ViewDefinition, pattern, replacement, RegexOptions.IgnoreCase | RegexOptions.Multiline));
            if (!sourceView.ViewDefinition.EndsWith("\n", StringComparison.Ordinal))
            {
                sb.AppendLine();
            }

            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateFunction(ABaseDbFunction sqlFunction, IReadOnlyList<ABaseDbDataType> dataTypes)
        {
            var sb = new StringBuilder();
            sb.Append($"{sqlFunction.Definition}");
            if (!sqlFunction.Definition.EndsWith("\n", StringComparison.Ordinal))
            {
                sb.AppendLine();
            }

            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropFunction(ABaseDbFunction sqlFunction, IReadOnlyList<ABaseDbDataType> dataTypes)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP FUNCTION {this.ScriptHelper.ScriptObjectName(sqlFunction)}");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterFunction(ABaseDbFunction sourceFunction, ABaseDbFunction targetFunction, IReadOnlyList<ABaseDbDataType> dataTypes)
        {
            const string pattern = @"^(\s*)CREATE(\s+)(FUNCTION)(\s+)";
            const string replacement = @"$1ALTER$2$3$4";

            var sb = new StringBuilder();
            sb.Append(Regex.Replace(sourceFunction.Definition, pattern, replacement, RegexOptions.IgnoreCase | RegexOptions.Multiline));
            if (!sourceFunction.Definition.EndsWith("\n", StringComparison.Ordinal))
            {
                sb.AppendLine();
            }

            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateStoredProcedure(ABaseDbStoredProcedure storedProcedure)
        {
            var sb = new StringBuilder();
            sb.Append($"{storedProcedure.Definition}");
            if (!storedProcedure.Definition.EndsWith("\n", StringComparison.Ordinal))
            {
                sb.AppendLine();
            }

            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropStoredProcedure(ABaseDbStoredProcedure storedProcedure)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP PROCEDURE {this.ScriptHelper.ScriptObjectName(storedProcedure)}");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterStoredProcedure(ABaseDbStoredProcedure sourceStoredProcedure, ABaseDbStoredProcedure targetStoredProcedure)
        {
            const string pattern = @"^(\s*)CREATE(\s+)(PROC|PROCEDURE)(\s+)";
            const string replacement = @"$1ALTER$2$3$4";

            var sb = new StringBuilder();
            sb.Append(Regex.Replace(sourceStoredProcedure.Definition, pattern, replacement, RegexOptions.IgnoreCase | RegexOptions.Multiline));
            if (!sourceStoredProcedure.Definition.EndsWith("\n", StringComparison.Ordinal))
            {
                sb.AppendLine();
            }

            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateTrigger(ABaseDbTrigger trigger)
        {
            var sb = new StringBuilder();
            sb.AppendLine(this.ScriptHelper.ScriptCommitTransaction());
            sb.Append($"{trigger.Definition}");
            if (!trigger.Definition.EndsWith("\n", StringComparison.Ordinal))
            {
                sb.AppendLine();
            }

            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropTrigger(ABaseDbTrigger trigger)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP TRIGGER {this.ScriptHelper.ScriptObjectName(trigger)}");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTrigger(ABaseDbTrigger sourceTrigger, ABaseDbTrigger targetTrigger)
        {
            const string pattern = @"^(\s*)CREATE(\s+)(TRIGGER)(\s+)";
            const string replacement = @"$1ALTER$2$3$4";

            var sb = new StringBuilder();
            sb.AppendLine(this.ScriptHelper.ScriptCommitTransaction());
            sb.Append(Regex.Replace(sourceTrigger.Definition, pattern, replacement, RegexOptions.IgnoreCase | RegexOptions.Multiline));
            if (!sourceTrigger.Definition.EndsWith("\n", StringComparison.Ordinal))
            {
                sb.AppendLine();
            }

            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateSequence(ABaseDbSequence sequence)
        {
            var sequenceMicrosoft = sequence as MicrosoftSqlSequence;
            if (sequenceMicrosoft == null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            var sb = new StringBuilder();
            sb.AppendLine($"CREATE SEQUENCE {this.ScriptHelper.ScriptObjectName(sequence)}");
            sb.AppendLine($"{this.Indent}AS {sequence.DataType}");
            sb.AppendLine($"{this.Indent}START WITH {sequence.StartValue}");
            sb.AppendLine($"{this.Indent}INCREMENT BY {sequence.Increment}");
            sb.AppendLine($"{this.Indent}MINVALUE {sequence.MinValue}");
            sb.AppendLine($"{this.Indent}MAXVALUE {sequence.MaxValue}");
            sb.AppendLine(sequence.IsCycling ?
                $"{this.Indent}CYCLE" :
                $"{this.Indent}NO CYCLE");
            sb.AppendLine(sequenceMicrosoft.IsCached ?
                $"{this.Indent}CACHE" :
                $"{this.Indent}NO CACHE");

            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropSequence(ABaseDbSequence sequence)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP SEQUENCE {this.ScriptHelper.ScriptObjectName(sequence)}");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterSequence(ABaseDbSequence sourceSequence, ABaseDbSequence targetSequence)
        {
            var sourceSequenceMicrosoft = sourceSequence as MicrosoftSqlSequence;
            if (sourceSequenceMicrosoft == null)
            {
                throw new ArgumentNullException(nameof(sourceSequence));
            }

            var targetSequenceMicrosoft = targetSequence as MicrosoftSqlSequence;
            if (targetSequenceMicrosoft == null)
            {
                throw new ArgumentNullException(nameof(targetSequence));
            }

            var sb = new StringBuilder();
            sb.AppendLine($"ALTER SEQUENCE {this.ScriptHelper.ScriptObjectName(targetSequence)}");
            if (sourceSequence.StartValue != targetSequence.StartValue)
            {
                sb.AppendLine($"{this.Indent}RESTART WITH {sourceSequence.StartValue}");
            }

            if (sourceSequence.Increment != targetSequence.Increment)
            {
                sb.AppendLine($"{this.Indent}INCREMENT BY {sourceSequence.Increment}");
            }

            if (sourceSequence.MinValue != targetSequence.MinValue)
            {
                sb.AppendLine($"{this.Indent}MINVALUE {sourceSequence.MinValue}");
            }

            if (sourceSequence.MaxValue != targetSequence.MaxValue)
            {
                sb.AppendLine($"{this.Indent}MAXVALUE {sourceSequence.MaxValue}");
            }

            if (sourceSequence.IsCycling != targetSequence.IsCycling)
            {
                sb.AppendLine(sourceSequence.IsCycling ?
                    $"{this.Indent}CYCLE" :
                    $"{this.Indent}NO CYCLE");
            }

            if (sourceSequenceMicrosoft.IsCached != targetSequenceMicrosoft.IsCached)
            {
                sb.AppendLine(sourceSequenceMicrosoft.IsCached ?
                    $"{this.Indent}CACHE" :
                    $"{this.Indent}NO CACHE");
            }

            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string ScriptCreateType(ABaseDbDataType type, IReadOnlyList<ABaseDbDataType> dataTypes)
        {
            var msType = (MicrosoftSqlDataType)type;

            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TYPE {this.ScriptHelper.ScriptObjectName(type)}");
            sb.Append($"{this.Indent}FROM {msType.SystemType.Name}");

            switch (msType.SystemType.Name)
            {
                // Cases with the configurable MaxLength parameter
                case "binary":
                case "char":
                case "nchar":
                case "nvarchar":
                case "varbinary":
                case "varchar":
                    if (msType.MaxLength == -1)
                    {
                        sb.Append("(max)");
                    }
                    else
                    {
                        var maxLength = msType.MaxLength;
                        if (msType.SystemType.Name == "nchar" ||
                            msType.SystemType.Name == "nvarchar")
                        {
                            maxLength /= 2;
                        }

                        sb.Append($"({maxLength.ToString(CultureInfo.InvariantCulture)})");
                    }

                    break;

                // Cases with the configurable Scale parameter only
                case "datetime2":
                case "datetimeoffset":
                case "time":
                    sb.Append($"({msType.Scale.ToString(CultureInfo.InvariantCulture)})");
                    break;

                // Cases with configurable Scale and Precision parameters
                case "decimal":
                case "numeric":
                    sb.Append($"({msType.Precision.ToString(CultureInfo.InvariantCulture)},{msType.Scale.ToString(CultureInfo.InvariantCulture)})");
                    break;
            }

            sb.AppendLine(msType.IsNullable ? " NULL" : " NOT NULL");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string ScriptDropType(ABaseDbDataType type)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP TYPE {this.ScriptHelper.ScriptObjectName(type)}");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string ScriptAlterType(ABaseDbDataType sourceType, ABaseDbDataType targetType, IReadOnlyList<ABaseDbDataType> dataTypes)
        {
            var sb = new StringBuilder();
            sb.Append(this.ScriptDropType(targetType));
            sb.Append(this.ScriptCreateType(sourceType, dataTypes));
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override IEnumerable<ABaseDbTable> GetSortedTables(List<ABaseDbTable> tables, bool dropOrder)
        {
            // Parameter dropOrder ignored because we want to drop the tables alphabetically
            return tables.OrderBy(x => x.Schema).ThenBy(x => x.Name);
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbColumn> OrderColumnsByOrdinalPosition(ABaseDbTable table)
        {
            return table.Columns.OrderBy(x => ((MicrosoftSqlColumn)x).OrdinalPosition);
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbFunction> GetSortedFunctions(List<ABaseDbFunction> functions, bool dropOrder)
        {
            // Parameter dropOrder ignored because we want to drop the functions alphabetically
            return functions.OrderBy(x => x.Schema).ThenBy(x => x.Name);
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbIndex> GetSortedIndexes(List<ABaseDbIndex> indexes)
        {
            // Clustered indexes must be created before Nonclustered
            var orderedIndexes = new List<MicrosoftSqlIndex>();
            orderedIndexes.AddRange(indexes.Cast<MicrosoftSqlIndex>().Where(x => x.Type == MicrosoftSqlIndex.IndexType.Clustered).OrderBy(x => x.Schema).ThenBy(x => x.Name));
            orderedIndexes.AddRange(indexes.Cast<MicrosoftSqlIndex>().Where(x => x.Type != MicrosoftSqlIndex.IndexType.Clustered).OrderBy(x => x.Schema).ThenBy(x => x.Name));
            return orderedIndexes;
        }
    }
}
