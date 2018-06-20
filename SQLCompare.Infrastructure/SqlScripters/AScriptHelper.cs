using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Project;

namespace SQLCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Implement common scripter helper functionality
    /// </summary>
    public abstract class AScriptHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AScriptHelper"/> class.
        /// </summary>
        /// <param name="options">The project options</param>
        protected AScriptHelper(ProjectOptions options)
        {
            this.Options = options;
        }

        /// <summary>
        /// Gets the project options
        /// </summary>
        protected ProjectOptions Options { get; }

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
        /// Get the table name
        /// </summary>
        /// <param name="table">The table</param>
        /// <returns>The normalized table name</returns>
        public string ScriptTableName(ABaseDbTable table)
        {
            return this.ScriptTableName(table.TableSchema, table.Name);
        }

        /// <summary>
        /// Get the table name
        /// </summary>
        /// <param name="tableSchema">The table schema</param>
        /// <param name="tableName">The table name</param>
        /// <returns>The normalized table name</returns>
        public abstract string ScriptTableName(string tableSchema, string tableName);

        /// <summary>
        /// Script the given table column
        /// </summary>
        /// <param name="column">The table column</param>
        /// <returns>The column script</returns>
        public abstract string ScriptColumn(ABaseDbColumn column);
    }
}
