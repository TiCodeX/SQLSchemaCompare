using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces;
using SQLCompare.Infrastructure.EntityFramework;
using System.Collections.Generic;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves various information from a Microsoft SQL Server
    /// </summary>
    internal class MicrosoftSqlDatabaseProvider : IDatabaseProvider
    {
        private readonly MicrosoftSqlDatabaseProviderOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlDatabaseProvider"/> class.
        /// </summary>
        /// <param name="options">The options to connect to the Microsoft SQL Database</param>
        public MicrosoftSqlDatabaseProvider(MicrosoftSqlDatabaseProviderOptions options)
        {
            this.options = options;
        }

        /// <inheritdoc/>
        public BaseDb GetDatabase()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public List<string> GetDatabaseList()
        {
            using (var context = new MicrosoftSqlDatabaseContext(this.options.Hostname, this.options.Database, this.options.Username, this.options.Password))
            {
                return context.Query("SELECT name FROM sysdatabases");
            }
        }
    }
}