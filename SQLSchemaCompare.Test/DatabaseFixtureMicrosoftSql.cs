using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;
using TiCodeX.SQLSchemaCompare.Infrastructure.EntityFramework;
using TiCodeX.SQLSchemaCompare.Test;

namespace SQLSchemaCompare.Test
{
    /// <summary>
    /// Creates the sakila database for the tests
    /// </summary>
    public class DatabaseFixtureMicrosoftSql : DatabaseFixture
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
                    serverPorts.Add(new object[] { (short)28001 }); // Version 2017 Linux
                    /*serverPorts.Add(new object[] { (short)28002 }); // Version 2019 Linux*/
                }
                else
                {
                    serverPorts.Add(new object[] { (short)1433 }); // Local server
                }

                return serverPorts;
            }
        }

        /// <inheritdoc />
        public override ADatabaseProviderOptions GetDatabaseProviderOptions(string databaseName, short port)
        {
            return new MicrosoftSqlDatabaseProviderOptions
            {
                Hostname = "localhost",
                Database = databaseName,
                Username = "sa",
                Password = this.CipherService.EncryptString("Test1234!"),
                Port = port
            };
        }

        /// <inheritdoc />
        public override void ExecuteScriptCore(string script, string databaseName, short port)
        {
            using (var context = new MicrosoftSqlDatabaseContext(this.LoggerFactory, this.CipherService, (MicrosoftSqlDatabaseProviderOptions)this.GetDatabaseProviderOptions(databaseName, port)))
            {
                var queries = script.Split(new[] { "GO\r\n", "GO\n" }, StringSplitOptions.None);
                foreach (var query in queries.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    context.ExecuteNonQuery(query);
                }
            }
        }

        /// <inheritdoc />
        public override void CreateSakilaDatabase(string databaseName, short port)
        {
            this.DropAndCreateDatabase(databaseName, port);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Datasources", "sakila-schema-microsoftsql.sql");
            this.ExecuteScript(File.ReadAllText(path), databaseName, port);
        }

        /// <inheritdoc />
        public override void DropDatabase(string databaseName, short port)
        {
            var dropDbQuery = new StringBuilder();
            dropDbQuery.AppendLine($"IF EXISTS(select * from sys.databases where name= '{databaseName}')");
            dropDbQuery.AppendLine("BEGIN");
            dropDbQuery.AppendLine($"  ALTER DATABASE {databaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE"); // Close existing connections
            dropDbQuery.AppendLine($"  DROP DATABASE {databaseName}");
            dropDbQuery.AppendLine("END");
            this.ExecuteScript(dropDbQuery.ToString(), string.Empty, port);
        }

        /// <inheritdoc />
        public override void DropAndCreateDatabase(string databaseName, short port)
        {
            this.DropDatabase(databaseName, port);
            this.ExecuteScript($"CREATE DATABASE {databaseName}", string.Empty, port);
        }
    }
}
