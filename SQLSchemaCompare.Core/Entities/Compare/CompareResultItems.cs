namespace TiCodeX.SQLSchemaCompare.Core.Entities.Compare;

/// <summary>
/// The various compare result items
/// </summary>
public class CompareResultItems
{
    /// <summary>
    /// Gets the schemas
    /// </summary>
    public List<CompareResultItem<ABaseDbSchema>> Schemas { get; } = [];

    /// <summary>
    /// Gets the tables
    /// </summary>
    public List<CompareResultItem<ABaseDbTable>> Tables { get; } = [];

    /// <summary>
    /// Gets the indexes
    /// </summary>
    public List<CompareResultItem<ABaseDbIndex>> Indexes { get; } = [];

    /// <summary>
    /// Gets the constraints
    /// </summary>
    public List<CompareResultItem<ABaseDbConstraint>> Constraints { get; } = [];

    /// <summary>
    /// Gets the primary keys
    /// </summary>
    public List<CompareResultItem<ABaseDbPrimaryKey>> PrimaryKeys { get; } = [];

    /// <summary>
    /// Gets the foreign keys
    /// </summary>
    public List<CompareResultItem<ABaseDbForeignKey>> ForeignKeys { get; } = [];

    /// <summary>
    /// Gets the triggers
    /// </summary>
    public List<CompareResultItem<ABaseDbTrigger>> Triggers { get; } = [];

    /// <summary>
    /// Gets the views
    /// </summary>
    public List<CompareResultItem<ABaseDbView>> Views { get; } = [];

    /// <summary>
    /// Gets the functions
    /// </summary>
    public List<CompareResultItem<ABaseDbFunction>> Functions { get; } = [];

    /// <summary>
    /// Gets the stored procedures
    /// </summary>
    public List<CompareResultItem<ABaseDbStoredProcedure>> StoredProcedures { get; } = [];

    /// <summary>
    /// Gets the sequences
    /// </summary>
    public List<CompareResultItem<ABaseDbSequence>> Sequences { get; } = [];

    /// <summary>
    /// Gets the data types
    /// </summary>
    public List<CompareResultItem<ABaseDbDataType>> DataTypes { get; } = [];
}
