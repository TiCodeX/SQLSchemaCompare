namespace TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters
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
        /// Get the database object name
        /// </summary>
        /// <param name="dbObject">The database object</param>
        /// <returns>The normalized database object name</returns>
        public string ScriptObjectName(ABaseDbObject dbObject)
        {
            if (dbObject == null)
            {
                throw new ArgumentNullException(nameof(dbObject));
            }

            return this.ScriptObjectName(dbObject.Schema, dbObject.Name);
        }

        /// <summary>
        /// Get the database object name
        /// </summary>
        /// <param name="objectName">The object name</param>
        /// <returns>The normalized database object name</returns>
        public string ScriptObjectName(string objectName)
        {
            return this.ScriptObjectName(string.Empty, objectName);
        }

        /// <summary>
        /// Get the database object name
        /// </summary>
        /// <param name="objectSchema">The object schema</param>
        /// <param name="objectName">The object name</param>
        /// <returns>The normalized database object name</returns>
        public abstract string ScriptObjectName(string objectSchema, string objectName);

        /// <summary>
        /// Script the given table column
        /// </summary>
        /// <param name="column">The table column</param>
        /// <returns>The column script</returns>
        public string ScriptColumn(ABaseDbColumn column)
        {
            return this.ScriptColumn(column, true);
        }

        /// <summary>
        /// Script the given table column
        /// </summary>
        /// <param name="column">The table column</param>
        /// <param name="scriptDefaultConstraint">True is the default constraint should be scripted in the column</param>
        /// <returns>The column script</returns>
        public abstract string ScriptColumn(ABaseDbColumn column, bool scriptDefaultConstraint);

        /// <summary>
        /// Scripts the column default value.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>The column default value</returns>
        public abstract string ScriptColumnDefaultValue(ABaseDbColumn column);

        /// <summary>
        /// Scripts the command to commit a transaction
        /// </summary>
        /// <returns>The script</returns>
        public abstract string ScriptCommitTransaction();
    }
}
