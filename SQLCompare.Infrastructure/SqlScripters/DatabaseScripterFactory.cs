using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.MicrosoftSql;
using SQLCompare.Core.Entities.Database.MySql;
using SQLCompare.Core.Entities.Project;
using SQLCompare.Core.Interfaces;
using System;

namespace SQLCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Implementation class for the factory that create a Database scripter
    /// </summary>
    public class DatabaseScripterFactory : IDatabaseScripterFactory
    {
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseScripterFactory"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected loggerfactory</param>
        public DatabaseScripterFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        /// <inheritdoc/>
        public IDatabaseScripter Create(ABaseDb database, ProjectOptions options)
        {
            switch (database)
            {
                case MicrosoftSqlDb _:
                    return new MicrosoftSqlScripter(this.loggerFactory.CreateLogger("MicrosoftSqlScripter"), options);
                case MySqlDb _:
                    return new MySqlScripter(this.loggerFactory.CreateLogger("MySqlScripter"), options);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
