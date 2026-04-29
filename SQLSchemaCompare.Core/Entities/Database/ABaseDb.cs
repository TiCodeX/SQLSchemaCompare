namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database;

/// <summary>
/// Provides generic information of database classes
/// </summary>
public abstract class ABaseDb
{
    /// <summary>
    /// Gets or sets the database name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the server version
    /// </summary>
    public Version ServerVersion { get; set; } = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);

    /// <summary>
    /// Gets or sets the database compare direction (source or target database)
    /// </summary>
    public CompareDirection Direction { get; set; }

    /// <summary>
    /// Gets the database schemas
    /// </summary>
    public List<ABaseDbSchema> Schemas { get; } = [];

    /// <summary>
    /// Gets the database tables
    /// </summary>
    public List<ABaseDbTable> Tables { get; } = [];

    /// <summary>
    /// Gets the database indexes
    /// </summary>
    public List<ABaseDbIndex> Indexes { get; } = [];

    /// <summary>
    /// Gets the database constraints
    /// </summary>
    public List<ABaseDbConstraint> Constraints { get; } = [];

    /// <summary>
    /// Gets the database constraints
    /// </summary>
    public List<ABaseDbPrimaryKey> PrimaryKeys { get; } = [];

    /// <summary>
    /// Gets the database foreign keys
    /// </summary>
    public List<ABaseDbForeignKey> ForeignKeys { get; } = [];

    /// <summary>
    /// Gets the database triggers
    /// </summary>
    public List<ABaseDbTrigger> Triggers { get; } = [];

    /// <summary>
    /// Gets the database views
    /// </summary>
    public List<ABaseDbView> Views { get; } = [];

    /// <summary>
    /// Gets the database functions
    /// </summary>
    public List<ABaseDbFunction> Functions { get; } = [];

    /// <summary>
    /// Gets the database stored procedures
    /// </summary>
    public List<ABaseDbStoredProcedure> StoredProcedures { get; } = [];

    /// <summary>
    /// Gets the database data types
    /// </summary>
    public List<ABaseDbDataType> DataTypes { get; } = [];

    /// <summary>
    /// Gets the database sequences
    /// </summary>
    public List<ABaseDbSequence> Sequences { get; } = [];
}
