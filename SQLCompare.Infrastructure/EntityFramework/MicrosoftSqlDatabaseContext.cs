using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.Infrastructure.EntityFramework
{
    /// <summary>
    /// Defines the MicrosoftSql database context
    /// </summary>
    internal class MicrosoftSqlDatabaseContext : ADatabaseContext<MicrosoftSqlDatabaseProviderOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlDatabaseContext"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="cipherService">The injected cipher service</param>
        /// <param name="dbpo">The MicrosoftSql database provider options</param>
        public MicrosoftSqlDatabaseContext(ILoggerFactory loggerFactory, ICipherService cipherService, MicrosoftSqlDatabaseProviderOptions dbpo)
            : base(loggerFactory, loggerFactory.CreateLogger(nameof(MicrosoftSqlDatabaseContext)), cipherService, dbpo)
        {
        }

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var connectionString = this.ConnectionString;
            if (this.DatabaseProviderOptions.UseWindowsAuthentication)
            {
                connectionString = $"Server={this.DatabaseProviderOptions.Hostname};Database={this.DatabaseProviderOptions.Database};Integrated Security=SSPI;";
            }

            if (this.DatabaseProviderOptions.UseSSL)
            {
                connectionString += "Encrypt=true;";
            }

            connectionString += "Connection Timeout=30;";
            connectionString += "Persist Security Info=False;";

            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
