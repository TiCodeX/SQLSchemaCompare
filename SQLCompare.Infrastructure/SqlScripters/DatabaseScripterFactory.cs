using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
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
        public IDatabaseScripter Create(ABaseDb database)
        {
            if (database is MicrosoftSqlDb)
            {
                return new MicrosoftSqlScripter(this.loggerFactory.CreateLogger("MicrosoftSqlScripter"), null);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
