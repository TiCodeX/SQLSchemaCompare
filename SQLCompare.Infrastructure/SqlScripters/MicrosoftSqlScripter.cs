using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.MicrosoftSql;
using SQLCompare.Core.Entities.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Sql scripter class specific for MicrosoftSql database
    /// </summary>
    internal class MicrosoftSqlScripter : ADatabaseScripter
    {
        private readonly ILogger logger;
        private ProjectOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlScripter"/> class.
        /// </summary>
        /// <param name="logger">The injected logger instance</param>
        /// <param name="options">The project options</param>
        public MicrosoftSqlScripter(ILogger logger, ProjectOptions options)
        {
            this.logger = logger;
            this.options = options;
        }

        /// <summary>
        /// Script a comment text
        /// </summary>
        /// <param name="comment">The comment text</param>
        /// <returns>The scripted comment</returns>
        public static string ScriptComment(string comment)
        {
            return $"/****** {comment} ******/";
        }

        /// <summary>
        /// Script the foreign key reference action
        /// </summary>
        /// <param name="action">The reference action</param>
        /// <returns>The scripted action</returns>
        public static string ScriptForeignKeyAction(MicrosoftSqlForeignKey.ReferentialAction action)
        {
            switch (action)
            {
                case MicrosoftSqlForeignKey.ReferentialAction.NOACTION: return "NO ACTION";
                case MicrosoftSqlForeignKey.ReferentialAction.CASCADE: return "CASCADE";
                case MicrosoftSqlForeignKey.ReferentialAction.SETDEFAULT: return "SET DEFAULT";
                case MicrosoftSqlForeignKey.ReferentialAction.SETNULL: return "SET NULL";
                default:
                    throw new ArgumentException("Invalid referential action: " + action.ToString(), nameof(action));
            }
        }

        /// <summary>
        /// Scripts the given database table
        /// </summary>
        /// <param name="table">The database table</param>
        /// <returns>The create table script</returns>
        public override string ScriptCreateTable(ABaseDbTable table)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            var ncol = table.Columns.Count;

            var columns = table.Columns.AsEnumerable();
            if (this.options.Scripting.OrderColumnAlphabetically)
            {
                columns = table.Columns.OrderBy(x => x.Name);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE {this.ScriptTableName(table)}(");

            int i = 0;
            foreach (var col in columns)
            {
                sb.Append($"{ADatabaseScripter.Indent}{this.ScriptColumn(col)}");
                sb.AppendLine((++i == ncol) ? string.Empty : ",");
            }

            sb.AppendLine(")");
            sb.AppendLine("GO");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(ScriptComment("Constraints and Indexes"));
            sb.AppendLine(this.ScriptPrimaryKeys(table));
            sb.AppendLine(ScriptComment("Foreign keys"));
            sb.AppendLine(this.ScriptForeignKeys(table));

            return sb.ToString();
        }

        /// <summary>
        /// Script the table primary keys
        /// </summary>
        /// <param name="table">The table</param>
        /// <returns>The create primary keys script </returns>
        public string ScriptPrimaryKeys(ABaseDbTable table)
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<string> columnList;
            foreach (var keys in table.PrimaryKeys.GroupBy(x => x.Name))
            {
                var key = keys.FirstOrDefault() as MicrosoftSqlPrimaryKey;
                columnList = keys.OrderBy(x => ((MicrosoftSqlPrimaryKey)x).OrdinalPosition).Select(x => $"[{((MicrosoftSqlPrimaryKey)x).ColumnName}]");

                sb.AppendLine($"ALTER TABLE {this.ScriptTableName(table)}");
                sb.AppendLine($"ADD CONSTRAINT [{key.Name}] PRIMARY KEY {key.TypeDescription}({string.Join(",", columnList)});");
                sb.AppendLine("GO");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Script the table foreign keys
        /// </summary>
        /// <param name="table">The table</param>
        /// <returns>The create foreign keys script </returns>
        public string ScriptForeignKeys(ABaseDbTable table)
        {
            StringBuilder sb = new StringBuilder();

            foreach (MicrosoftSqlForeignKey key in table.ForeignKeys.OrderBy(x => x.Name))
            {
                sb.AppendLine($"ALTER TABLE {this.ScriptTableName(table)} WITH CHECK ADD CONSTRAINT [{key.Name}] FOREIGN KEY([{key.TableColumn}])");
                sb.AppendLine($"REFERENCES {this.ScriptTableName(key.ReferencedTableSchema, key.ReferencedTableName)}([{key.ReferencedTableColumn}])");
                sb.AppendLine($"ON DELETE {ScriptForeignKeyAction(key.DeleteReferentialAction)}");
                sb.AppendLine($"ON UPDATE {ScriptForeignKeyAction(key.UpdateReferentialAction)}");
                sb.AppendLine("GO");
                sb.AppendLine();
                sb.AppendLine($"ALTER TABLE {this.ScriptTableName(table)} CHECK CONSTRAINT[{key.Name}]");
                sb.AppendLine("GO");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get the table name
        /// </summary>
        /// <param name="tableSchema">The table schema</param>
        /// <param name="tableName">The table name</param>
        /// <returns>The normalized table name</returns>
        public string ScriptTableName(string tableSchema, string tableName)
        {
            if (this.options.Scripting.UseSchemaName)
            {
                return $"[{tableSchema}].[{tableName}]";
            }
            else
            {
                return $"[{tableName}]";
            }
        }

        /// <summary>
        /// Get the table name
        /// </summary>
        /// <param name="table">The table</param>
        /// <returns>The normalized table name</returns>
        public string ScriptTableName(ABaseDbTable table)
        {
            return this.ScriptTableName(table.TableSchema, table.Name);
        }

        /// <summary>
        /// Script the given talbe column
        /// </summary>
        /// <param name="column">The table column</param>
        /// <returns>The column script</returns>
        public string ScriptColumn(ABaseDbColumn column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            var col = column as MicrosoftSqlColumn;

            StringBuilder sb = new StringBuilder();

            sb.Append($"[{col.Name}] {this.ScriptDataType(col)} ");
            if (col.IsIdentity)
            {
                sb.Append($"IDENTITY({col.IdentitySeed},{col.IdentityIncrement}) ");
            }

            if (col.ColumnDefault != null)
            {
                sb.Append($"DEFAULT {col.ColumnDefault} ");
            }

            if (col.IsNullable)
            {
                sb.Append($"NULL");
            }
            else
            {
                sb.Append($"NOT NULL");
            }

            return sb.ToString();
        }

        private string ScriptDataType(MicrosoftSqlColumn column)
        {
            switch (column.DataType)
            {
                // Exact numerics
                case "bigint":
                case "int":
                case "smallint":
                case "tinyint":
                case "bit":
                case "smallmoney":
                case "money":
                    return $"[{column.DataType}]";
                case "numeric":
                case "decimal":
                    return $"[{column.DataType}]({column.NumericPrecision}, {column.NumericScale})";

                // Approximate numerics
                case "float":
                    return (column.NumericPrecision == 53) ? $"[{column.DataType}]" : $"[{column.DataType}]({column.NumericPrecision})";
                case "real":
                    return $"[{column.DataType}]";

                // Date and time
                case "date":
                case "datetime":
                case "smalldatetime":
                    return $"[{column.DataType}]";
                case "datetimeoffset":
                case "datetime2":
                case "time":
                    return $"[{column.DataType}]({column.DateTimePrecision})";

                // Character strings
                // Unicode character strings
                case "char":
                case "nchar":
                    {
                        string collate = this.options.Scripting.IgnoreCollate ? string.Empty : $" COLLATE {column.CollationName}";
                        return $"[{column.DataType}]({column.CharacterMaxLenght}){collate}";
                    }

                case "varchar":
                case "nvarchar":
                    {
                        string length = column.CharacterMaxLenght == -1 ? "max" : $"{column.CharacterMaxLenght}";
                        string collate = this.options.Scripting.IgnoreCollate ? string.Empty : $" COLLATE {column.CollationName}";

                        return $"[{column.DataType}]({length}){collate}";
                    }

                case "text":
                case "ntext":
                    {
                        string collate = this.options.Scripting.IgnoreCollate ? string.Empty : $" COLLATE {column.CollationName}";
                        return $"[{column.DataType}]{collate}";
                    }

                // Binary strings
                case "binary":
                    return $"[{column.DataType}]({column.CharacterMaxLenght})";
                case "varbinary":
                    {
                        string length = column.CharacterMaxLenght == -1 ? "max" : $"{column.CharacterMaxLenght}";
                        return $"[{column.DataType}]({length})";
                    }

                case "image":
                    return $"[{column.DataType}]";

                // Other data types
                case "cursor":
                case "rowversion":
                case "hierarchyid":
                case "uniqueidentifier":
                case "sql_variant":
                case "xml":
                    return $"[{column.DataType}]";
                default: throw new ArgumentException("Unknown column data type");
            }
        }
    }
}