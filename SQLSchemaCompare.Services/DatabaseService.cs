using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiCodeX.SQLSchemaCompare.Core.Entities;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;
using TiCodeX.SQLSchemaCompare.Core.Interfaces;
using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;

namespace TiCodeX.SQLSchemaCompare.Services
{
    /// <summary>
    /// Implementation that provides the mechanisms to read information from a database
    /// </summary>
    public class DatabaseService : IDatabaseService
    {
        private readonly IDatabaseProviderFactory dbProviderFactory;
        private readonly IAccountService accountService;
        private readonly IAppSettingsService appSettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseService"/> class.
        /// </summary>
        /// <param name="dbProviderFactory">The injected database provider factory</param>
        /// <param name="accountService">The account service</param>
        /// <param name="appSettingsService">The injected app settings service</param>
        public DatabaseService(IDatabaseProviderFactory dbProviderFactory, IAccountService accountService, IAppSettingsService appSettingsService)
        {
            this.dbProviderFactory = dbProviderFactory;
            this.accountService = accountService;
            this.appSettingsService = appSettingsService;
        }

        /// <inheritdoc />
        public List<string> ListDatabases(ADatabaseProviderOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Remove the database since we want to retrieve all of them
            options.Database = string.Empty;
            var provider = this.dbProviderFactory.Create(options);
            return provider.GetDatabaseList().OrderBy(x => x).ToList();
        }

        /// <inheritdoc />
        public ABaseDb GetDatabase(ADatabaseProviderOptions options, TaskInfo taskInfo)
        {
            var provider = this.dbProviderFactory.Create(options);
            try
            {
                return provider.GetDatabase(taskInfo);
            }
            catch (Exception ex)
            {
                try
                {
                    var exceptions = ex is AggregateException aggEx ? aggEx.InnerExceptions.ToArray() : new Exception[] { ex };

                    var sb = new StringBuilder();

                    var serverType = string.Empty;
                    switch (options)
                    {
                        case MicrosoftSqlDatabaseProviderOptions _:
                            serverType = "MicrosoftSQL";
                            break;
                        case MySqlDatabaseProviderOptions _:
                            serverType = "MySQL";
                            break;
                        case PostgreSqlDatabaseProviderOptions _:
                            serverType = "PostgreSQL";
                            break;
                        case MariaDbDatabaseProviderOptions _:
                            serverType = "MariaDB";
                            break;
                    }

                    sb.AppendLine($"Error retrieving database on {serverType} ({provider.CurrentServerVersion})");

                    foreach (var exception in exceptions)
                    {
                        if (sb.Length > 0)
                        {
                            sb.AppendLine("********************");
                        }

                        sb.AppendLine(exception.Message);
                        sb.AppendLine(exception.StackTrace);
                    }

                    var session = string.Empty;
                    try
                    {
                        session = this.appSettingsService.GetAppSettings().Session;
                    }
                    catch
                    {
                        // Do nothing
                    }

                    this.accountService.SendFeedback(session, -1, sb.ToString());
                }
                catch
                {
                    // Do nothing
                }

                throw;
            }
        }
    }
}
