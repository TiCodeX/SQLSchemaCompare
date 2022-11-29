using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;
using TiCodeX.SQLSchemaCompare.Infrastructure.EntityFramework;

namespace TiCodeX.SQLSchemaCompare.Test
{
    /// <summary>
    /// Creates the sakila database for the tests
    /// </summary>
    public class DatabaseFixturePostgreSql : DatabaseFixture
    {
        /// <summary>
        /// Gets the list of MicrosoftSQL server ports
        /// </summary>
        public static IEnumerable<object[]> ServerPorts
        {
            get
            {
                var serverPorts = new List<object[]>();

                if (Environment.GetEnvironmentVariable("RunDockerTests")?.ToUpperInvariant() == "TRUE" || DatabaseFixture.ForceDockerTests)
                {
                    /*serverPorts.Add(new object[] { (short)26001 });*/ // Version 9.3 (EOL November 2018)
                    /*serverPorts.Add(new object[] { (short)26002 });*/ // Version 9.4 (EOL February 2020)
                    /*serverPorts.Add(new object[] { (short)26003 });*/ // Version 9.5 (EOL February 2021)
                    /*serverPorts.Add(new object[] { (short)26004 });*/ // Version 9.6 (EOL November 2021)
                    /*serverPorts.Add(new object[] { (short)26005 });*/ // Version 10 (EOL November 2022)
                    serverPorts.Add(new object[] { (short)26006 }); // Version 11 (EOL November 2023)
                    serverPorts.Add(new object[] { (short)26007 }); // Version 12 (EOL November 2024)
                    serverPorts.Add(new object[] { (short)26008 }); // Version 13 (EOL November 2025)
                    serverPorts.Add(new object[] { (short)26009 }); // Version 14 (EOL November 2026)
                    serverPorts.Add(new object[] { (short)26010 }); // Version 15 (EOL November 2027)
                }
                else
                {
                    serverPorts.Add(new object[] { (short)5432 }); // Local server
                }

                return serverPorts;
            }
        }

        /// <inheritdoc />
        public override ADatabaseProviderOptions GetDatabaseProviderOptions(string databaseName, short port)
        {
            return new PostgreSqlDatabaseProviderOptions
            {
                Hostname = "localhost",
                Database = databaseName,
                Username = "postgres",
                Password = this.CipherService.EncryptString("test1234"),
                Port = port,
            };
        }

        /// <inheritdoc />
        public override void ExecuteScriptCore(string script, string databaseName, short port)
        {
            using var context = new PostgreSqlDatabaseContext(this.LoggerFactory, this.CipherService, (PostgreSqlDatabaseProviderOptions)this.GetDatabaseProviderOptions(databaseName, port));
            context.ExecuteNonQuery(script);
        }

        /// <inheritdoc />
        public override void CreateSakilaDatabase(string databaseName, short port)
        {
            this.DropAndCreateDatabase(databaseName, port);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Datasources", "sakila-schema-postgresql.sql");
            this.ExecuteScript(File.ReadAllText(path), databaseName, port);
        }

        /// <inheritdoc />
        public override void DropDatabase(string databaseName, short port)
        {
            var dropDbQuery = new StringBuilder();
            dropDbQuery.AppendLine($"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='{databaseName}';");
            dropDbQuery.AppendLine($"DROP DATABASE IF EXISTS {databaseName};");
            this.ExecuteScript(dropDbQuery.ToString(), string.Empty, port);
        }

        /// <inheritdoc />
        public override void DropAndCreateDatabase(string databaseName, short port)
        {
            this.DropDatabase(databaseName, port);
            this.ExecuteScript($"CREATE DATABASE {databaseName};", string.Empty, port);
        }
    }
}
