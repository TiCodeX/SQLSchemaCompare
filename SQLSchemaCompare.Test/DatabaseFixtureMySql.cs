namespace TiCodeX.SQLSchemaCompare.Test
{
    /// <summary>
    /// Creates the sakila database for the tests
    /// </summary>
    public class DatabaseFixtureMySql : DatabaseFixture
    {
        /// <summary>
        /// Gets the list of MySQL server ports
        /// </summary>
        [SuppressMessage("Code Smell", "S125:Sections of code should not be commented out", Justification = "Historical reference")]
        public static IEnumerable<object[]> ServerPorts
        {
            get
            {
                var serverPorts = new List<object[]>();

                if (Environment.GetEnvironmentVariable("RunDockerTests")?.ToUpperInvariant() == "TRUE" || DatabaseFixture.ForceDockerTests)
                {
                    /*serverPorts.Add(new object[] { (ushort)27001 });*/ // Version 5.5 (EOL December 2018)
                    /*serverPorts.Add(new object[] { (ushort)27002 });*/ // Version 5.6 (EOL February 2021)
                    /*serverPorts.Add(new object[] { (ushort)27003 });*/ // Version 5.7 (EOL October 2023)
                    serverPorts.Add(new object[] { (ushort)27004 }); // Version 8.0 (EOL April 2026)
                    /*serverPorts.Add(new object[] { (ushort)27005 });*/ // Version 8.1 (EOL October 2023)
                    serverPorts.Add(new object[] { (ushort)27006 }); // Version 8.2
                    serverPorts.Add(new object[] { (ushort)27007 }); // Version 8.3
                    serverPorts.Add(new object[] { (ushort)27008 }); // Version 8.4 (EOL April 2032)
                }
                else
                {
                    serverPorts.Add(new object[] { (ushort)3306 }); // Local server
                }

                return serverPorts;
            }
        }

        /// <inheritdoc />
        public override ADatabaseProviderOptions GetDatabaseProviderOptions(string databaseName, ushort port)
        {
            return new MySqlDatabaseProviderOptions
            {
                Hostname = "localhost",
                Database = databaseName,
                Username = "root",
                Password = this.CipherService.EncryptString("test1234"),
                UseSsl = false,
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

            using var context = new MySqlDatabaseContext(this.LoggerFactory, this.CipherService, (MySqlDatabaseProviderOptions)this.GetDatabaseProviderOptions(databaseName, port));
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

                context.ExecuteNonQuery(!string.IsNullOrWhiteSpace(currentDelimiter) ? query.Replace(currentDelimiter, ";", StringComparison.Ordinal) : query);
            }
        }

        /// <inheritdoc />
        public override void CreateSakilaDatabase(string databaseName, ushort port)
        {
            this.DropAndCreateDatabase(databaseName, port);

            var sakilaScript = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Datasources", "sakila-schema-mysql.sql"));

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
