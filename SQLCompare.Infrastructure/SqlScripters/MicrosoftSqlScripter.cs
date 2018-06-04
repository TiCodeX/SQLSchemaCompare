﻿using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Interfaces;
using System.Text;

namespace SQLCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Sql scripter class specific for MicrosoftSql database
    /// </summary>
    internal class MicrosoftSqlScripter : IDatabaseScripter
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
        public string ScriptCreateTable(ABaseDbTable table)
        {
            this.logger.LogInformation($"Generating SQL script for table '{GetTableName(table as MicrosoftSqlTable)}'");

            var sql = new StringBuilder();

            sql.AppendLine($"CREATE TABLE {GetTableName(table as MicrosoftSqlTable)} (");
            var i = 0;
            foreach (var col in table.Columns)
            {
                sql.Append($"   {ScriptColumn(col as MicrosoftSqlColumn)}");
                if (i++ < table.Columns.Count - 1)
                {
                    sql.AppendLine(",");
                }
            }

            sql.AppendLine(string.Empty);
            sql.AppendLine(");");

            return sql.ToString();
        }

        private static string ScriptColumn(MicrosoftSqlColumn col)
        {
            var sql = new StringBuilder();

            sql.Append($"{col.Name} {ScriptColumnDataType(col)} ");
            sql.Append(col.IsNullable ? "NULL " : "NOT NULL ");

            if (!string.IsNullOrWhiteSpace(col.ColumnDefault))
            {
                sql.Append($"DEFAULT {col.ColumnDefault}");
            }

            return sql.ToString().TrimEnd();
        }

        private static object ScriptColumnDataType(MicrosoftSqlColumn col)
        {
            switch (col.DataType)
            {
                case "varchar":
                case "nvarchar":
                    return $"[{col.DataType}] ({col.CharacterMaxLenght})";

                default:
                    return $"[{col.DataType}]";
            }
        }

        private static string GetTableName(MicrosoftSqlTable table)
        {
            // Depending on options
            var name = string.Empty;

            if (!string.IsNullOrWhiteSpace(table.TableCatalog))
            {
                name += $"[{table.TableCatalog}].";
            }

            if (!string.IsNullOrWhiteSpace(table.TableSchema))
            {
                name += $"[{table.TableSchema}].";
            }

            name += $"[{table.Name}]";

            return name;
        }
    }
}
