using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;
using TiCodeX.SQLSchemaCompare.Infrastructure.EntityFramework;
using TiCodeX.SQLSchemaCompare.Test;

namespace SQLSchemaCompare.Test
{
    /// <summary>
    /// Creates the sakila database for the tests
    /// </summary>
    public class DatabaseFixtureMariaDb : DatabaseFixture
    {
        /// <summary>
        /// Gets the list of MariaDB server ports
        /// </summary>
        public static IEnumerable<object[]> ServerPorts
        {
            get
            {
                var serverPorts = new List<object[]>();

                if (Environment.GetEnvironmentVariable("RunDockerTests")?.ToUpperInvariant() == "TRUE" || DatabaseFixture.ForceDockerTests)
                {
                    serverPorts.Add(new object[] { (short)29001 }); // Version 5.5 (EOL April 2020)
                    serverPorts.Add(new object[] { (short)29002 }); // Version 10.0 (EOL March 2019)
                    serverPorts.Add(new object[] { (short)29003 }); // Version 10.1 (EOL October 2020)
                    serverPorts.Add(new object[] { (short)29004 }); // Version 10.2 (EOL May 2022)
                    serverPorts.Add(new object[] { (short)29005 }); // Version 10.3 (EOL May 2023)
                }
                else
                {
                    serverPorts.Add(new object[] { (short)3307 }); // Local server
                }

                return serverPorts;
            }
        }

        /// <inheritdoc />
        public override ADatabaseProviderOptions GetDatabaseProviderOptions(string databaseName, short port)
        {
            return new MariaDbDatabaseProviderOptions
            {
                Hostname = "localhost",
                Database = databaseName,
                Username = "root",
                Password = this.CipherService.EncryptString("test1234"),
                UseSSL = false,
                Port = port,
            };
        }

        /// <inheritdoc/>
        public override void ExecuteScriptCore(string script, string databaseName, short port)
        {
            var mariadbdpo = this.GetDatabaseProviderOptions(databaseName, port);
            var mysqldpo = new MySqlDatabaseProviderOptions
            {
                Hostname = mariadbdpo.Hostname,
                Database = mariadbdpo.Database,
                Username = mariadbdpo.Username,
                Password = mariadbdpo.Password,
                UseSSL = mariadbdpo.UseSSL,
                Port = mariadbdpo.Port,
            };
            using (var context = new MySqlDatabaseContext(this.LoggerFactory, this.CipherService, mysqldpo))
            {
                context.Database.OpenConnection();

                var mySqlScript = new MySqlScript((MySqlConnection)context.Database.GetDbConnection(), script);
                mySqlScript.Execute();
            }
        }

        /// <inheritdoc />
        public override void CreateSakilaDatabase(string databaseName, short port)
        {
            this.DropAndCreateDatabase(databaseName, port);

            var sakilaScript = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Datasources", "sakila-schema-mariadb.sql"));

            this.ExecuteScript(sakilaScript, databaseName, port);
        }

        /// <inheritdoc />
        public override void DropDatabase(string databaseName, short port)
        {
            this.ExecuteScript($"DROP SCHEMA IF EXISTS `{databaseName}`;", string.Empty, port);
        }

        /// <inheritdoc />
        public override void DropAndCreateDatabase(string databaseName, short port)
        {
            this.DropDatabase(databaseName, port);

            this.ExecuteScript($"CREATE SCHEMA `{databaseName}`;", string.Empty, port);
        }
    }
}
