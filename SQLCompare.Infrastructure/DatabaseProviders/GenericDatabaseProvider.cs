using Microsoft.EntityFrameworkCore;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces;
using SQLCompare.Infrastructure.EntityFramework;
using System.Collections.Generic;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves common information from a Server
    /// </summary>
    /// <typeparam name="TDatabase">Concrete type of the Database</typeparam>
    /// <typeparam name="TTable">Concrete type of the Table</typeparam>
    /// <typeparam name="TColumn">Concrete type of the Column</typeparam>
    /// <typeparam name="TDatabaseProviderOptions">Concrete type of the database provider options</typeparam>
    public abstract class GenericDatabaseProvider<TDatabase, TTable, TColumn, TDatabaseProviderOptions> : IDatabaseProvider
        where TDatabase : BaseDb, new()
        where TTable : BaseDbTable, new()
        where TColumn : BaseDbColumn, new()
        where TDatabaseProviderOptions : DatabaseProviderOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDatabaseProvider{TDatabase, TTable, TColumn, TDatabaseProviderOptions}"/> class.
        /// </summary>
        /// <param name="options">The options to connect to the Database</param>
        protected GenericDatabaseProvider(TDatabaseProviderOptions options)
        {
            this.Options = options;
        }

        /// <summary>
        /// Gets the options to connect to the Database
        /// </summary>
        protected TDatabaseProviderOptions Options { get; }

        /// <inheritdoc/>
        public abstract BaseDb GetDatabase();

        /// <inheritdoc/>
        public abstract List<string> GetDatabaseList();

        /// <summary>
        /// Gets the common database structure
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>The database structure</returns>
        internal static BaseDb GetCommonDatabase(GenericDatabaseContext<TDatabaseProviderOptions> context)
        {
            var db = new TDatabase();
            foreach (var t in context.Tables.Include(x => x.Columns))
            {
                var table = new TTable
                {
                    Name = t.TableName,
                };
                foreach (var c in t.Columns)
                {
                    table.Columns.Add(new TColumn
                    {
                        Name = c.ColumnName
                    });
                }

                db.Tables.Add(table);
            }

            return db;
        }
    }
}
