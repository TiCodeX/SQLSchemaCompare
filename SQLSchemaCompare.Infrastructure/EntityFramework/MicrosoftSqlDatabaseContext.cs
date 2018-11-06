using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;
using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;

namespace TiCodeX.SQLSchemaCompare.Infrastructure.EntityFramework
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
            : base(loggerFactory, loggerFactory.CreateLogger(nameof(MicrosoftSqlDatabaseContext)), dbpo)
        {
            var server = dbpo.Hostname;
            if (!server.Contains("\\"))
            {
                server += $",{dbpo.Port}";
            }

            var connStr = $"Server={server};Database={dbpo.Database};";

            if (dbpo.UseWindowsAuthentication)
            {
                connStr += "Integrated Security=SSPI;";
            }
            else
            {
                connStr += $"User Id={dbpo.Username};Password={cipherService.DecryptString(dbpo.Password)};";
            }

            if (dbpo.UseSSL)
            {
                connStr += "Encrypt=true;";
            }

            connStr += "Connection Timeout=15;";
            connStr += "Persist Security Info=False;";

            this.ConnectionString = connStr;
        }

        /// <summary>
        /// Gets the string used for the connection
        /// </summary>
        private string ConnectionString { get; }

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(this.ConnectionString);
        }
    }
}
