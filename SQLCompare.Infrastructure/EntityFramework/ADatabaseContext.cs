using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Entities.Exceptions;

namespace SQLCompare.Infrastructure.EntityFramework
{
    /// <inheritdoc />
    /// <summary>
    /// Common EF database context
    /// </summary>
    public abstract class ADatabaseContext<TDatabaseProviderOptions> : DbContext
        where TDatabaseProviderOptions : ADatabaseProviderOptions
    {
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ADatabaseContext{TDatabaseProviderOptions}"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="dbpo">The database provider options</param>
        protected ADatabaseContext(ILoggerFactory loggerFactory, TDatabaseProviderOptions dbpo)
        {
            this.loggerFactory = loggerFactory;
            this.DatabaseProviderOptions = dbpo;
            this.ConnectionString = $"Server={dbpo.Hostname};Database={dbpo.Database};User Id={dbpo.Username};Password={dbpo.Password};";
        }

        /// <summary>
        /// Gets the database provider options
        /// </summary>
        protected TDatabaseProviderOptions DatabaseProviderOptions { get; }

        /// <summary>
        /// Gets the string used for the connection
        /// </summary>
        protected string ConnectionString { get; }

        /// <summary>
        /// Performs a query
        /// </summary>
        /// <typeparam name="T">The type of the result</typeparam>
        /// <param name="query">The SQL query</param>
        /// <returns>The list of specified type</returns>
        public List<T> Query<T>(string query)
            where T : new()
        {
            var result = new List<T>();
            this.Database.OpenConnection();
            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var t = new T();
                        var type = t.GetType();
                        for (var inc = 0; inc < reader.FieldCount; inc++)
                        {
                            var columnName = reader.GetName(inc);
                            var prop = type.GetProperty(columnName);
                            var value = reader.GetValue(inc);
                            if (prop != null)
                            {
                                if (prop.PropertyType == typeof(bool) && !(value is bool))
                                {
                                    switch (value.ToString())
                                    {
                                        case "1":
                                            prop.SetValue(t, true, null);
                                            break;
                                        case "0":
                                            prop.SetValue(t, false, null);
                                            break;
                                        default:
                                            throw new WrongTypeException();
                                    }
                                }
                                else
                                {
                                    prop.SetValue(t, value is DBNull ? null : value, null);
                                }
                            }
                            else
                            {
                                throw new PropertyNotFoundException(type, columnName);
                            }
                        }

                        result.Add(t);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Performs a query
        /// </summary>
        /// <param name="query">The SQL query</param>
        /// <param name="columnIndex">The desired column</param>
        /// <returns>The list of the requested column</returns>
        public List<string> Query(string query, int columnIndex = 0)
        {
            var result = new List<string>();
            this.Database.OpenConnection();
            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(columnIndex));
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(this.loggerFactory);
        }
    }
}
