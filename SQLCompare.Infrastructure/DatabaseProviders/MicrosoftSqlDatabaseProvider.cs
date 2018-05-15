using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Infrastructure.EntityFramework;
using System.Collections.Generic;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves various information from a Microsoft SQL Server
    /// </summary>
    internal class MicrosoftSqlDatabaseProvider : GenericDatabaseProvider<MicrosoftSqlDb, MicrosoftSqlTable, MicrosoftSqlColumn, MicrosoftSqlDatabaseProviderOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlDatabaseProvider"/> class.
        /// </summary>
        /// <param name="options">The options to connect to the Microsoft SQL Database</param>
        public MicrosoftSqlDatabaseProvider(MicrosoftSqlDatabaseProviderOptions options)
            : base(options)
        {
        }

        /// <inheritdoc/>
        public override BaseDb GetDatabase()
        {
            using (var context = new MicrosoftSqlDatabaseContext(this.Options))
            {
                return GetCommonDatabase(context);
            }
        }

        /// <inheritdoc/>
        public override List<string> GetDatabaseList()
        {
            using (var context = new MicrosoftSqlDatabaseContext(this.Options))
            {
                return context.Query("SELECT name FROM sysdatabases");
            }
        }
    }
}