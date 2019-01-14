using System;
using System.Collections.Generic;
using System.Linq;
using TiCodeX.SQLSchemaCompare.Core.Entities;
using TiCodeX.SQLSchemaCompare.Core.Entities.Compare;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
using TiCodeX.SQLSchemaCompare.Core.Interfaces;
using TiCodeX.SQLSchemaCompare.Services;

namespace TiCodeX.SQLSchemaCompare.Infrastructure.DatabaseMappers
{
    /// <summary>
    /// Implements the database mapper functionality
    /// </summary>
    public class DatabaseMapper : IDatabaseMapper
    {
        /// <inheritdoc/>
        public object PerformMapping(ABaseDb source, ABaseDb target, object mappingTable, TaskInfo taskInfo)
        {
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

            return null;
        }

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
                    // For the moment we much schema and name.
                    /*var foundTargets = m.Target.Where(x => x.Name == sourceObject.Name);
                    if (foundTargets.Any())
                    {
                        var targetObject = foundTargets.FirstOrDefault(x => x.Schema == sourceObject.Schema);

                        if (targetObject == null)
                        {
                            targetObject = foundTargets.First();
                        }

                        sourceObject.Schema = targetObject.Schema;
                        sourceObject.Name = targetObject.Name;

                        sourceObject.MappedDbObject = targetObject;
                        targetObject.MappedDbObject = sourceObject;

                        if (sourceObject is ABaseDbTable)
                        {
                            this.PerformTableMapping(sourceObject as ABaseDbTable);
                        }
                    }*/
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
                    }
                }

                if (taskInfo != null)
                {
                    taskInfo.CancellationToken.ThrowIfCancellationRequested();
                    taskInfo.Percentage = (short)((100 * i++) / count);
                }
            }
        }

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
    }
}
