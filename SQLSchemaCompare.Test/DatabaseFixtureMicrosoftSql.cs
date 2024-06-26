﻿namespace TiCodeX.SQLSchemaCompare.Test
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
                    serverPorts.Add(new object[] { (ushort)28001 }); // Version 2017 Linux
                    serverPorts.Add(new object[] { (ushort)28002 }); // Version 2019 Linux
                    serverPorts.Add(new object[] { (ushort)28003 }); // Version 2022 Linux
                }
                else
                {
                    serverPorts.Add(new object[] { (ushort)1433 }); // Local server
                }

                return serverPorts;
            }
        }

        /// <inheritdoc />
        public override ADatabaseProviderOptions GetDatabaseProviderOptions(string databaseName, ushort port)
        {
            return new MicrosoftSqlDatabaseProviderOptions
            {
                Hostname = "localhost",
                Database = databaseName,
                Username = "sa",
                Password = this.CipherService.EncryptString("Test1234!"),
                Port = port,
            };
        }

        /// <inheritdoc />
        public override void ExecuteScriptCore(string script, string databaseName, ushort port)
        {
            if (script == null)
            {
                throw new ArgumentNullException(nameof(script));
            }

            using var context = new MicrosoftSqlDatabaseContext(this.LoggerFactory, this.CipherService, (MicrosoftSqlDatabaseProviderOptions)this.GetDatabaseProviderOptions(databaseName, port));
            var queries = script.Split(new[] { "GO\r\n", "GO\n" }, StringSplitOptions.None);
            foreach (var query in queries.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                context.ExecuteNonQuery(query);
            }
        }

        /// <inheritdoc />
        public override void CreateSakilaDatabase(string databaseName, ushort port)
        {
            this.DropAndCreateDatabase(databaseName, port);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Datasources", "sakila-schema-microsoftsql.sql");
            this.ExecuteScript(File.ReadAllText(path), databaseName, port);
        }

        /// <inheritdoc />
        public override void DropDatabase(string databaseName, ushort port)
        {
            var dropDbQuery = new StringBuilder();
            dropDbQuery.AppendLine($"IF EXISTS(select * from sys.databases where name= '{databaseName}')");
            dropDbQuery.AppendLine("BEGIN");

            // Close existing connections
            dropDbQuery.AppendLine("  DECLARE @Spid INT");
            dropDbQuery.AppendLine("  DECLARE @ExecSQL VARCHAR(255)");
            dropDbQuery.AppendLine("  DECLARE KillCursor CURSOR LOCAL STATIC READ_ONLY FORWARD_ONLY");
            dropDbQuery.AppendLine($"  FOR SELECT DISTINCT SPID FROM MASTER..SysProcesses WHERE DBID = DB_ID('{databaseName}')");
            dropDbQuery.AppendLine("  OPEN KillCursor");
            dropDbQuery.AppendLine("  FETCH NEXT FROM KillCursor INTO @Spid");
            dropDbQuery.AppendLine("  WHILE @@FETCH_STATUS = 0");
            dropDbQuery.AppendLine("  BEGIN");
            dropDbQuery.AppendLine("    SET @ExecSQL = 'KILL ' + CAST(@Spid AS VARCHAR(50))");
            dropDbQuery.AppendLine("    EXEC(@ExecSQL)");
            dropDbQuery.AppendLine("    FETCH NEXT FROM KillCursor INTO @Spid");
            dropDbQuery.AppendLine("  END");
            dropDbQuery.AppendLine("  CLOSE KillCursor");
            dropDbQuery.AppendLine("  DEALLOCATE KillCursor");

            dropDbQuery.AppendLine($"  DROP DATABASE {databaseName}");
            dropDbQuery.AppendLine("END");
            this.ExecuteScript(dropDbQuery.ToString(), string.Empty, port);
        }

        /// <inheritdoc />
        public override void DropAndCreateDatabase(string databaseName, ushort port)
        {
            this.DropDatabase(databaseName, port);
            this.ExecuteScript($"CREATE DATABASE {databaseName}", string.Empty, port);
        }
    }
}
