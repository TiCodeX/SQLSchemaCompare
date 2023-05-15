namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database
{
    using TiCodeX.SQLSchemaCompare.Core.Enums;

    /// <summary>
    /// Provides the common properties of a database object
    /// </summary>
    public abstract class ABaseDbObject
    {
        /// <summary>
        /// Gets the object type
        /// </summary>
        public abstract DatabaseObjectType ObjectType { get; }

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

        /// <summary>
        /// Gets or sets the mapped object
        /// </summary>
        public ABaseDbObject MappedDbObject { get; set; }

        /// <summary>
        /// Gets or sets the database
        /// </summary>
        public ABaseDb Database { get; set; }

        /// <summary>
        /// Gets or sets the create script
        /// </summary>
        public string CreateScript { get; set; }

        /// <summary>
        /// Gets or sets the alter script
        /// </summary>
        public string AlterScript { get; set; }
    }
}
