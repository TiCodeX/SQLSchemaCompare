namespace TiCodeX.SQLSchemaCompare.Core.Entities.Compare
{
    using System.Collections.Generic;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Database;

    /// <summary>
    /// Structure class useful for mapping objects
    /// </summary>
    public class ObjectMap
    {
        /// <summary>
        /// Gets or sets a generic title for the objects list
        /// </summary>
        public string ObjectTitle { get; set; }

        /// <summary>
        /// Gets or sets the database objects
        /// </summary>
        public IEnumerable<ABaseDbObject> DbObjects { get; set; }

        /// <summary>
        /// Gets or sets the mappable database objects
        /// </summary>
        public IEnumerable<ABaseDbObject> MappableDbObjects { get; set; }
    }
}
