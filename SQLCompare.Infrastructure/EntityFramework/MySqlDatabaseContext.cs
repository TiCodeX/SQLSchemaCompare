using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.Infrastructure.EntityFramework
{
    /// <summary>
    /// Defines the MySql database context
    /// </summary>
    internal class MySqlDatabaseContext : ADatabaseContext<MySqlDatabaseProviderOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlDatabaseContext"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="cipherService">The injected cipher service</param>
        /// <param name="dbpo">The MySql database provider options</param>
        public MySqlDatabaseContext(ILoggerFactory loggerFactory, ICipherService cipherService, MySqlDatabaseProviderOptions dbpo)
            : base(loggerFactory, loggerFactory.CreateLogger(nameof(MySqlDatabaseContext)), cipherService, dbpo)
        {
        }

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var connectionString = this.ConnectionString;
            if (this.DatabaseProviderOptions.UseSSL)
            {
                connectionString += "SslMode=Required;";
            }
            else
            {
                connectionString += "SslMode=None;";
            }

            optionsBuilder.UseMySQL(connectionString);
        }
    }
}
