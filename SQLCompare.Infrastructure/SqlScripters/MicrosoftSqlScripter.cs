using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.MicrosoftSql;
using SQLCompare.Core.Entities.Project;
using System;
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

            sb.AppendLine(");");
            return sb.ToString();
        }

        /// <summary>
        /// Get the table name
        /// </summary>
        /// <param name="table">The table</param>
        /// <returns>The column script</returns>
        public string ScriptTableName(ABaseDbTable table)
        {
            if (this.options.Scripting.UseSchemaName)
            {
                return $"[{table.TableSchema}].[{table.Name}]";
            }
            else
            {
                return $"[{table.Name}]";
            }
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