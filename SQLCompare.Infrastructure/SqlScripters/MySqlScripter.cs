using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.MySql;
using SQLCompare.Core.Entities.Project;

namespace SQLCompare.Infrastructure.SqlScripters
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

        /// <inheritdoc/>
        protected override string ScriptCreateTable(ABaseDbTable table, ABaseDbTable sourceTable)
        {
            var ncol = table.Columns.Count;

            var columns = this.GetSortedTableColumns(table, sourceTable);

            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE {this.ScriptHelper.ScriptTableName(table)}(");

            var i = 0;
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
            foreach (var keys in table.PrimaryKeys.GroupBy(x => x.Name))
            {
                var key = (MySqlPrimaryKey)keys.First();
                var columnList = keys.OrderBy(x => ((MySqlPrimaryKey)x).OrdinalPosition).Select(x => $"`{((MySqlPrimaryKey)x).ColumnName}`");

                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptTableName(table)}");
                sb.AppendLine($"ADD CONSTRAINT `{key.Name}` PRIMARY KEY ({string.Join(",", columnList)});");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptForeignKeysAlterTable(ABaseDbTable table)
        {
            var sb = new StringBuilder();

            foreach (var keys in table.ForeignKeys.GroupBy(x => x.Name))
            {
                var key = (MySqlForeignKey)keys.First();
                var columnList = keys.OrderBy(x => ((MySqlForeignKey)x).OrdinalPosition).Select(x => $"`{((MySqlForeignKey)x).ColumnName}`");
                var referencedColumnList = keys.OrderBy(x => ((MySqlForeignKey)x).OrdinalPosition).Select(x => $"`{((MySqlForeignKey)x).ReferencedColumnName}`");

                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptTableName(table)}");
                sb.AppendLine($"ADD CONSTRAINT `{key.Name}` FOREIGN KEY ({string.Join(",", columnList)})");
                sb.AppendLine($"REFERENCES {this.ScriptHelper.ScriptTableName(key.ReferencedTableSchema, key.ReferencedTableName)} ({string.Join(",", referencedColumnList)})");
                sb.AppendLine($"ON DELETE {key.DeleteRule}");
                sb.AppendLine($"ON UPDATE {key.UpdateRule};");
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}