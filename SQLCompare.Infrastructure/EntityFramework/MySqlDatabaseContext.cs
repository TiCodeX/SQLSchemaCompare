using Microsoft.EntityFrameworkCore;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Entities.EntityFramework;

namespace SQLCompare.Infrastructure.EntityFramework
{
    /// <summary>
    /// Defines the MySql database context
    /// </summary>
    internal class MySqlDatabaseContext : GenericDatabaseContext<MySqlDatabaseProviderOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlDatabaseContext"/> class.
        /// </summary>
        /// <param name="dbpo">The MySql database provider options</param>
        public MySqlDatabaseContext(MySqlDatabaseProviderOptions dbpo)
            : base(dbpo)
        {
        }

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = this.ConnectionString;
            if (!this.DatabaseProviderOptions.UseSSL)
            {
                connectionString += ";SslMode=None";
            }

            optionsBuilder.UseMySQL(connectionString);
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var table = modelBuilder.Entity<InformationSchemaTable>();
            table.HasQueryFilter(x => string.Equals(x.TableType, "BASE TABLE", System.StringComparison.Ordinal) &&
                                      string.Equals(x.TableSchema, this.DatabaseProviderOptions.Database, System.StringComparison.Ordinal));
        }
    }
}
