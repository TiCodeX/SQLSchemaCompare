namespace TiCodeX.SQLSchemaCompare.Infrastructure.DatabaseUtilities;

/// <summary>
/// Implements the database mapper functionality
/// </summary>
public class DatabaseMapper(ILogger<DatabaseMapper> logger) : IDatabaseMapper
{
    /// <inheritdoc/>
    public void PerformMapping(ABaseDb source, ABaseDb target, object mappingTable, TaskInfo taskInfo)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        // Linearize the 2 databases for mapping
        var maps = new List<ObjectMap>
        {
            new() { ObjectTitle = Localization.StatusMappingSchemas, DbObjects = source.Schemas, MappableDbObjects = target.Schemas },
            new() { ObjectTitle = Localization.StatusMappingTables, DbObjects = source.Tables, MappableDbObjects = target.Tables },
            new() { ObjectTitle = Localization.StatusMappingViews, DbObjects = source.Views, MappableDbObjects = target.Views },
            new() { ObjectTitle = Localization.StatusMappingFunctions, DbObjects = source.Functions, MappableDbObjects = target.Functions },
            new() { ObjectTitle = Localization.StatusMappingStoredProcedures, DbObjects = source.StoredProcedures, MappableDbObjects = target.StoredProcedures },
            new() { ObjectTitle = Localization.StatusMappingDataTypes, DbObjects = source.DataTypes, MappableDbObjects = target.DataTypes },
            new() { ObjectTitle = Localization.StatusMappingSequences, DbObjects = source.Sequences, MappableDbObjects = target.Sequences },
        };

        logger.LogInformation("Mapping source and target database objects...");
        this.PerformMapping(maps, taskInfo);
    }

    /// <summary>
    /// Perform the mapping
    /// </summary>
    /// <param name="maps">The objects to map</param>
    /// <param name="taskInfo">The task info</param>
    [SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "TODO")]
    private void PerformMapping(IReadOnlyCollection<ObjectMap> maps, TaskInfo taskInfo = null)
    {
        var i = 1;
        var count = maps.Count;

        // Iterate the linearized db
        foreach (var m in maps)
        {
            taskInfo?.Message = m.ObjectTitle;

            foreach (var sourceObject in m.DbObjects)
            {
                var targetObject = m.MappableDbObjects.FirstOrDefault(x => x.Schema == sourceObject.Schema && x.Name == sourceObject.Name);
                if (targetObject != null)
                {
                    sourceObject.Schema = targetObject.Schema;
                    sourceObject.Name = targetObject.Name;

                    sourceObject.MappedDbObject = targetObject;
                    targetObject.MappedDbObject = sourceObject;

                    if (sourceObject is ABaseDbTable table)
                    {
                        this.PerformTableMapping(table);
                    }
                    else if (sourceObject is ABaseDbView view)
                    {
                        this.PerformViewMapping(view);
                    }
                    else
                    {
                        // Do nothing
                    }
                }
            }

            taskInfo?.CancellationToken.ThrowIfCancellationRequested();
            taskInfo?.Percentage = (short)(100 * i++ / count);
        }
    }

    /// <summary>
    /// Perform the mapping of the table
    /// </summary>
    /// <param name="sourceTable">The source table</param>
    private void PerformTableMapping(ABaseDbTable sourceTable)
    {
        if (sourceTable.MappedDbObject is not ABaseDbTable targetTable)
        {
            throw new ArgumentException($"{nameof(sourceTable.MappedDbObject)} is null");
        }

        // Linearize the 2 tables for mapping
        var maps = new List<ObjectMap>
        {
            new() { DbObjects = sourceTable.Columns, MappableDbObjects = targetTable.Columns },
            new() { DbObjects = sourceTable.Indexes, MappableDbObjects = targetTable.Indexes },
            new() { DbObjects = sourceTable.ForeignKeys, MappableDbObjects = targetTable.ForeignKeys },
            new() { DbObjects = sourceTable.Constraints, MappableDbObjects = targetTable.Constraints },
            new() { DbObjects = sourceTable.Triggers, MappableDbObjects = targetTable.Triggers },
            new() { DbObjects = sourceTable.PrimaryKeys, MappableDbObjects = targetTable.PrimaryKeys },
        };

        this.PerformMapping(maps);
    }

    /// <summary>
    /// Perform the mapping of the view
    /// </summary>
    /// <param name="sourceView">The source view</param>
    private void PerformViewMapping(ABaseDbView sourceView)
    {
        if (sourceView.MappedDbObject is not ABaseDbView targetView)
        {
            throw new ArgumentException($"{nameof(sourceView.MappedDbObject)} is null");
        }

        // Linearize the 2 views for mapping
        var maps = new List<ObjectMap>
        {
            new() { DbObjects = sourceView.Indexes, MappableDbObjects = targetView.Indexes },
        };

        this.PerformMapping(maps);
    }
}
