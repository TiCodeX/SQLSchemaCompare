using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Entities.EntityFramework;
using System;
using System.Collections.Generic;

namespace SQLCompare.Infrastructure.EntityFramework
{
    /// <inheritdoc />
    /// <summary>
    /// Common EF database context
    /// </summary>
    internal abstract class ADatabaseContext<TDatabaseProviderOptions> : DbContext
        where TDatabaseProviderOptions : DatabaseProviderOptions
    {
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ADatabaseContext{TDatabaseProviderOptions}"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="dbpo">The database provider options</param>
        protected ADatabaseContext(ILoggerFactory loggerFactory, TDatabaseProviderOptions dbpo)
        {
            this.loggerFactory = loggerFactory;
            this.DatabaseProviderOptions = dbpo;
            this.ConnectionString = $"Server={dbpo.Hostname};Database={dbpo.Database};User Id={dbpo.Username};Password={dbpo.Password}";
        }

        /// <summary>
        /// Gets or sets the tables of the InformationSchema
        /// </summary>
        public virtual DbSet<InformationSchemaTable> Tables { get; protected set; }

        /// <summary>
        /// Gets or sets the columns of the InformationSchema
        /// </summary>
        public virtual DbSet<InformationSchemaColumn> Columns { get; protected set; }

        /// <summary>
        /// Gets the database provider options
        /// </summary>
        protected TDatabaseProviderOptions DatabaseProviderOptions { get; }

        /// <summary>
        /// Gets the string used for the connection
        /// </summary>
        protected string ConnectionString { get; }

        /// <summary>
        /// Performs a query
        /// </summary>
        /// <typeparam name="T">The type of the result</typeparam>
        /// <param name="query">The SQL query</param>
        /// <returns>The list of specified type</returns>
        public List<T> Query<T>(string query)
            where T : new()
        {
            var result = new List<T>();
            this.Database.OpenConnection();
            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var t = new T();
                        var type = t.GetType();
                        for (var inc = 0; inc < reader.FieldCount; inc++)
                        {
                            var prop = type.GetProperty(reader.GetName(inc));
                            var value = reader.GetValue(inc);
                            prop.SetValue(t, value is DBNull ? null : value, null);
                        }

                        result.Add(t);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Performs a query
        /// </summary>
        /// <param name="query">The SQL query</param>
        /// <returns>The list of the first column</returns>
        public List<string> Query(string query)
        {
            var result = new List<string>();
            this.Database.OpenConnection();
            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(this.loggerFactory);
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var table = modelBuilder.Entity<InformationSchemaTable>();
            table.ToTable("tables", "information_schema");
            table.HasKey(a => new { a.TableName, a.TableCatalog, a.TableSchema });
            table.HasQueryFilter(x => string.Equals(x.TableType, "BASE TABLE", System.StringComparison.Ordinal));

            var column = modelBuilder.Entity<InformationSchemaColumn>();
            column.ToTable("columns", "information_schema");
            column.HasKey(a => new { a.TableName, a.TableCatalog, a.TableSchema, a.ColumnName });

            // Create table/column relationship
            table.HasMany(x => x.Columns).WithOne(x => x.Table).HasForeignKey(a => new { a.TableName, a.TableCatalog, a.TableSchema });
        }
    }
}
