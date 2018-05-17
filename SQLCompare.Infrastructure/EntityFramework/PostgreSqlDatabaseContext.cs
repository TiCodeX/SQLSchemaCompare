using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Entities.EntityFramework;

namespace SQLCompare.Infrastructure.EntityFramework
{
    /// <summary>
    /// Defines the PostgresSql database context
    /// </summary>
    internal class PostgreSqlDatabaseContext : GenericDatabaseContext<PostgreSqlDatabaseProviderOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlDatabaseContext"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="dbpo">The PostgreSql database provider options</param>
        public PostgreSqlDatabaseContext(ILoggerFactory loggerFactory, PostgreSqlDatabaseProviderOptions dbpo)
            : base(loggerFactory, dbpo)
        {
        }

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseNpgsql(this.ConnectionString);
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var table = modelBuilder.Entity<InformationSchemaTable>();
            table.HasQueryFilter(x => string.Equals(x.TableType, "BASE TABLE", System.StringComparison.Ordinal) &&
                                      string.Equals(x.TableSchema, "public", System.StringComparison.Ordinal));
        }
    }
}
