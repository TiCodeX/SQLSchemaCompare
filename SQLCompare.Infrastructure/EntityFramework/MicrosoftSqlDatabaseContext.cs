using Microsoft.EntityFrameworkCore;
using SQLCompare.Core.Entities.DatabaseProvider;

namespace SQLCompare.Infrastructure.EntityFramework
{
    /// <summary>
    /// Defines the MicrosoftSql database context
    /// </summary>
    internal class MicrosoftSqlDatabaseContext : GenericDatabaseContext<MicrosoftSqlDatabaseProviderOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlDatabaseContext"/> class.
        /// </summary>
        /// <param name="dbpo">The MicrosoftSql database provider options</param>
        public MicrosoftSqlDatabaseContext(MicrosoftSqlDatabaseProviderOptions dbpo)
            : base(dbpo)
        {
        }

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = this.ConnectionString;
            if (this.DatabaseProviderOptions.UseWindowsAuthentication)
            {
                connectionString = $"Server={this.DatabaseProviderOptions.Hostname};Database={this.DatabaseProviderOptions.Database};Integrated Security=SSPI";
            }

            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
