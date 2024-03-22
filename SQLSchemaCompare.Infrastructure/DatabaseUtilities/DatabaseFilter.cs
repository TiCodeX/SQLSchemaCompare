namespace TiCodeX.SQLSchemaCompare.Infrastructure.DatabaseUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Database.PostgreSql;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Project;
    using TiCodeX.SQLSchemaCompare.Core.Enums;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces;

    /// <summary>
    /// Implements the database filtering functionality
    /// </summary>
    public class DatabaseFilter : IDatabaseFilter
    {
        /// <inheritdoc />
        public void PerformFilter(ABaseDb database, FilteringOptions filteringOptions)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            if (filteringOptions == null)
            {
                throw new ArgumentNullException(nameof(filteringOptions));
            }

            if (!filteringOptions.Clauses.Any())
            {
                return;
            }

            /* Schemas are a special case because the schema name is in the Name property itself, so we can temporarily
             * set the Schema property as the Name so that the filters can match, then remove it again
             */
            database.Schemas.ForEach(x =>
            {
                x.Schema = x.Name;
                x.Name = null;
            });
            database.Schemas.RemoveAll(x => !FilterIncludeObject(x, filteringOptions));
            database.Schemas.ForEach(x =>
            {
                x.Name = x.Schema;
                x.Schema = null;
            });

            // Create an HashSet in order to avoid checking if the item is already present when adding it
            var tablesToRemove = new HashSet<ABaseDbTable>(database.Tables.Where(x => !FilterIncludeObject(x, filteringOptions)));

            // Special case for PostgreSQL which has table inheritance, if a table inherits from one removed it should also be removed
            if (database is PostgreSqlDb)
            {
                foreach (var tableWithInheritance in database.Tables.Cast<PostgreSqlTable>().Where(x => !string.IsNullOrWhiteSpace(x.InheritedTableName)))
                {
                    if (tablesToRemove.FirstOrDefault(x => x.Schema == tableWithInheritance.InheritedTableSchema && x.Name == tableWithInheritance.InheritedTableName) != null)
                    {
                        tablesToRemove.Add(tableWithInheritance);
                    }
                }
            }

            // Remove from the global lists the objects related to the removed tables
            foreach (var table in tablesToRemove)
            {
                table.ForeignKeys.ForEach(x => database.ForeignKeys.Remove(x));
                table.PrimaryKeys.ForEach(x => database.PrimaryKeys.Remove(x));
                table.Indexes.ForEach(x => database.Indexes.Remove(x));
                table.Constraints.ForEach(x => database.Constraints.Remove(x));
                table.Triggers.ForEach(x => database.Triggers.Remove(x));
                database.Tables.Remove(table);
            }

            // Loop remaining table to apply the filter to their child objects
            foreach (var table in database.Tables)
            {
                table.ForeignKeys.RemoveAll(x => !FilterIncludeObject(x, filteringOptions));
                table.ReferencingForeignKeys.RemoveAll(x => !FilterIncludeObject(x, filteringOptions));
                table.PrimaryKeys.RemoveAll(x => !FilterIncludeObject(x, filteringOptions));
                table.Indexes.RemoveAll(x => !FilterIncludeObject(x, filteringOptions));
                table.Constraints.RemoveAll(x => !FilterIncludeObject(x, filteringOptions));
                table.Triggers.RemoveAll(x => !FilterIncludeObject(x, filteringOptions));
            }

            // Apply again the filter to the table child objects but on the global lists this time
            database.ForeignKeys.RemoveAll(x => !FilterIncludeObject(x, filteringOptions));
            database.PrimaryKeys.RemoveAll(x => !FilterIncludeObject(x, filteringOptions));
            database.Indexes.RemoveAll(x => !FilterIncludeObject(x, filteringOptions));
            database.Constraints.RemoveAll(x => !FilterIncludeObject(x, filteringOptions));
            database.Triggers.RemoveAll(x => !FilterIncludeObject(x, filteringOptions));

            var viewsToRemove = database.Views.Where(x => !FilterIncludeObject(x, filteringOptions)).ToList();

            // Remove from the global lists the objects related to the removed views
            foreach (var view in viewsToRemove)
            {
                view.Indexes.ForEach(x => database.Indexes.Remove(x));
                database.Views.Remove(view);
            }

            foreach (var view in database.Views)
            {
                view.Indexes.RemoveAll(x => !FilterIncludeObject(x, filteringOptions));
            }

            database.Functions.RemoveAll(x => !FilterIncludeObject(x, filteringOptions));
            database.StoredProcedures.RemoveAll(x => !FilterIncludeObject(x, filteringOptions));
            database.DataTypes.RemoveAll(x => x.IsUserDefined && !FilterIncludeObject(x, filteringOptions));
            database.Sequences.RemoveAll(x => !FilterIncludeObject(x, filteringOptions));
        }

        /// <summary>
        /// Evaluate the filter clauses to check whether the object has to be included
        /// </summary>
        /// <param name="dbObject">The database object</param>
        /// <param name="filteringOptions">The filtering options</param>
        /// <returns>Whether to include the object</returns>
        [SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "TODO")]
        private static bool FilterIncludeObject(ABaseDbObject dbObject, FilteringOptions filteringOptions)
        {
            var filteredClauses = filteringOptions.Clauses.Where(x => x.ObjectType == null || x.ObjectType == dbObject.ObjectType).ToList();
            if (!filteredClauses.Any())
            {
                return true;
            }

            bool? includeObject = null;
            foreach (var filterClauseGroup in filteredClauses.GroupBy(x => x.Group))
            {
                bool? includeObjectGroup = null;
                foreach (var filterClause in filterClauseGroup)
                {
                    var objectFieldToCheck = (filterClause.Field == FilterField.Schema ? dbObject.Schema : dbObject.Name) ?? string.Empty;

                    bool objectMatch;
                    switch (filterClause.Operator)
                    {
                        case FilterOperator.BeginsWith:
                            objectMatch = objectFieldToCheck.StartsWith(filterClause.Value, StringComparison.Ordinal);
                            break;
                        case FilterOperator.EndsWith:
                            objectMatch = objectFieldToCheck.EndsWith(filterClause.Value, StringComparison.Ordinal);
                            break;
                        case FilterOperator.Contains:
                            objectMatch = objectFieldToCheck.Contains(filterClause.Value, StringComparison.Ordinal);
                            break;
                        case FilterOperator.Equals:
                            objectMatch = objectFieldToCheck.Equals(filterClause.Value, StringComparison.Ordinal);
                            break;
                        case FilterOperator.NotBeginsWith:
                            objectMatch = !objectFieldToCheck.StartsWith(filterClause.Value, StringComparison.Ordinal);
                            break;
                        case FilterOperator.NotEndsWith:
                            objectMatch = !objectFieldToCheck.EndsWith(filterClause.Value, StringComparison.Ordinal);
                            break;
                        case FilterOperator.NotContains:
                            objectMatch = !objectFieldToCheck.Contains(filterClause.Value, StringComparison.Ordinal);
                            break;
                        case FilterOperator.NotEquals:
                            objectMatch = !objectFieldToCheck.Equals(filterClause.Value, StringComparison.Ordinal);
                            break;
                        default:
                            throw new NotSupportedException("Unknown operator");
                    }

                    // If it's the first match inside the group, take the actual value, otherwise perform an AND
                    if (includeObjectGroup == null)
                    {
                        includeObjectGroup = objectMatch;
                    }
                    else
                    {
                        includeObjectGroup &= objectMatch;
                    }
                }

                // If it's the first group, take the actual value, otherwise perform an OR
                if (includeObject == null)
                {
                    includeObject = includeObjectGroup;
                }
                else
                {
                    includeObject |= includeObjectGroup;
                }
            }

            // Should never happen because at this point there is at least one filter clause
            if (includeObject == null)
            {
                throw new NotSupportedException();
            }

            return filteringOptions.Include ? includeObject.Value : !includeObject.Value;
        }
    }
}
