using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;
using TiCodeX.SQLSchemaCompare.Infrastructure.EntityFramework;
using TiCodeX.SQLSchemaCompare.Test;
using Xunit.Sdk;

namespace SQLSchemaCompare.Test
{
    /// <summary>
    /// Creates the sakila database for the tests
    /// </summary>
    public class DatabaseFixtureMySql : DatabaseFixture
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseFixtureMySql"/> class
        /// </summary>
        public DatabaseFixtureMySql()
            : base(ServerPorts)
        {
        }

        /// <summary>
        /// Gets the list of MySQL server ports
        /// </summary>
        public static IEnumerable<object[]> ServerPorts
        {
            get
            {
                var serverPorts = new List<object[]>();

                if (Environment.GetEnvironmentVariable("RunDockerTests")?.ToUpperInvariant() == "TRUE" || DatabaseFixture.ForceDockerTests)
                {
                    /*serverPorts.Add(new object[] { (short)27001 });*/ // Version 5.5 (EOL December 2018)
                    serverPorts.Add(new object[] { (short)27002 }); // Version 5.6 (EOL February 2021)
                    serverPorts.Add(new object[] { (short)27003 }); // Version 5.7 (EOL October 2023)
                    serverPorts.Add(new object[] { (short)27004 }); // Version 8.0 (EOL April 2026)
                }
                else
                {
                    serverPorts.Add(new object[] { (short)3306 }); // Local server
                }

                return serverPorts;
            }
        }

        /// <inheritdoc />
        public override ADatabaseProviderOptions GetDatabaseProviderOptions(string databaseName, short port)
        {
            return new MySqlDatabaseProviderOptions
            {
                Hostname = "localhost",
                Database = databaseName,
                Username = "root",
                Password = this.CipherService.EncryptString("test1234"),
                UseSSL = port == 3306, // Use SSL only for local server
                Port = port,
            };
        }

        /// <inheritdoc/>
        public override void ExecuteScript(string script, string databaseName, short port)
        {
            using (var context = new MySqlDatabaseContext(this.LoggerFactory, this.CipherService, (MySqlDatabaseProviderOptions)this.GetDatabaseProviderOptions(databaseName, port)))
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

            var sakilaScript = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Datasources", "sakila-schema-mysql.sql"));

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
