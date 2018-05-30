using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
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
        public string ScriptCreateTable(MicrosoftSqlTable table)
        {
            this.logger.LogInformation($"Generating SQL script for table '{GetTableName(table)}'");

            var sql = new StringBuilder();

            sql.AppendLine($"CREATE TABLE {GetTableName(table)} (");
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
            if (col.IsNullable)
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
            var useSimpleSintax = true;
            var name = string.Empty;

            if (!string.IsNullOrWhiteSpace(table.CatalogName) || !useSimpleSintax)
            {
                name += $"[{table.CatalogName}].";
            }

            if (!string.IsNullOrWhiteSpace(table.SchemaName) || !useSimpleSintax)
            {
                name += $"[{table.SchemaName}].";
            }

            name += $"[{table.Name}]";

            return name;
        }
    }
}
