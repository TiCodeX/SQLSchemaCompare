using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Core.Interfaces.Services;
using SQLCompare.Infrastructure.DatabaseProviders;
using SQLCompare.Infrastructure.EntityFramework;
using SQLCompare.Services;

namespace SQLCompare.Test
{
    /// <summary>
    /// Creates the sakila/pagila databases for the tests
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class DatabaseFixture : IDisposable
    {
        private readonly ICipherService cipherService = new CipherService();
        private ILoggerFactory loggerFactory = new XunitLoggerFactory(null);

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseFixture"/> class
        /// </summary>
        public DatabaseFixture()
        {
            // MicrosoftSQL
            this.CreateMicrosoftSqlSakilaDatabase("sakila");

            // MySQL
            this.ExecuteMySqlScript(Path.Combine(Directory.GetCurrentDirectory(), "Datasources\\sakila-schema-mysql.sql"));

            // PostgreSQL
            this.DropAndCreatePostgreSqlDatabase("sakila");
            using (var context = new PostgreSqlDatabaseContext(this.loggerFactory, this.cipherService, this.GetPostgreSqlDatabaseProviderOptions("sakila")))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Datasources\\sakila-schema-postgresql.sql");
                context.ExecuteNonQuery(File.ReadAllText(path));
            }
        }

        /// <summary>
        /// Sets the test output helper
        /// </summary>
        /// <param name="logFactory">The test output helper</param>
        public void SetLoggerFactory(ILoggerFactory logFactory)
        {
            this.loggerFactory = logFactory;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.loggerFactory.Dispose();
        }

        /// <summary>
        /// Executes my SQL script
        /// </summary>
        /// <param name="path">The path</param>
        internal void ExecuteMySqlScript(string path)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "C:\\Program Files\\MySQL\\MySQL Server 8.0\\bin\\mysql.exe",
                Arguments = $"--user root -ptest1234 -e \"SOURCE {path}\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            });
            process.Should().NotBeNull();
            process.WaitForExit((int)TimeSpan.FromMinutes(5).TotalMilliseconds);
            process.ExitCode.Should().Be(0);
        }

        /// <summary>
        /// Drops the and create microsoft database
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        internal void DropAndCreateMicrosoftSqlDatabase(string databaseName)
        {
            using (var context = new MicrosoftSqlDatabaseContext(this.loggerFactory, this.cipherService, this.GetMicrosoftSqlDatabaseProviderOptions(string.Empty)))
            {
                var dropDbQuery = new StringBuilder();
                dropDbQuery.AppendLine($"IF EXISTS(select * from sys.databases where name= '{databaseName}')");
                dropDbQuery.AppendLine("BEGIN");
                dropDbQuery.AppendLine($"  ALTER DATABASE {databaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE"); // Close existing connections
                dropDbQuery.AppendLine($"  DROP DATABASE {databaseName}");
                dropDbQuery.AppendLine("END");
                context.ExecuteNonQuery(dropDbQuery.ToString());

                context.ExecuteNonQuery($"CREATE DATABASE {databaseName}");
            }
        }

        /// <summary>
        /// Drops the and create postgresql database
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        internal void DropAndCreatePostgreSqlDatabase(string databaseName)
        {
            using (var context = new PostgreSqlDatabaseContext(this.loggerFactory, this.cipherService, this.GetPostgreSqlDatabaseProviderOptions(string.Empty)))
            {
                var dropDbQuery = new StringBuilder();
                dropDbQuery.AppendLine($"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='{databaseName}';");
                dropDbQuery.AppendLine($"DROP DATABASE IF EXISTS {databaseName};");
                dropDbQuery.AppendLine($"CREATE DATABASE {databaseName};");
                context.ExecuteNonQuery(dropDbQuery.ToString());
            }
        }

        /// <summary>
        /// Creates the sakila database
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        internal void CreateMicrosoftSqlSakilaDatabase(string databaseName)
        {
            this.DropAndCreateMicrosoftSqlDatabase(databaseName);

            using (var context = new MicrosoftSqlDatabaseContext(this.loggerFactory, this.cipherService, this.GetMicrosoftSqlDatabaseProviderOptions(databaseName)))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Datasources\\sakila-schema-microsoftsql.sql");
                var queries = File.ReadAllText(path).Split(new[] { "GO" + Environment.NewLine }, StringSplitOptions.None);
                foreach (var query in queries.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    context.ExecuteNonQuery(query);
                }
            }
        }

        /// <summary>
        /// Gets the Microsoft SQL database provider
        /// </summary>
        /// <param name="databaseName">The database name to connect</param>
        /// <returns>The Microsoft SQL database provider</returns>
        internal MicrosoftSqlDatabaseProvider GetMicrosoftSqlDatabaseProvider(string databaseName = "")
        {
            var dpf = new DatabaseProviderFactory(this.loggerFactory, this.cipherService);
            return (MicrosoftSqlDatabaseProvider)dpf.Create(this.GetMicrosoftSqlDatabaseProviderOptions(databaseName));
        }

        /// <summary>
        /// Gets the MySQL database provider
        /// </summary>
        /// <param name="databaseName">The database name to connect</param>
        /// <returns>The MySQL SQL database provider</returns>
        internal MySqlDatabaseProvider GetMySqlDatabaseProvider(string databaseName = "")
        {
            var dpf = new DatabaseProviderFactory(this.loggerFactory, this.cipherService);
            return (MySqlDatabaseProvider)dpf.Create(this.GetMySqlDatabaseProviderOptions(databaseName));
        }

        /// <summary>
        /// Gets the PostgreSQL database provider
        /// </summary>
        /// <param name="databaseName">The database name to connect</param>
        /// <returns>The PostgreSQL SQL database provider</returns>
        internal PostgreSqlDatabaseProvider GetPostgreSqlDatabaseProvider(string databaseName = "")
        {
            var dpf = new DatabaseProviderFactory(this.loggerFactory, this.cipherService);
            return (PostgreSqlDatabaseProvider)dpf.Create(this.GetPostgreSqlDatabaseProviderOptions(databaseName));
        }

        /// <summary>
        /// Gets the Microsoft SQL database provider options
        /// </summary>
        /// <param name="databaseName">The database name to connect</param>
        /// <returns>The Microsoft SQL database provider options</returns>
        internal MicrosoftSqlDatabaseProviderOptions GetMicrosoftSqlDatabaseProviderOptions(string databaseName)
        {
            return new MicrosoftSqlDatabaseProviderOptions
            {
                Hostname = "localhost\\SQLEXPRESS",
                Database = databaseName,
                UseWindowsAuthentication = true,
            };
        }

        /// <summary>
        /// Gets the MySQL database provider options
        /// </summary>
        /// <param name="databaseName">The database name to connect</param>
        /// <returns>The MySQL database provider options</returns>
        internal MySqlDatabaseProviderOptions GetMySqlDatabaseProviderOptions(string databaseName)
        {
            return new MySqlDatabaseProviderOptions
            {
                Hostname = "localhost",
                Database = databaseName,
                Username = "root",
                Password = this.cipherService.EncryptString("test1234"),
                UseSSL = Environment.MachineName != "DESKTOP-VH0A18B", // debe's MySql Server doesn't support SSL
            };
        }

        /// <summary>
        /// Gets the PostgreSQL database provider options
        /// </summary>
        /// <param name="databaseName">The database name to connect</param>
        /// <returns>The PostgreSQL database provider options</returns>
        internal PostgreSqlDatabaseProviderOptions GetPostgreSqlDatabaseProviderOptions(string databaseName)
        {
            return new PostgreSqlDatabaseProviderOptions
            {
                Hostname = "localhost",
                Database = databaseName,
                Username = "postgres",
                Password = this.cipherService.EncryptString("test1234"),
            };
        }
    }
}
