namespace TiCodeX.SQLSchemaCompare.Core.Entities.Compare
{
    using System.Collections.Generic;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Database;

    /// <summary>
    /// The various compare result items
    /// </summary>
    public class CompareResultItems
    {
        /// <summary>
        /// Gets the schemas
        /// </summary>
        public List<CompareResultItem<ABaseDbSchema>> Schemas { get; } = new List<CompareResultItem<ABaseDbSchema>>();

        /// <summary>
        /// Gets the tables
        /// </summary>
        public List<CompareResultItem<ABaseDbTable>> Tables { get; } = new List<CompareResultItem<ABaseDbTable>>();

        /// <summary>
        /// Gets the indexes
        /// </summary>
        public List<CompareResultItem<ABaseDbIndex>> Indexes { get; } = new List<CompareResultItem<ABaseDbIndex>>();

        /// <summary>
        /// Gets the constraints
        /// </summary>
        public List<CompareResultItem<ABaseDbConstraint>> Constraints { get; } = new List<CompareResultItem<ABaseDbConstraint>>();

        /// <summary>
        /// Gets the primary keys
        /// </summary>
        public List<CompareResultItem<ABaseDbPrimaryKey>> PrimaryKeys { get; } = new List<CompareResultItem<ABaseDbPrimaryKey>>();

        /// <summary>
        /// Gets the foreign keys
        /// </summary>
        public List<CompareResultItem<ABaseDbForeignKey>> ForeignKeys { get; } = new List<CompareResultItem<ABaseDbForeignKey>>();

        /// <summary>
        /// Gets the triggers
        /// </summary>
        public List<CompareResultItem<ABaseDbTrigger>> Triggers { get; } = new List<CompareResultItem<ABaseDbTrigger>>();

        /// <summary>
        /// Gets the views
        /// </summary>
        public List<CompareResultItem<ABaseDbView>> Views { get; } = new List<CompareResultItem<ABaseDbView>>();

        /// <summary>
        /// Gets the functions
        /// </summary>
        public List<CompareResultItem<ABaseDbFunction>> Functions { get; } = new List<CompareResultItem<ABaseDbFunction>>();

        /// <summary>
        /// Gets the stored procedures
        /// </summary>
        public List<CompareResultItem<ABaseDbStoredProcedure>> StoredProcedures { get; } = new List<CompareResultItem<ABaseDbStoredProcedure>>();

        /// <summary>
        /// Gets the sequences
        /// </summary>
        public List<CompareResultItem<ABaseDbSequence>> Sequences { get; } = new List<CompareResultItem<ABaseDbSequence>>();

        /// <summary>
        /// Gets the data types
        /// </summary>
        public List<CompareResultItem<ABaseDbDataType>> DataTypes { get; } = new List<CompareResultItem<ABaseDbDataType>>();
    }
}
