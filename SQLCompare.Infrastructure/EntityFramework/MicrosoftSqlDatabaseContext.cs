using Microsoft.EntityFrameworkCore;

namespace SQLCompare.Infrastructure.EntityFramework
{
    /// <summary>
    /// Defines the MicrosoftSql database context
    /// </summary>
    internal class MicrosoftSqlDatabaseContext : GenericDatabaseContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlDatabaseContext"/> class.
        /// </summary>
        /// <param name="server">The database server</param>
        /// <param name="databaseName">The database instance</param>
        /// <param name="username">The username used for database connection</param>
        /// <param name="password">The password used for database connection</param>
        public MicrosoftSqlDatabaseContext(string server, string databaseName, string username, string password)
            : base(server, databaseName, username, password)
        {
        }

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(this.ConnectionString);
        }
    }
}
