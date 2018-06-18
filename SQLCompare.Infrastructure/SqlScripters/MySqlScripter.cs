using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.MySql;
using SQLCompare.Core.Entities.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            StringBuilder sb = new StringBuilder();
            IEnumerable<string> columnList;
            foreach (var keys in table.PrimaryKeys.GroupBy(x => x.Name))
            {
                var key = keys.FirstOrDefault() as MySqlPrimaryKey;
                columnList = keys.OrderBy(x => ((MySqlPrimaryKey)x).OrdinalPosition).Select(x => $"[{((MySqlPrimaryKey)x).ColumnName}]");

                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptTableName(table)}");
                sb.AppendLine($"ADD CONSTRAINT [{key.Name}] PRIMARY KEY ({string.Join(",", columnList)});");
                sb.AppendLine("GO");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptForeignKeysAlterTable(ABaseDbTable table)
        {
            StringBuilder sb = new StringBuilder();

            foreach (MySqlForeignKey key in table.ForeignKeys.OrderBy(x => x.Name))
            {
                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptTableName(table)} WITH CHECK ADD CONSTRAINT [{key.Name}] FOREIGN KEY([{key.ColumnName}])");
                sb.AppendLine($"REFERENCES {this.ScriptHelper.ScriptTableName(key.ReferencedTableSchema, key.ReferencedTableName)}([{key.ReferencedColumnName}])");
                sb.AppendLine($"ON DELETE {key.DeleteRule}");
                sb.AppendLine($"ON UPDATE {key.UpdateRule}");
                sb.AppendLine("GO");
                sb.AppendLine();
                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptTableName(table)} CHECK CONSTRAINT[{key.Name}]");
                sb.AppendLine("GO");
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}