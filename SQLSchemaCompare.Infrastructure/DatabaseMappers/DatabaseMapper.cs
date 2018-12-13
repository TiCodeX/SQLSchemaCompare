using System;
using System.Collections.Generic;
using System.Linq;
using TiCodeX.SQLSchemaCompare.Core.Entities;
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
                new ObjectMap { StatusMessage = Localization.StatusMappingTables, Source = source.Tables, Target = target.Tables },
                new ObjectMap { StatusMessage = Localization.StatusMappingViews, Source = source.Views, Target = target.Views },
                new ObjectMap { StatusMessage = Localization.StatusMappingFunctions, Source = source.Functions, Target = target.Functions },
                new ObjectMap { StatusMessage = Localization.StatusMappingStoredProcedures, Source = source.StoredProcedures, Target = target.StoredProcedures },
                new ObjectMap { StatusMessage = Localization.StatusMappingDataTypes, Source = source.DataTypes, Target = target.DataTypes },
                new ObjectMap { StatusMessage = Localization.StatusMappingSequences, Source = source.Sequences, Target = target.Sequences },
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
                    taskInfo.Message = m.StatusMessage;
                }

                foreach (var sourceObject in m.Source)
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
                    var targetObject = m.Target.FirstOrDefault(x => x.Schema == sourceObject.Schema && x.Name == sourceObject.Name);
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
                new ObjectMap { Source = sourceTable.Columns, Target = targetTable.Columns },
                new ObjectMap { Source = sourceTable.Indexes, Target = targetTable.Indexes },
                new ObjectMap { Source = sourceTable.ForeignKeys, Target = targetTable.ForeignKeys },
                new ObjectMap { Source = sourceTable.Constraints, Target = targetTable.Constraints },
                new ObjectMap { Source = sourceTable.Triggers, Target = targetTable.Triggers },
                new ObjectMap { Source = sourceTable.PrimaryKeys, Target = targetTable.PrimaryKeys },
            };

            this.PerformMapping(maps);
        }

        private class ObjectMap
        {
            public string StatusMessage { get; internal set; }

            public IEnumerable<ABaseDbObject> Source { get; internal set; }

            public IEnumerable<ABaseDbObject> Target { get; internal set; }
        }
    }
}
