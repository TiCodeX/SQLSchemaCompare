using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.PostgreSql;
using SQLCompare.Core.Entities.Project;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Sql scripter class specific for PostgreSql database
    /// </summary>
    internal class PostgreSqlScripter : ADatabaseScripter<PostgreSqlScriptHelper>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlScripter"/> class.
        /// </summary>
        /// <param name="logger">The injected logger instance</param>
        /// <param name="options">The project options</param>
        public PostgreSqlScripter(ILogger logger, ProjectOptions options)
            : base(logger, options, new PostgreSqlScriptHelper(options))
        {
        }

        /// <inheritdoc/>
        protected override string ScriptCreateTable(ABaseDbTable table)
        {
            var ncol = table.Columns.Count;

            var columns = table.Columns.AsEnumerable();
            if (this.Options.Scripting.OrderColumnAlphabetically)
            {
                columns = table.Columns.OrderBy(x => x.Name);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE {this.ScriptHelper.ScriptTableName(table)}(");

            int i = 0;
            foreach (var col in columns)
            {
                sb.Append($"{this.Indent}{this.ScriptHelper.ScriptColumn(col)}");
                sb.AppendLine((++i == ncol) ? string.Empty : ",");
            }

            sb.AppendLine(");");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptPrimaryKeysAlterTable(ABaseDbTable table)
        {
            var sb = new StringBuilder();
            if (sb != null)
            {
                return "Not implemented";
            }

            IEnumerable<string> columnList;
            foreach (var keys in table.PrimaryKeys.GroupBy(x => x.Name))
            {
                var key = (PostgreSqlPrimaryKey)keys.First();
                columnList = keys.OrderBy(x => ((PostgreSqlPrimaryKey)x).OrdinalPosition).Select(x => $"\"{((PostgreSqlPrimaryKey)x).ColumnName}\"");

                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptTableName(table)}");
                sb.AppendLine($"ADD CONSTRAINT \"{key.Name}\" PRIMARY KEY ({string.Join(",", columnList)});");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptForeignKeysAlterTable(ABaseDbTable table)
        {
            var sb = new StringBuilder();
            if (sb != null)
            {
                return "Not implemented";
            }

            foreach (var keys in table.ForeignKeys.GroupBy(x => x.Name))
            {
                var key = (PostgreSqlForeignKey)keys.First();
                var columnList = keys.OrderBy(x => ((PostgreSqlForeignKey)x).OrdinalPosition).Select(x => $"\"{((PostgreSqlForeignKey)x).ColumnName}\"");
                var referencedColumnList = keys.OrderBy(x => ((PostgreSqlForeignKey)x).OrdinalPosition).Select(x => $"\"{((PostgreSqlForeignKey)x).ReferencedColumnName}\"");

                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptTableName(table)}");
                sb.AppendLine($"ADD CONSTRAINT \"{key.Name}\" FOREIGN KEY ({string.Join(",", columnList)})");
                sb.AppendLine($"REFERENCES {this.ScriptHelper.ScriptTableName(key.ReferencedTableSchema, key.ReferencedTableName)} ({string.Join(",", referencedColumnList)})");
                sb.AppendLine($"ON DELETE {key.DeleteRule}");
                sb.AppendLine($"ON UPDATE {key.UpdateRule};");
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}