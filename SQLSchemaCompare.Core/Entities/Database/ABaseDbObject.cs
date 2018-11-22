namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database
{
    /// <summary>
    /// Provides the common properties of a database object
    /// </summary>
    public abstract class ABaseDbObject
    {
        /// <summary>
        /// Gets or sets the object name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the object schema
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the alter script is supported
        /// </summary>
        public bool AlterScriptSupported { get; protected set; } = true;
    }
}
