using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Infrastructure.DatabaseProviders;
using SQLCompare.Infrastructure.EntityFramework;

namespace SQLCompare.Test.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Creates the sakila/pagila databases for the tests
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class DatabaseFixture : IDisposable
    {
        private ILoggerFactory loggerFactory = new XunitLoggerFactory(null);

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseFixture"/> class
        /// </summary>
        public DatabaseFixture()
        {
            // MicrosoftSQL
            using (var context = new MicrosoftSqlDatabaseContext(this.loggerFactory, this.GetMicrosoftSqlDatabaseProviderOptions(false)))
            {
                var dropDbQuery = new StringBuilder();
                dropDbQuery.AppendLine("IF EXISTS(select * from sys.databases where name= 'sakila')");
                dropDbQuery.AppendLine("BEGIN");
                dropDbQuery.AppendLine("  ALTER DATABASE sakila SET SINGLE_USER WITH ROLLBACK IMMEDIATE"); // Close existing connections
                dropDbQuery.AppendLine("  DROP DATABASE sakila");
                dropDbQuery.AppendLine("END");
                context.Query(dropDbQuery.ToString());

                var path = Path.Combine(Directory.GetCurrentDirectory(), "Datasources\\sakila-schema-microsoftsql.sql");
                var queries = File.ReadAllText(path).Split(new[] { "GO" + Environment.NewLine }, StringSplitOptions.None);
                foreach (var query in queries)
                {
                    context.Query(query);
                }
            }

            // MySQL
            var pathMySql = Path.Combine(Directory.GetCurrentDirectory(), "Datasources\\sakila-schema-mysql.sql");
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "C:\\Program Files\\MySQL\\MySQL Server 8.0\\bin\\mysql.exe",
                Arguments = $"--user root -ptest1234 -e \"SOURCE {pathMySql}\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            });
            process.Should().NotBeNull();
            process.WaitForExit((int)TimeSpan.FromMinutes(5).TotalMilliseconds);
            process.ExitCode.Should().Be(0);

            // PostgreSQL
            using (var context = new PostgreSqlDatabaseContext(this.loggerFactory, this.GetPostgreSqlDatabaseProviderOptions(false)))
            {
                var dropDbQuery = new StringBuilder();
                dropDbQuery.AppendLine("select pg_terminate_backend(pid) from pg_stat_activity where datname='sakila';");
                dropDbQuery.AppendLine("DROP DATABASE IF EXISTS sakila;");
                dropDbQuery.AppendLine("CREATE DATABASE sakila;");
                context.Query(dropDbQuery.ToString());
            }

            using (var context = new PostgreSqlDatabaseContext(this.loggerFactory, this.GetPostgreSqlDatabaseProviderOptions(true)))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Datasources\\pagila-schema-postgresql.sql");
                context.Query(File.ReadAllText(path));
            }
        }

        /// <summary>
        /// Sets the test output helper
        /// </summary>
        /// <param name="loggerFactory">The test output helper</param>
        public void SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.loggerFactory.Dispose();
        }

        /// <summary>
        /// Gets the Microsoft SQL database provider
        /// </summary>
        /// <param name="connectToDatabase">if set to <c>true</c> it will connect directly to the test database</param>
        /// <returns>The Microsoft SQL database provider</returns>
        internal MicrosoftSqlDatabaseProvider GetMicrosoftSqlDatabaseProvider(bool connectToDatabase = true)
        {
            var dpf = new DatabaseProviderFactory(this.loggerFactory);
            return (MicrosoftSqlDatabaseProvider)dpf.Create(this.GetMicrosoftSqlDatabaseProviderOptions(connectToDatabase));
        }

        /// <summary>
        /// Gets the MySQL database provider
        /// </summary>
        /// <param name="connectToDatabase">if set to <c>true</c> it will connect directly to the test database</param>
        /// <returns>The MySQL SQL database provider</returns>
        internal MySqlDatabaseProvider GetMySqlDatabaseProvider(bool connectToDatabase = true)
        {
            var dpf = new DatabaseProviderFactory(this.loggerFactory);
            return (MySqlDatabaseProvider)dpf.Create(this.GetMySqlDatabaseProviderOptions(connectToDatabase));
        }

        /// <summary>
        /// Gets the PostgreSQL database provider
        /// </summary>
        /// <param name="connectToDatabase">if set to <c>true</c> it will connect directly to the test database</param>
        /// <returns>The PostgreSQL SQL database provider</returns>
        internal PostgreSqlDatabaseProvider GetPostgreDatabaseProvider(bool connectToDatabase = true)
        {
            var dpf = new DatabaseProviderFactory(this.loggerFactory);
            return (PostgreSqlDatabaseProvider)dpf.Create(this.GetPostgreSqlDatabaseProviderOptions(connectToDatabase));
        }

        private MicrosoftSqlDatabaseProviderOptions GetMicrosoftSqlDatabaseProviderOptions(bool connectToDatabase = true)
        {
            return new MicrosoftSqlDatabaseProviderOptions
            {
                Hostname = "localhost\\SQLEXPRESS",
                Database = connectToDatabase ? "sakila" : string.Empty,
                UseWindowsAuthentication = true,
            };
        }

        private MySqlDatabaseProviderOptions GetMySqlDatabaseProviderOptions(bool connectToDatabase = true)
        {
            return new MySqlDatabaseProviderOptions
            {
                Hostname = "localhost",
                Database = connectToDatabase ? "sakila" : string.Empty,
                Username = "root",
                Password = "test1234",
                UseSSL = Environment.MachineName != "DESKTOP-VH0A18B", // debe's MySql Server doesn't support SSL
            };
        }

        private PostgreSqlDatabaseProviderOptions GetPostgreSqlDatabaseProviderOptions(bool connectToDatabase = true)
        {
            return new PostgreSqlDatabaseProviderOptions
            {
                Hostname = "localhost",
                Database = connectToDatabase ? "sakila" : string.Empty,
                Username = "postgres",
                Password = "test1234",
            };
        }
    }
}
