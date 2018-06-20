using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.MicrosoftSql;
using SQLCompare.Core.Entities.Project;
using System.Linq;
using System.Text;

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
        protected override string ScriptCreateTable(ABaseDbTable table)
        {
            var ncol = table.Columns.Count;

            var columns = table.Columns.AsEnumerable();
            if (this.Options.Scripting.OrderColumnAlphabetically)
            {
                columns = table.Columns.OrderBy(x => x.Name);
            }

            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE {this.ScriptHelper.ScriptTableName(table)}(");

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
                var key = (MicrosoftSqlPrimaryKey)keys.First();
                var columnList = keys.OrderBy(x => ((MicrosoftSqlPrimaryKey)x).OrdinalPosition).Select(x => $"[{((MicrosoftSqlPrimaryKey)x).ColumnName}]");

                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptTableName(table)}");
                sb.AppendLine($"ADD CONSTRAINT [{key.Name}] PRIMARY KEY {key.TypeDescription}({string.Join(",", columnList)});");
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

                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptTableName(table)} WITH CHECK ADD CONSTRAINT [{key.Name}] FOREIGN KEY([{key.TableColumn}])");
                sb.AppendLine($"REFERENCES {this.ScriptHelper.ScriptTableName(key.ReferencedTableSchema, key.ReferencedTableName)}([{key.ReferencedTableColumn}])");
                sb.AppendLine($"ON DELETE {MicrosoftSqlScriptHelper.ScriptForeignKeyAction(key.DeleteReferentialAction)}");
                sb.AppendLine($"ON UPDATE {MicrosoftSqlScriptHelper.ScriptForeignKeyAction(key.UpdateReferentialAction)}");
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