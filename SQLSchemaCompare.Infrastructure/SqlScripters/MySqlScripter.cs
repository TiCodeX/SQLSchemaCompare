﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database.MySql;
using TiCodeX.SQLSchemaCompare.Core.Entities.Project;

namespace TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Sql scripter class specific for MySql database
    /// </summary>
    internal class MySqlScripter : ADatabaseScripter<MySqlScriptHelper>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlScripter"/> class.
        /// </summary>
        /// <param name="logger">The injected logger instance</param>
        /// <param name="options">The project options</param>
        public MySqlScripter(ILogger logger, ProjectOptions options)
            : base(logger, options, new MySqlScriptHelper(options))
        {
        }

        /// <summary>
        /// Gets the functions/stored procedures/triggers delimiter
        /// </summary>
        private static string Delimiter => "$$$$";

        /// <inheritdoc/>
        protected override string ScriptCreateTable(ABaseDbTable table)
        {
            var mySqlTable = (MySqlTable)table;
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

            sb.Append(")");
            if (!string.IsNullOrWhiteSpace(mySqlTable.Engine) && !mySqlTable.Engine.Equals("InnoDB", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append($" ENGINE={mySqlTable.Engine}");
            }

            if (!string.IsNullOrWhiteSpace(mySqlTable.TableCharacterSet))
            {
                sb.Append($" DEFAULT CHARSET={mySqlTable.TableCharacterSet}");
            }

            sb.AppendLine(";");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropTable(ABaseDbTable table)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP TABLE {this.ScriptHelper.ScriptObjectName(table)};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableAddPrimaryKeys(ABaseDbTable table)
        {
            var sb = new StringBuilder();

            foreach (var key in table.PrimaryKeys.OrderBy(x => x.Schema).ThenBy(x => x.Name).Cast<MySqlIndex>())
            {
                var columnList = key.ColumnNames.Select(x => $"{this.ScriptHelper.ScriptObjectName(x)}");

                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)}");
                sb.AppendLine($"ADD CONSTRAINT {this.ScriptHelper.ScriptObjectName(key.Name)} PRIMARY KEY ({string.Join(",", columnList)});");
                foreach (var columnName in key.ColumnNames)
                {
                    if (table.Columns.FirstOrDefault(x => x.Name == columnName) is MySqlColumn col &&
                        col.Extra.ToUpperInvariant() == "AUTO_INCREMENT")
                    {
                        sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)}");
                        sb.AppendLine($"MODIFY {this.ScriptHelper.ScriptColumn(col)} AUTO_INCREMENT;");
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableDropPrimaryKeys(ABaseDbTable table)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)} DROP PRIMARY KEY;");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableAddForeignKey(ABaseDbForeignKey foreignKey)
        {
            var key = (MySqlForeignKey)foreignKey;

            var sb = new StringBuilder();

            var columnList = key.ColumnNames.Select(x => $"{this.ScriptHelper.ScriptObjectName(x)}");
            var referencedColumnList = key.ReferencedColumnNames.Select(x => $"{this.ScriptHelper.ScriptObjectName(x)}");

            sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(key.TableSchema, key.TableName)}");
            sb.AppendLine($"ADD CONSTRAINT {this.ScriptHelper.ScriptObjectName(key.Name)} FOREIGN KEY ({string.Join(",", columnList)})");
            sb.AppendLine($"REFERENCES {this.ScriptHelper.ScriptObjectName(key.ReferencedTableSchema, key.ReferencedTableName)} ({string.Join(",", referencedColumnList)})");
            sb.AppendLine($"ON DELETE {key.DeleteRule}");
            sb.AppendLine($"ON UPDATE {key.UpdateRule};");
            sb.AppendLine();

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableDropForeignKey(ABaseDbForeignKey foreignKey)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(foreignKey.TableSchema, foreignKey.TableName)} DROP FOREIGN KEY {this.ScriptHelper.ScriptObjectName(foreignKey.Name)};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableDropReferencingForeignKeys(ABaseDbTable table)
        {
            var sb = new StringBuilder();

            foreach (var key in table.ReferencingForeignKeys.OrderBy(x => x.Schema).ThenBy(x => x.Name))
            {
                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(key.TableName)} DROP FOREIGN KEY {this.ScriptHelper.ScriptObjectName(key.Name)};");
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableAddConstraint(ABaseDbConstraint constraint)
        {
            throw new NotSupportedException("MySQL doesn't have CHECK constraints");
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTableDropConstraint(ABaseDbConstraint constraint)
        {
            throw new NotSupportedException("MySQL doesn't have CHECK constraints");
        }

        /// <inheritdoc/>
        protected override string ScriptCreateIndex(ABaseDbIndex index)
        {
            var indexMySql = (MySqlIndex)index;

            var sb = new StringBuilder();

            // If there is a column with descending order, specify the order on all columns
            var scriptOrder = index.ColumnDescending.Any(x => x);
            var columnList = index.ColumnNames.Select((x, i) => (scriptOrder ?
                $"{this.ScriptHelper.ScriptObjectName(x)} {(index.ColumnDescending[i] ? "DESC" : "ASC")}" :
                $"{this.ScriptHelper.ScriptObjectName(x)}"));

            sb.Append("CREATE ");
            if (indexMySql.IndexType == "FULLTEXT")
            {
                sb.Append("FULLTEXT ");
            }
            else if (indexMySql.IndexType == "SPATIAL")
            {
                sb.Append("SPATIAL ");
            }
            else if (index.ConstraintType == "UNIQUE")
            {
                sb.Append("UNIQUE ");
            }

            sb.Append($"INDEX {index.Name} ");

            // If not specified it will use the BTREE
            if (indexMySql.IndexType == "HASH")
            {
                sb.Append("USING HASH ");
            }

            sb.AppendLine($"ON {this.ScriptHelper.ScriptObjectName(index.TableSchema, index.TableName)}({string.Join(",", columnList)});");
            sb.AppendLine();

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropIndex(ABaseDbIndex index)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP INDEX {index.Name} ON {this.ScriptHelper.ScriptObjectName(index.TableSchema, index.TableName)};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateView(ABaseDbView view)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{view.ViewDefinition.TrimEnd('\r', '\n', ' ', ';')};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropView(ABaseDbView view)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP VIEW {this.ScriptHelper.ScriptObjectName(view)};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterView(ABaseDbView sourceView, ABaseDbView targetView)
        {
            const string pattern = @"^\s*CREATE.*VIEW\s+(`[^`]*`|`[^`]*`\s+\([^\)]*\))\s+AS";
            const string replacement = @"ALTER VIEW $1 AS";

            var alterViewDefinition = Regex.Replace(sourceView.ViewDefinition, pattern, replacement, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            var sb = new StringBuilder();
            sb.AppendLine($"{alterViewDefinition.TrimEnd('\r', '\n', ' ', ';')};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateFunction(ABaseDbFunction sqlFunction, IReadOnlyList<ABaseDbDataType> dataTypes)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DELIMITER {Delimiter}");
            sb.Append($"{sqlFunction.Definition.TrimEnd('\r', '\n', ' ')}");
            sb.AppendLine($"{Delimiter}");
            sb.AppendLine("DELIMITER ;");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropFunction(ABaseDbFunction sqlFunction, IReadOnlyList<ABaseDbDataType> dataTypes)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP FUNCTION {this.ScriptHelper.ScriptObjectName(sqlFunction)};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterFunction(ABaseDbFunction sourceFunction, ABaseDbFunction targetFunction, IReadOnlyList<ABaseDbDataType> dataTypes)
        {
            var sb = new StringBuilder();
            sb.AppendLine(this.ScriptDropFunction(targetFunction, dataTypes));
            sb.AppendLine(this.ScriptCreateFunction(sourceFunction, dataTypes));
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateStoredProcedure(ABaseDbStoredProcedure storedProcedure)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DELIMITER {Delimiter}");
            sb.Append($"{storedProcedure.Definition.TrimEnd('\r', '\n', ' ')}");
            sb.AppendLine($"{Delimiter}");
            sb.AppendLine("DELIMITER ;");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropStoredProcedure(ABaseDbStoredProcedure storedProcedure)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP PROCEDURE {this.ScriptHelper.ScriptObjectName(storedProcedure)};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterStoredProcedure(ABaseDbStoredProcedure sourceStoredProcedure, ABaseDbStoredProcedure targetStoredProcedure)
        {
            var sb = new StringBuilder();
            sb.AppendLine(this.ScriptDropStoredProcedure(targetStoredProcedure));
            sb.AppendLine(this.ScriptCreateStoredProcedure(sourceStoredProcedure));
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateTrigger(ABaseDbTrigger trigger)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DELIMITER {Delimiter}");
            sb.Append($"{trigger.Definition.TrimEnd('\r', '\n', ' ')}");
            sb.AppendLine($"{Delimiter}");
            sb.AppendLine("DELIMITER ;");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptDropTrigger(ABaseDbTrigger trigger)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP TRIGGER {this.ScriptHelper.ScriptObjectName(trigger)};");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptAlterTrigger(ABaseDbTrigger sourceTrigger, ABaseDbTrigger targetTrigger)
        {
            var sb = new StringBuilder();
            sb.AppendLine(this.ScriptDropTrigger(targetTrigger));
            sb.AppendLine(this.ScriptCreateTrigger(sourceTrigger));
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateSequence(ABaseDbSequence sequence)
        {
            throw new NotSupportedException("MySQL doesn't support sequences");
        }

        /// <inheritdoc/>
        protected override string ScriptDropSequence(ABaseDbSequence sequence)
        {
            throw new NotSupportedException("MySQL doesn't support sequences");
        }

        /// <inheritdoc/>
        protected override string ScriptAlterSequence(ABaseDbSequence sourceSequence, ABaseDbSequence targetSequence)
        {
            throw new NotSupportedException("MySQL doesn't support sequences");
        }

        /// <inheritdoc />
        protected override string ScriptCreateType(ABaseDbDataType type, IReadOnlyList<ABaseDbDataType> dataTypes)
        {
            throw new NotSupportedException("MySQL doesn't support user defined types");
        }

        /// <inheritdoc />
        protected override string ScriptDropType(ABaseDbDataType type)
        {
            throw new NotSupportedException("MySQL doesn't support user defined types");
        }

        /// <inheritdoc />
        protected override string ScriptAlterType(ABaseDbDataType sourceType, ABaseDbDataType targetType, IReadOnlyList<ABaseDbDataType> dataTypes)
        {
            throw new NotSupportedException("MySQL doesn't support user defined types");
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
            return table.Columns.OrderBy(x => ((MySqlColumn)x).OrdinalPosition);
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
            return indexes.OrderBy(x => x.Schema).ThenBy(x => x.Name);
        }
    }
}