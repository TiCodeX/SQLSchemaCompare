namespace TiCodeX.SQLSchemaCompare.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.EntityFrameworkCore;
    using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;
    using TiCodeX.SQLSchemaCompare.Infrastructure.EntityFramework;

    /// <summary>
    /// Creates the sakila database for the tests
    /// </summary>
    public class DatabaseFixtureMariaDb : DatabaseFixture
    {
        /// <summary>
        /// Gets the list of MariaDB server ports
        /// </summary>
        [SuppressMessage("Code Smell", "S125:Sections of code should not be commented out", Justification = "Historical reference")]
        [SuppressMessage("Code Smell", "S2589:Boolean expressions should not be gratuitous", Justification = "Debugging convenience")]
        public static IEnumerable<object[]> ServerPorts
        {
            get
            {
                var serverPorts = new List<object[]>();

                if (Environment.GetEnvironmentVariable("RunDockerTests")?.ToUpperInvariant() == "TRUE" || DatabaseFixture.ForceDockerTests)
                {
                    /*serverPorts.Add(new object[] { (ushort)29001 });*/ // Version 5.5 (EOL April 2020)
                    /*serverPorts.Add(new object[] { (ushort)29002 });*/ // Version 10.0 (EOL March 2019)
                    /*serverPorts.Add(new object[] { (ushort)29003 });*/ // Version 10.1 (EOL October 2020)
                    /*serverPorts.Add(new object[] { (ushort)29004 });*/ // Version 10.2 (EOL May 2022)
                    serverPorts.Add(new object[] { (ushort)29005 }); // Version 10.3 (EOL May 2023)
                    serverPorts.Add(new object[] { (ushort)29006 }); // Version 10.4 (EOL June 2024)
                    serverPorts.Add(new object[] { (ushort)29007 }); // Version 10.5 (EOL June 2025)
                    serverPorts.Add(new object[] { (ushort)29008 }); // Version 10.6 (EOL July 2026)
                    serverPorts.Add(new object[] { (ushort)29009 }); // Version 10.7 (EOL February 2023)
                    serverPorts.Add(new object[] { (ushort)29010 }); // Version 10.8 (EOL May 2023)
                    serverPorts.Add(new object[] { (ushort)29011 }); // Version 10.9 (EOL August 2023)
                    serverPorts.Add(new object[] { (ushort)29012 }); // Version 10.10 (EOL November 2023)
                    serverPorts.Add(new object[] { (ushort)29013 }); // Version 10.11 (EOL February 2028)
                }
                else
                {
                    serverPorts.Add(new object[] { (ushort)3307 }); // Local server
                }

                return serverPorts;
            }
        }

        /// <inheritdoc />
        public override ADatabaseProviderOptions GetDatabaseProviderOptions(string databaseName, ushort port)
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
        public override void ExecuteScriptCore(string script, string databaseName, ushort port)
        {
            if (script == null)
            {
                throw new ArgumentNullException(nameof(script));
            }

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

            using var context = new MySqlDatabaseContext(this.LoggerFactory, this.CipherService, mysqldpo);
            context.Database.OpenConnection();

            var queries = Regex.Split(script, "^(DELIMITER .*)$", RegexOptions.Multiline);
            var currentDelimiter = string.Empty;
            foreach (var query in queries.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                if (query.StartsWith("DELIMITER", StringComparison.Ordinal))
                {
                    currentDelimiter = query.Substring(9).Trim();
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(currentDelimiter))
                {
                    context.ExecuteNonQuery(query.Replace(currentDelimiter, ";", StringComparison.Ordinal));
                }
                else
                {
                    context.ExecuteNonQuery(query);
                }
            }
        }

        /// <inheritdoc />
        public override void CreateSakilaDatabase(string databaseName, ushort port)
        {
            this.DropAndCreateDatabase(databaseName, port);

            var sakilaScript = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Datasources", "sakila-schema-mariadb.sql"));

            this.ExecuteScript(sakilaScript, databaseName, port);
        }

        /// <inheritdoc />
        public override void DropDatabase(string databaseName, ushort port)
        {
            this.ExecuteScript($"DROP SCHEMA IF EXISTS `{databaseName}`;", string.Empty, port);
        }

        /// <inheritdoc />
        public override void DropAndCreateDatabase(string databaseName, ushort port)
        {
            this.DropDatabase(databaseName, port);

            this.ExecuteScript($"CREATE SCHEMA `{databaseName}`;", string.Empty, port);
        }
    }
}
