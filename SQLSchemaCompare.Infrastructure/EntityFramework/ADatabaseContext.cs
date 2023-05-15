namespace TiCodeX.SQLSchemaCompare.Infrastructure.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Exceptions;

    /// <summary>
    /// Common EF database context
    /// </summary>
    /// <typeparam name="TDatabaseProviderOptions">The database provider options type</typeparam>
    public abstract class ADatabaseContext<TDatabaseProviderOptions> : DbContext
        where TDatabaseProviderOptions : ADatabaseProviderOptions
    {
        /// <summary>
        /// The logger factory
        /// </summary>
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ADatabaseContext{TDatabaseProviderOptions}"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="dbpo">The database provider options</param>
        protected ADatabaseContext(ILoggerFactory loggerFactory, TDatabaseProviderOptions dbpo)
        {
            if (dbpo == null)
            {
                throw new ArgumentNullException(nameof(dbpo));
            }

            this.loggerFactory = loggerFactory;
            this.Hostname = dbpo.Hostname;
            this.DatabaseName = dbpo.Database;
        }

        /// <summary>
        /// Gets the hostname
        /// </summary>
        public string Hostname { get; }

        /// <summary>
        /// Gets the database name
        /// </summary>
        public string DatabaseName { get; }

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
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                command.CommandText = query;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
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
                                else if (prop.PropertyType == typeof(long) && (value is decimal))
                                {
                                    prop.SetValue(t, decimal.ToInt64((decimal)value));
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
        /// <typeparam name="T">The type of the result</typeparam>
        /// <param name="query">The SQL query</param>
        /// <param name="columnIndex">The desired column</param>
        /// <returns>The list of the requested column</returns>
        public List<T> QuerySingleColumn<T>(string query, int columnIndex = 0)
        {
            var result = new List<T>();
            this.Database.OpenConnection();
            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                command.CommandText = query;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            var value = reader.GetValue(columnIndex);
                            result.Add(value is DBNull ? default(T) : (T)value);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            result.Add(default(T));
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Performs a non query command
        /// </summary>
        /// <param name="query">The SQL query</param>
        public void ExecuteNonQuery(string query)
        {
            this.Database.OpenConnection();
            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                command.CommandText = query;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                command.ExecuteNonQuery();
            }
        }

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder == null)
            {
                throw new ArgumentNullException(nameof(optionsBuilder));
            }

            optionsBuilder.UseLoggerFactory(this.loggerFactory);
        }
    }
}
