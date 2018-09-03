using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
                sb.AppendLine((++i == ncol) ? string.Empty : ",");
            }

            sb.AppendLine(")");
            sb.AppendLine("GO");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptPrimaryKeysAlterTable(ABaseDbTable table)
        {
            var sb = new StringBuilder();
            foreach (var keys in table.PrimaryKeys.GroupBy(x => x.Name))
            {
                var key = (MicrosoftSqlIndex)keys.First();
                var columnList = keys.OrderBy(x => ((MicrosoftSqlIndex)x).OrdinalPosition).Select(x => $"[{((MicrosoftSqlIndex)x).ColumnName}]");

                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)}");
                sb.AppendLine($"ADD CONSTRAINT [{key.Name}] PRIMARY KEY {key.TypeDescription}({string.Join(",", columnList)})");
                sb.AppendLine("GO");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptForeignKeysAlterTable(ABaseDbTable table)
        {
            var sb = new StringBuilder();

            foreach (var aBaseDbConstraint in table.ForeignKeys.OrderBy(x => x.Name))
            {
                var key = (MicrosoftSqlForeignKey)aBaseDbConstraint;

                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)} WITH CHECK ADD CONSTRAINT [{key.Name}] FOREIGN KEY([{key.ColumnName}])");
                sb.AppendLine($"REFERENCES {this.ScriptHelper.ScriptObjectName(key.Database, key.ReferencedTableSchema, key.ReferencedTableName)}([{key.ReferencedTableColumn}])");
                sb.AppendLine($"ON DELETE {MicrosoftSqlScriptHelper.ScriptForeignKeyAction(key.DeleteReferentialAction)}");
                sb.AppendLine($"ON UPDATE {MicrosoftSqlScriptHelper.ScriptForeignKeyAction(key.UpdateReferentialAction)}");
                sb.AppendLine("GO");
                sb.AppendLine();
                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)} CHECK CONSTRAINT[{key.Name}]");
                sb.AppendLine("GO");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptConstraintsAlterTable(ABaseDbTable table)
        {
            var sb = new StringBuilder();
            foreach (var keys in table.Constraints.GroupBy(x => x.Name))
            {
                var key = keys.First();
                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)}");
                sb.AppendLine($"ADD CONSTRAINT [{key.Name}] CHECK {key.Definition}");
                sb.AppendLine("GO");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateIndexes(ABaseDbTable table)
        {
            var sb = new StringBuilder();

            // Reorder: Clustered indexes must be created before Nonclustered
            var orderedIndexes = new List<MicrosoftSqlIndex>();
            orderedIndexes.AddRange(table.Indexes.Cast<MicrosoftSqlIndex>().Where(x => x.Type == MicrosoftSqlIndex.IndexType.Clustered));
            orderedIndexes.AddRange(table.Indexes.Cast<MicrosoftSqlIndex>().Where(x => x.Type != MicrosoftSqlIndex.IndexType.Clustered));

            foreach (var indexes in orderedIndexes.GroupBy(x => x.Name))
            {
                var index = indexes.First();

                // If there is a column with descending order, specify the order on all columns
                var scriptOrder = indexes.Any(x => x.IsDescending);
                var columnList = indexes.OrderBy(x => x.OrdinalPosition).Select(x =>
                    scriptOrder ? $"[{x.ColumnName}] {(x.IsDescending ? "DESC" : "ASC")}" : $"[{x.ColumnName}]");

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
                        if (index.Type == MicrosoftSqlIndex.IndexType.Clustered)
                        {
                            sb.Append("CLUSTERED ");
                        }

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

                sb.AppendLine($"INDEX {index.Name} ON {this.ScriptHelper.ScriptObjectName(table)}({string.Join(",", columnList)})");
                sb.AppendLine("GO");
                sb.AppendLine();
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

            sb.AppendLine("GO");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateFunction(ABaseDbFunction sqlFunction, IEnumerable<ABaseDbObject> dataTypes)
        {
            var sb = new StringBuilder();
            sb.Append($"{sqlFunction.Definition}");
            if (!sqlFunction.Definition.EndsWith("\n", StringComparison.Ordinal))
            {
                sb.AppendLine();
            }

            sb.AppendLine("GO");
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

            sb.AppendLine("GO");
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

            sb.AppendLine("GO");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateSequence(ABaseDbSequence sequence)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE SEQUENCE {this.ScriptHelper.ScriptObjectName(sequence)}");
            sb.AppendLine($"    AS {sequence.DataType}");
            sb.AppendLine($"    START WITH {sequence.StartValue}");
            sb.AppendLine($"    INCREMENT BY {sequence.Increment}");

            // TODO: Handle min/max values correctly
            sb.AppendLine("    NO MINVALUE");
            sb.AppendLine("    NO MAXVALUE");
            sb.AppendLine("GO");
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string ScriptCreateType(ABaseDbDataType type)
        {
            var msType = (MicrosoftSqlDataType)type;

            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TYPE {this.ScriptHelper.ScriptObjectName(type)}");
            sb.Append($"    FROM {msType.SystemType.Name}");

            switch (msType.SystemType.Name)
            {
                // Cases with the configurable MaxLength parameter
                case "binary":
                case "char":
                case "nchar":
                case "nvarchar":
                case "varbinary":
                case "varchar":
                    sb.Append($"({(msType.MaxLength == -1 ? "max" : msType.MaxLength.ToString(CultureInfo.InvariantCulture))})");
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
            sb.AppendLine("GO");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbColumn> OrderColumnsByOrdinalPosition(ABaseDbTable table)
        {
            return table.Columns.OrderBy(x => ((MicrosoftSqlColumn)x).OrdinalPosition);
        }
    }
}