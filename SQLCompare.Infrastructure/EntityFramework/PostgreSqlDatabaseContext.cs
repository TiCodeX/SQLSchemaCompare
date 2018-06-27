using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.DatabaseProvider;

namespace SQLCompare.Infrastructure.EntityFramework
{
    /// <summary>
    /// Defines the PostgresSql database context
    /// </summary>
    internal class PostgreSqlDatabaseContext : ADatabaseContext<PostgreSqlDatabaseProviderOptions>
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

            var connectionString = this.ConnectionString;
            if (this.DatabaseProviderOptions.UseSSL)
            {
                connectionString += "SSL Mode=Require;";
            }

            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}
