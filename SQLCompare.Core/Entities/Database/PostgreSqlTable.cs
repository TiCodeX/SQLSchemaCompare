namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Specific PostgreSql table definition
    /// </summary>
    public class PostgreSqlTable : ABaseDbTable
    {
        /// <summary>
        /// Gets or sets the table self referencing column name
        /// </summary>
        public string SelfReferencingColumnName { get; set; }

        /// <summary>
        /// Gets or sets the reference generation
        /// </summary>
        public string ReferenceGeneration { get; set; }

        /// <summary>
        /// Gets or sets the the user defined type catalog
        /// </summary>
        public string UserDefinedTypeCatalog { get; set; }

        /// <summary>
        /// Gets or sets the user defined type schema
        /// </summary>
        public string UserDefinedTypeSchema { get; set; }

        /// <summary>
        /// Gets or sets the user defined type name
        /// </summary>
        public string UserDefinedTypeName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether it's possible to insert into the table
        /// </summary>
        public bool IsInsertableInto { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the table is typed
        /// </summary>
        public bool IsTyped { get; set; }

        /// <summary>
        /// Gets or sets the commit action
        /// </summary>
        public string CommitAction { get; set; }
    }
}