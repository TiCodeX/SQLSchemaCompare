using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.EntityFramework;
using System;
using System.Text;

namespace SQLCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Sql scripter class specific for MicrosoftSql database
    /// </summary>
    internal class MicrosoftSqlScripter
    {
        private readonly ILogger logger;
        private object options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlScripter"/> class.
        /// </summary>
        /// <param name="logger">The injected logger instance</param>
        /// <param name="options">The scripter options</param>
        public MicrosoftSqlScripter(ILogger logger, object options)
        {
            this.logger = logger;
            this.options = options;
        }

        /// <summary>
        /// Scripts the given database table
        /// </summary>
        /// <param name="table">The database table</param>
        /// <returns>The create table script</returns>
        public string ScriptCreateTable(InformationSchemaTable table)
        {
            this.logger.LogInformation(string.Empty);

            var sql = new StringBuilder();

            if (string.Equals(table.TableType, "BASE TABLE", StringComparison.Ordinal))
            {
                sql.AppendLine($"CREATE TABLE {this.GetTableName(table)} (");
                var i = 0;
                foreach (var col in table.Columns)
                {
                    sql.Append($"   {this.ScriptColumn(col)}");
                    if (i++ < table.Columns.Count - 1)
                    {
                        sql.AppendLine(",");
                    }
                }

                sql.AppendLine(string.Empty);
                sql.AppendLine(");");
            }

            return sql.ToString();
        }

        private string ScriptColumn(InformationSchemaColumn col)
        {
            this.logger.LogInformation(string.Empty);
            var sql = new StringBuilder();

            sql.Append($"{col.ColumnName} {this.ScriptColumnDataType(col)} ");
            if (string.Equals(col.IsNullable, "YES", StringComparison.Ordinal))
            {
                sql.Append("NULL ");
            }
            else
            {
                sql.Append("NOT NULL ");
            }

            if (!string.IsNullOrWhiteSpace(col.ColumnDefault))
            {
                sql.Append($"DEFAULT {col.ColumnDefault}");
            }

            return sql.ToString().TrimEnd();
        }

        private object ScriptColumnDataType(InformationSchemaColumn col)
        {
            this.logger.LogInformation(string.Empty);
            switch (col.DataType)
            {
                case "varchar":
                case "nvarchar":
                    return $"[{col.DataType}] ({col.CharacterMaximumLength})";

                default:
                    return $"[{col.DataType}]";
            }
        }

        private string GetTableName(InformationSchemaTable table)
        {
            this.logger.LogInformation(string.Empty);

            // Depending on options
            var useSimpleSintax = true;
            var name = string.Empty;

            if (!string.IsNullOrWhiteSpace(table.TableCatalog) || !useSimpleSintax)
            {
                name += $"[{table.TableCatalog}].";
            }

            if (!string.IsNullOrWhiteSpace(table.TableSchema) || !useSimpleSintax)
            {
                name += $"[{table.TableSchema}].";
            }

            name += $"[{table.TableName}]";

            return name;
        }
    }
}
