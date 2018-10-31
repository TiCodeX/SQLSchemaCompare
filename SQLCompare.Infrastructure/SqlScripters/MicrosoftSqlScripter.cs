using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.MicrosoftSql;
using SQLCompare.Core.Entities.Project;

namespace SQLCompare.Infrastructure.SqlScripters
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
        protected override string ScriptCreateTable(ABaseDbTable table, ABaseDbTable referenceTable = null)
        {
            var ncol = table.Columns.Count;

            var columns = this.GetSortedTableColumns(table, referenceTable);

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
        protected override string ScriptAlterTableAddPrimaryKeys(ABaseDbTable table)
        {
            var sb = new StringBuilder();

            // GroupBy because there might be multiple columns with the same key
            foreach (var keys in table.PrimaryKeys.OrderBy(x => x.Schema).ThenBy(x => x.Name).GroupBy(x => x.Name))
            {
                var key = (MicrosoftSqlIndex)keys.First();
                var columnList = keys.OrderBy(x => ((MicrosoftSqlIndex)x).OrdinalPosition).Select(x => $"{this.ScriptHelper.ScriptObjectName(((MicrosoftSqlIndex)x).ColumnName)}");

                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)} ADD CONSTRAINT {this.ScriptHelper.ScriptObjectName(key.Name)}");
                sb.AppendLine($"PRIMARY KEY {key.TypeDescription} ({string.Join(",", columnList)})");
                sb.Append(this.ScriptHelper.ScriptCommitTransaction());
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableDropPrimaryKeys(ABaseDbTable table)
        {
            var sb = new StringBuilder();

            // GroupBy because there might be multiple columns with the same key
            foreach (var key in table.PrimaryKeys.OrderBy(x => x.Schema).ThenBy(x => x.Name).GroupBy(x => x.Name).Select(x => x.First()))
            {
                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)} DROP CONSTRAINT {this.ScriptHelper.ScriptObjectName(key.Name)}");
                sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableAddForeignKeys(ABaseDbTable table)
        {
            var sb = new StringBuilder();

            // GroupBy because there might be multiple columns with the same key
            foreach (var keys in table.ForeignKeys.OrderBy(x => x.Schema).ThenBy(x => x.Name).GroupBy(x => x.Name))
            {
                var key = (MicrosoftSqlForeignKey)keys.First();
                var columnList = keys.OrderBy(x => ((MicrosoftSqlForeignKey)x).OrdinalPosition).Select(x => $"{this.ScriptHelper.ScriptObjectName(x.ColumnName)}");
                var referencedColumnList = keys.OrderBy(x => ((MicrosoftSqlForeignKey)x).ReferencedOrdinalPosition).Select(x => $"{this.ScriptHelper.ScriptObjectName(x.ReferencedColumnName)}");

                sb.Append($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)} WITH ");
                sb.Append(key.Disabled ? "NOCHECK" : "CHECK");
                sb.AppendLine($" ADD CONSTRAINT {this.ScriptHelper.ScriptObjectName(key.Name)}");

                sb.Append($"FOREIGN KEY ({string.Join(",", columnList)}) ");
                sb.AppendLine($"REFERENCES {this.ScriptHelper.ScriptObjectName(key.ReferencedTableSchema, key.ReferencedTableName)} ({string.Join(",", referencedColumnList)})");

                sb.AppendLine($"ON DELETE {MicrosoftSqlScriptHelper.ScriptForeignKeyAction(key.DeleteReferentialAction)}");
                sb.AppendLine($"ON UPDATE {MicrosoftSqlScriptHelper.ScriptForeignKeyAction(key.UpdateReferentialAction)}");
                sb.Append(this.ScriptHelper.ScriptCommitTransaction());

                sb.Append($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)} ");
                sb.Append(key.Disabled ? "NOCHECK" : "CHECK");
                sb.AppendLine($" CONSTRAINT {this.ScriptHelper.ScriptObjectName(key.Name)}");

                sb.Append(this.ScriptHelper.ScriptCommitTransaction());
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableDropForeignKeys(ABaseDbTable table)
        {
            var sb = new StringBuilder();

            // GroupBy because there might be multiple columns with the same key
            foreach (var key in table.ForeignKeys.OrderBy(x => x.Schema).ThenBy(x => x.Name).GroupBy(x => x.Name).Select(x => x.First()))
            {
                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)} DROP CONSTRAINT {this.ScriptHelper.ScriptObjectName(key.Name)}");
                sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableDropReferencingForeignKeys(ABaseDbTable table)
        {
            var sb = new StringBuilder();

            // GroupBy because there might be multiple columns with the same key
            foreach (var key in table.ReferencingForeignKeys.OrderBy(x => x.Schema).ThenBy(x => x.Name).GroupBy(x => x.Name).Select(x => x.First()))
            {
                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(key.TableSchema, key.TableName)} DROP CONSTRAINT {this.ScriptHelper.ScriptObjectName(key.Name)}");
                sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableAddConstraints(ABaseDbTable table)
        {
            var sb = new StringBuilder();

            // GroupBy because there might be multiple columns with the same key
            foreach (var constraint in table.Constraints.OrderBy(x => x.Schema).ThenBy(x => x.Name).GroupBy(x => x.Name).Select(x => x.First()))
            {
                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)} ADD CONSTRAINT {this.ScriptHelper.ScriptObjectName(constraint.Name)}");
                sb.AppendLine($"CHECK {constraint.Definition}");
                sb.Append(this.ScriptHelper.ScriptCommitTransaction());
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableDropConstraints(ABaseDbTable table)
        {
            var sb = new StringBuilder();

            // GroupBy because there might be multiple columns with the same key
            foreach (var constraint in table.Constraints.OrderBy(x => x.Schema).ThenBy(x => x.Name).GroupBy(x => x.Name).Select(x => x.First()))
            {
                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)} DROP CONSTRAINT {this.ScriptHelper.ScriptObjectName(constraint.Name)}");
                sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            }

            foreach (var column in table.Columns.Cast<MicrosoftSqlColumn>()
                .Where(x => !string.IsNullOrWhiteSpace(x.DefaultConstraintName))
                .OrderBy(x => x.DefaultConstraintName))
            {
                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)} DROP CONSTRAINT {this.ScriptHelper.ScriptObjectName(column.DefaultConstraintName)}");
                sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateIndexes(ABaseDbObject dbObject, List<ABaseDbIndex> indexes)
        {
            var sb = new StringBuilder();

            // Reorder: Clustered indexes must be created before Nonclustered
            var orderedIndexes = new List<MicrosoftSqlIndex>();
            orderedIndexes.AddRange(indexes.Cast<MicrosoftSqlIndex>().Where(x => x.Type == MicrosoftSqlIndex.IndexType.Clustered).OrderBy(x => x.Schema).ThenBy(x => x.Name));
            orderedIndexes.AddRange(indexes.Cast<MicrosoftSqlIndex>().Where(x => x.Type != MicrosoftSqlIndex.IndexType.Clustered).OrderBy(x => x.Schema).ThenBy(x => x.Name));

            // GroupBy because there might be multiple columns with the same index
            foreach (var indexGroup in orderedIndexes.GroupBy(x => x.Name))
            {
                var index = indexGroup.First();

                // If there is a column with descending order, specify the order on all columns
                var scriptOrder = indexGroup.Any(x => x.IsDescending);
                var columnList = indexGroup.OrderBy(x => x.OrdinalPosition).Select(x => scriptOrder ?
                        $"{this.ScriptHelper.ScriptObjectName(x.ColumnName)} {(x.IsDescending ? "DESC" : "ASC")}" :
                        $"{this.ScriptHelper.ScriptObjectName(x.ColumnName)}");

                sb.Append("CREATE ");
                switch (index.Type)
                {
                    case MicrosoftSqlIndex.IndexType.Clustered:
                    case MicrosoftSqlIndex.IndexType.Nonclustered:

                        if (index.IsUnique.HasValue && index.IsUnique == true)
                        {
                            sb.Append("UNIQUE ");
                        }

                        // If CLUSTERED is not specified, a NONCLUSTERED index is created
                        sb.Append(index.Type == MicrosoftSqlIndex.IndexType.Clustered ? "CLUSTERED " : "NONCLUSTERED ");

                        break;

                    case MicrosoftSqlIndex.IndexType.XML:
                        sb.Append("XML ");
                        break;

                    case MicrosoftSqlIndex.IndexType.Spatial:
                        sb.Append("SPATIAL ");
                        break;

                    default:
                        throw new NotSupportedException($"Index of type '{index.Type.ToString()}' is not supported");
                }

                sb.AppendLine($"INDEX {this.ScriptHelper.ScriptObjectName(index.Name)} ON {this.ScriptHelper.ScriptObjectName(dbObject)}({string.Join(",", columnList)})");
                if (!string.IsNullOrWhiteSpace(index.FilterDefinition))
                {
                    sb.AppendLine($"{this.Indent}WHERE {index.FilterDefinition}");
                }

                sb.Append(this.ScriptHelper.ScriptCommitTransaction());
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropIndexes(ABaseDbObject dbObject, List<ABaseDbIndex> indexes)
        {
            var sb = new StringBuilder();

            // GroupBy because there might be multiple columns with the same index
            foreach (var index in indexes.OrderBy(x => x.Schema).ThenBy(x => x.Name).GroupBy(x => x.Name).Select(x => x.First()))
            {
                sb.AppendLine($"DROP INDEX {this.ScriptHelper.ScriptObjectName(index.Name)} ON {this.ScriptHelper.ScriptObjectName(dbObject)}");
                sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            }

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
            sb.AppendLine($"DROP VIEW {this.ScriptHelper.ScriptObjectName(view)};");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterView(ABaseDbView sourceView, ABaseDbView targetView)
        {
            const string pattern = @"^\s*CREATE\s+VIEW\s+";
            const string replacement = @"ALTER VIEW ";

            var sb = new StringBuilder();
            sb.Append(Regex.Replace(sourceView.ViewDefinition, pattern, replacement, RegexOptions.IgnoreCase | RegexOptions.Singleline));
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
        protected override string ScriptDropFunction(ABaseDbFunction sqlFunction)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP FUNCTION {this.ScriptHelper.ScriptObjectName(sqlFunction)}");
            sb.Append(this.ScriptHelper.ScriptCommitTransaction());
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterFunction(ABaseDbFunction sourceFunction, ABaseDbFunction targetFunction, IReadOnlyList<ABaseDbDataType> dataTypes)
        {
            const string pattern = @"^\s*CREATE\s+FUNCTION\s+";
            const string replacement = @"ALTER FUNCTION ";

            var sb = new StringBuilder();
            sb.Append(Regex.Replace(sourceFunction.Definition, pattern, replacement, RegexOptions.IgnoreCase | RegexOptions.Singleline));
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
            const string pattern = @"^\s*CREATE\s+(PROC|PROCEDURE)\s+";
            const string replacement = @"ALTER PROCEDURE ";

            var sb = new StringBuilder();
            sb.Append(Regex.Replace(sourceStoredProcedure.Definition, pattern, replacement, RegexOptions.IgnoreCase | RegexOptions.Singleline));
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
            const string pattern = @"^\s*CREATE\s+TRIGGER\s+";
            const string replacement = @"ALTER TRIGGER ";

            var sb = new StringBuilder();
            sb.Append(Regex.Replace(sourceTrigger.Definition, pattern, replacement, RegexOptions.IgnoreCase | RegexOptions.Singleline));
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
            sb.AppendLine(this.ScriptDropType(targetType));
            sb.AppendLine(this.ScriptCreateType(sourceType, dataTypes));
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override IEnumerable<ABaseDbTable> GetSortedTables(List<ABaseDbTable> tables, bool dropOrder)
        {
            // Parameter dropOrder ignores because we want to drop the tables alphabetically
            return tables.OrderBy(x => x.Schema).ThenBy(x => x.Name);
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbColumn> OrderColumnsByOrdinalPosition(ABaseDbTable table)
        {
            return table.Columns.OrderBy(x => ((MicrosoftSqlColumn)x).OrdinalPosition);
        }
    }
}