namespace TiCodeX.SQLSchemaCompare.Infrastructure.DatabaseUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TiCodeX.SQLSchemaCompare.Core.Entities;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Compare;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces;
    using TiCodeX.SQLSchemaCompare.Services;

    /// <summary>
    /// Implements the database mapper functionality
    /// </summary>
    public class DatabaseMapper : IDatabaseMapper
    {
        /// <inheritdoc/>
        public void PerformMapping(ABaseDb source, ABaseDb target, object mappingTable, TaskInfo taskInfo)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            // Linearize the 2 databases for mapping
            var maps = new List<ObjectMap>
            {
                new ObjectMap { ObjectTitle = Localization.StatusMappingSchemas, DbObjects = source.Schemas, MappableDbObjects = target.Schemas },
                new ObjectMap { ObjectTitle = Localization.StatusMappingTables, DbObjects = source.Tables, MappableDbObjects = target.Tables },
                new ObjectMap { ObjectTitle = Localization.StatusMappingViews, DbObjects = source.Views, MappableDbObjects = target.Views },
                new ObjectMap { ObjectTitle = Localization.StatusMappingFunctions, DbObjects = source.Functions, MappableDbObjects = target.Functions },
                new ObjectMap { ObjectTitle = Localization.StatusMappingStoredProcedures, DbObjects = source.StoredProcedures, MappableDbObjects = target.StoredProcedures },
                new ObjectMap { ObjectTitle = Localization.StatusMappingDataTypes, DbObjects = source.DataTypes, MappableDbObjects = target.DataTypes },
                new ObjectMap { ObjectTitle = Localization.StatusMappingSequences, DbObjects = source.Sequences, MappableDbObjects = target.Sequences },
            };

            this.PerformMapping(maps, taskInfo);
        }

        /// <summary>
        /// Perform the mapping
        /// </summary>
        /// <param name="maps">The objects to map</param>
        /// <param name="taskInfo">The task info</param>
        private void PerformMapping(IReadOnlyCollection<ObjectMap> maps, TaskInfo taskInfo = null)
        {
            var i = 1;
            var count = maps.Count;

            // Iterate the linearized db
            foreach (var m in maps)
            {
                if (taskInfo != null)
                {
                    taskInfo.Message = m.ObjectTitle;
                }

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
                    }
                }

                if (taskInfo != null)
                {
                    taskInfo.CancellationToken.ThrowIfCancellationRequested();
                    taskInfo.Percentage = (short)(100 * i++ / count);
                }
            }
        }

        /// <summary>
        /// Perform the mapping of the table
        /// </summary>
        /// <param name="sourceTable">The source table</param>
        private void PerformTableMapping(ABaseDbTable sourceTable)
        {
            var targetTable = sourceTable.MappedDbObject as ABaseDbTable;
            if (targetTable == null)
            {
                throw new ArgumentException($"{nameof(sourceTable.MappedDbObject)} is null");
            }

            // Linearize the 2 tables for mapping
            var maps = new List<ObjectMap>
            {
                new ObjectMap { DbObjects = sourceTable.Columns, MappableDbObjects = targetTable.Columns },
                new ObjectMap { DbObjects = sourceTable.Indexes, MappableDbObjects = targetTable.Indexes },
                new ObjectMap { DbObjects = sourceTable.ForeignKeys, MappableDbObjects = targetTable.ForeignKeys },
                new ObjectMap { DbObjects = sourceTable.Constraints, MappableDbObjects = targetTable.Constraints },
                new ObjectMap { DbObjects = sourceTable.Triggers, MappableDbObjects = targetTable.Triggers },
                new ObjectMap { DbObjects = sourceTable.PrimaryKeys, MappableDbObjects = targetTable.PrimaryKeys },
            };

            this.PerformMapping(maps);
        }

        /// <summary>
        /// Perform the mapping of the view
        /// </summary>
        /// <param name="sourceView">The source view</param>
        private void PerformViewMapping(ABaseDbView sourceView)
        {
            var targetView = sourceView.MappedDbObject as ABaseDbView;
            if (targetView == null)
            {
                throw new ArgumentException($"{nameof(sourceView.MappedDbObject)} is null");
            }

            // Linearize the 2 views for mapping
            var maps = new List<ObjectMap>
            {
                new ObjectMap { DbObjects = sourceView.Indexes, MappableDbObjects = targetView.Indexes },
            };

            this.PerformMapping(maps);
        }
    }
}
