namespace TiCodeX.SQLSchemaCompare.Test.Integration
{
    /// <summary>
    /// Integration tests for Microsoft SQL
    /// </summary>
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "Limitation: the rule does not perform cross-procedural analysis")]
    public class MicrosoftSqlTests : BaseTests<MicrosoftSqlTests>, IClassFixture<DatabaseFixtureMicrosoftSql>
    {
        /// <summary>
        /// The database fixture
        /// </summary>
        private readonly DatabaseFixtureMicrosoftSql dbFixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        /// <param name="dbFixture">The database fixture</param>
        public MicrosoftSqlTests(ITestOutputHelper output, DatabaseFixtureMicrosoftSql dbFixture)
            : base(output)
        {
            if (dbFixture == null)
            {
                throw new ArgumentNullException(nameof(dbFixture));
            }

            this.dbFixture = dbFixture;
            this.dbFixture.SetLoggerFactory(this.LoggerFactory);
            this.dbFixture.InitServers(DatabaseFixtureMicrosoftSql.ServerPorts);
        }

        /// <summary>
        /// Test the retrieval of database list
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void GetMicrosoftSqlDatabaseList(ushort port)
        {
            var mssqldbp = this.dbFixture.GetDatabaseProvider(string.Empty, port);
            var dbList = mssqldbp.GetDatabaseList();
            dbList.Should().Contain("msdb");
            dbList.Should().Contain("master");
            dbList.Should().Contain("sakila");
        }

        /// <summary>
        /// Test the retrieval of the sakila database
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void GetMicrosoftSqlSakilaDatabase(ushort port)
        {
            var mssqldbp = this.dbFixture.GetDatabaseProvider("sakila", port);
            var db = mssqldbp.GetDatabase(new TaskInfo("test"));
            db.Should().NotBeNull();
            db.Name.Should().Be("sakila");

            db.Schemas.Count.Should().Be(3);
            db.Tables.Count.Should().Be(21);

            var table = db.Tables.FirstOrDefault(x => x.Name == "film");
            table.Should().NotBeNull();
            table.HasPeriod.Should().BeFalse();
            table.PeriodName.Should().BeNull();
            table.PeriodStartColumn.Should().BeNull();
            table.PeriodEndColumn.Should().BeNull();
            table.HasHistoryTable.Should().BeFalse();
            table.HistoryTableSchema.Should().BeNull();
            table.HistoryTableName.Should().BeNull();
            table.Columns.Count.Should().Be(13);
            table.Columns.Select(x => x.Name).Should().Contain("language_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "film_category");
            table.Should().NotBeNull();
            table.HasPeriod.Should().BeFalse();
            table.PeriodName.Should().BeNull();
            table.PeriodStartColumn.Should().BeNull();
            table.PeriodEndColumn.Should().BeNull();
            table.HasHistoryTable.Should().BeFalse();
            table.HistoryTableSchema.Should().BeNull();
            table.HistoryTableName.Should().BeNull();
            table.PrimaryKeys.Count.Should().Be(1);
            table.PrimaryKeys.First().ColumnNames.Should().Contain("film_id");
            table.PrimaryKeys.First().ColumnNames.Should().Contain("category_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "rental");
            table.Should().NotBeNull();
            table.HasPeriod.Should().BeFalse();
            table.PeriodName.Should().BeNull();
            table.PeriodStartColumn.Should().BeNull();
            table.PeriodEndColumn.Should().BeNull();
            table.HasHistoryTable.Should().BeFalse();
            table.HistoryTableSchema.Should().BeNull();
            table.HistoryTableName.Should().BeNull();
            table.ForeignKeys.Count.Should().Be(3);
            table.ForeignKeys.Should().Contain(x => x.Name == "fk_rental_customer");
            table.ForeignKeys.Should().Contain(x => x.Name == "fk_rental_inventory");
            table.ForeignKeys.Should().Contain(x => x.Name == "fk_rental_staff");

            table = db.Tables.FirstOrDefault(x => x.Name == "TableWithPeriod");
            table.Should().NotBeNull();
            table.HasPeriod.Should().BeTrue();
            table.PeriodName.Should().Be("SYSTEM_TIME");
            table.PeriodStartColumn.Should().Be("ValidFrom");
            table.PeriodEndColumn.Should().Be("ValidTo");
            table.HasHistoryTable.Should().BeFalse();
            table.HistoryTableSchema.Should().BeNull();
            table.HistoryTableName.Should().BeNull();
            table.Columns.Count.Should().Be(4);
            ((MicrosoftSqlColumn)table.Columns.First(x => x.Name == "ValidFrom")).GeneratedAlwaysType.Should().Be(1);
            ((MicrosoftSqlColumn)table.Columns.First(x => x.Name == "ValidTo")).GeneratedAlwaysType.Should().Be(2);

            table = db.Tables.FirstOrDefault(x => x.Name == "TemporalTable");
            table.Should().NotBeNull();
            table.HasPeriod.Should().BeTrue();
            table.PeriodName.Should().Be("SYSTEM_TIME");
            table.PeriodStartColumn.Should().Be("ValidFrom");
            table.PeriodEndColumn.Should().Be("ValidTo");
            table.HasHistoryTable.Should().BeTrue();
            table.HistoryTableSchema.Should().Be("business");
            table.HistoryTableName.Should().Be("TemporalTableHistory");
            table.Columns.Count.Should().Be(4);
            ((MicrosoftSqlColumn)table.Columns.First(x => x.Name == "ValidFrom")).GeneratedAlwaysType.Should().Be(1);
            ((MicrosoftSqlColumn)table.Columns.First(x => x.Name == "ValidTo")).GeneratedAlwaysType.Should().Be(2);

            db.Views.Count.Should().Be(6);
            db.Views.Should().ContainSingle(x => x.Name == "film_list");

            db.Functions.Count.Should().Be(2);
            db.StoredProcedures.Count.Should().Be(2);

            db.DataTypes.Should().NotBeNullOrEmpty();
            db.DataTypes.Count.Should().Be(37);

            if (db.ServerVersion.Major < 11)
            {
                db.Sequences.Should().BeNullOrEmpty();
            }
            else
            {
                db.Sequences.Count.Should().Be(1);
            }
        }

        /// <summary>
        /// Test cloning MicrosoftSQL 'sakila' database
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void CloneMicrosoftSqlDatabase(ushort port)
        {
            const string databaseName = "sakila";
            var clonedDatabaseName = DatabaseFixture.GenerateDatabaseName();

            try
            {
                var mssqldbp = this.dbFixture.GetDatabaseProvider(databaseName, port);
                var db = mssqldbp.GetDatabase(new TaskInfo("test"));

                var scripterFactory = new DatabaseScripterFactory(this.LoggerFactory);
                var scripter = scripterFactory.Create(db, new TiCodeX.SQLSchemaCompare.Core.Entities.Project.ProjectOptions());
                var fullScript = scripter.GenerateFullCreateScript(db);

                this.dbFixture.DropAndCreateDatabase(clonedDatabaseName, port);

                this.dbFixture.ExecuteScript(fullScript, clonedDatabaseName, port);

                this.dbFixture.CompareDatabases(DatabaseType.MicrosoftSql, clonedDatabaseName, databaseName, port);
            }
            finally
            {
                this.dbFixture.DropDatabase(clonedDatabaseName, port);
            }
        }

        /// <summary>
        /// Test migration script when source db is empty (a.k.a. drop whole target)
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseSourceEmpty(ushort port)
        {
            var sourceDatabaseName = DatabaseFixture.GenerateDatabaseName();
            var targetDatabaseName = DatabaseFixture.GenerateDatabaseName();

            try
            {
                // Create the empty database
                this.dbFixture.DropAndCreateDatabase(sourceDatabaseName, port);

                // Create the database with sakila to be migrated to empty
                this.dbFixture.CreateSakilaDatabase(targetDatabaseName, port);

                this.dbFixture.ExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sourceDatabaseName, targetDatabaseName, port);
            }
            finally
            {
                this.dbFixture.DropDatabase(sourceDatabaseName, port);
                this.dbFixture.DropDatabase(targetDatabaseName, port);
            }
        }

        /// <summary>
        /// Test migration script when target db is empty (a.k.a. re-create whole source)
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetEmpty(ushort port)
        {
            const string sourceDatabaseName = "sakila";
            var targetDatabaseName = DatabaseFixture.GenerateDatabaseName();

            try
            {
                this.dbFixture.DropAndCreateDatabase(targetDatabaseName, port);

                this.dbFixture.ExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sourceDatabaseName, targetDatabaseName, port);
            }
            finally
            {
                this.dbFixture.DropDatabase(targetDatabaseName, port);
            }
        }

        /// <summary>
        /// Test migration script when target db doesn't have a column
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetMissingColumn(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE business.staff DROP COLUMN last_name");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an extra column with data
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetMissingColumnWithData(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DECLARE @sqlCommand nvarchar(MAX)");
            sb.AppendLine("SET @sqlCommand = (SELECT STRING_AGG(CONCAT('ALTER TABLE business.TableWithData DROP COLUMN ', COLUMN_NAME), '; ')");
            sb.AppendLine("                   FROM INFORMATION_SCHEMA.COLUMNS");
            sb.AppendLine("                   WHERE TABLE_SCHEMA='business' AND TABLE_NAME='TableWithData' AND COLUMN_NAME != 'TableWithDataId')");
            sb.AppendLine("EXEC (@sqlCommand)");
            this.dbFixture.ProjectOptions.Scripting.GenerateUpdateScriptForNewNotNullColumns = true;
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a column
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetMissingColumns(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE business.staff DROP COLUMN last_name, username");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an extra column
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetExtraColumn(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE business.staff ADD middle_name VARCHAR(45) NOT NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an extra column
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetExtraColumns(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE business.staff ADD middle_name VARCHAR(45) NOT NULL");
            sb.AppendLine("ALTER TABLE business.staff ADD middle_name2 VARCHAR(45) NOT NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a column with different type
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetColumnDifferentType(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE customer_data.country ALTER COLUMN country NVARCHAR(80) NOT NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different view
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentView(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER VIEW customer_list AS SELECT NULL AS 'test'");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different function
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentFunction(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER FUNCTION StripWWWandCom (@Input VARCHAR(250))");
            sb.AppendLine("RETURNS VARCHAR(250)");
            sb.AppendLine("AS BEGIN");
            sb.AppendLine("    DECLARE @Work VARCHAR(250)");
            sb.AppendLine();
            sb.AppendLine("    SET @Work = @Input");
            sb.AppendLine();
            sb.AppendLine("    SET @Work = REPLACE(@Work, 'www.', '')");
            sb.AppendLine("    SET @Work = REPLACE(@Work, '.net', '')");
            sb.AppendLine();
            sb.AppendLine("    RETURN @Work");
            sb.AppendLine("END");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different stored procedure
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentStoredProcedure(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER PROCEDURE uspGetAddress @City nvarchar(30) = NULL");
            sb.AppendLine("AS");
            sb.AppendLine("SELECT *");
            sb.AppendLine("FROM city");
            sb.AppendLine("WHERE city = @City");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different sequence
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentSequence(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER SEQUENCE actor_seq");
            sb.AppendLine("INCREMENT BY 5");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different type
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentType(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP TYPE custom_decimal");
            sb.AppendLine("CREATE TYPE custom_decimal FROM DECIMAL(21, 6) NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a index
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetMissingIndex(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX idx_uq ON business.rental");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a index with included columns
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetMissingIndexWithIncludedColumns(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX idx_address_address2_district_postal_code ON customer_data.address");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional index
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetExtraIndex(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE INDEX idx_replacement_cost ON inventory.film (replacement_cost)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional index with included columns
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetExtraIndexWithIncludedColumns(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE INDEX idx_title_release_year_special_features ON inventory.film (title, release_year) INCLUDE (special_features)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different filtered index
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentIndexFilter(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX idx_actor_last_name ON customer_data.actor");
            sb.AppendLine("CREATE INDEX idx_actor_last_name ON customer_data.actor(last_name) WHERE (first_name IS NOT NULL)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different index type
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentIndexType(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX idx_fk_address_id ON business.store");
            sb.AppendLine("CREATE CLUSTERED INDEX idx_fk_address_id ON business.store(manager_staff_id)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different index included columns
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentIndexIncludedColumns(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX idx_address_address2_district_postal_code ON customer_data.address");
            sb.AppendLine("CREATE INDEX idx_address_address2_district_postal_code ON customer_data.address(address, address2) INCLUDE(district, postal_code, phone)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a trigger
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetMissingTrigger(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP TRIGGER customer_data.reminder1");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional trigger
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetExtraTrigger(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE TRIGGER reminder2");
            sb.AppendLine("ON customer_data.country");
            sb.AppendLine("AFTER DELETE");
            sb.AppendLine("AS RAISERROR ('BLABLABLA', 16, 10)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different trigger
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentTrigger(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TRIGGER customer_data.reminder1");
            sb.AppendLine("ON customer_data.country");
            sb.AppendLine("AFTER INSERT, DELETE");
            sb.AppendLine("AS RAISERROR ('test', 2, 10)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a default constraint
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetMissingDefaultConstraint(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE customer_data.actor DROP CONSTRAINT DF_actor_last_update");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional default constraint
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetExtraDefaultConstraint(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE customer_data.actor ADD CONSTRAINT DF_actor_last_name DEFAULT 'test' FOR last_name");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different default constraint
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentDefaultConstraint(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE customer_data.actor DROP CONSTRAINT DF_actor_last_update");
            sb.AppendLine("ALTER TABLE customer_data.actor ADD CONSTRAINT DF_actor_last_update DEFAULT (GETDATE()-444) FOR last_update");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a check constraint
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetMissingCheckConstraint(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE inventory.film DROP CONSTRAINT CHECK_special_features");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional check constraint
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetExtraCheckConstraint(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE customer_data.customer ADD CONSTRAINT check_email CHECK(email LIKE '_%@_%._%')");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different check constraint
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentCheckConstraint(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE inventory.film DROP CONSTRAINT CHECK_special_features");
            sb.AppendLine("ALTER TABLE inventory.film ADD CONSTRAINT CHECK_special_features CHECK(special_features IS NOT null)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a primary key
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetMissingPrimaryKey(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE inventory.film_actor_description DROP CONSTRAINT PK_film_actor_description_film_actor_description_id");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional primary key
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetExtraPrimaryKey(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE inventory.film_text ADD CONSTRAINT PK_film_text_film_id PRIMARY KEY NONCLUSTERED (film_id)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional primary key with a foreing key that reference it
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetExtraPrimaryKeyWithReferencingForeignKey(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE inventory.film_text ADD CONSTRAINT PK_film_text_film_id PRIMARY KEY NONCLUSTERED (film_id)");
            sb.AppendLine("ALTER TABLE inventory.film_text_extra ADD CONSTRAINT FK_film_text_extra_film FOREIGN KEY (film_id) REFERENCES inventory.film_text (film_id) ON DELETE CASCADE");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port, 2);
        }

        /// <summary>
        /// Test migration script when target db have an primary key on a different colum
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetPrimaryKeyOnDifferentColumn(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE business.payment DROP CONSTRAINT PK_payment_payment_id");
            sb.AppendLine("ALTER TABLE business.payment ADD CONSTRAINT PK_payment_payment_id PRIMARY KEY NONCLUSTERED (payment_id_new)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional foreign key
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetExtraForeignKey(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE category ADD CONSTRAINT FK_category_language FOREIGN KEY (language_id) REFERENCES language (language_id)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a foreign key
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetMissingForeignKey(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE inventory.film_actor DROP CONSTRAINT fk_film_actor_film");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a column with a foreign key
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetMissingColumnWithForeignKey(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX idx_fk_city_id ON customer_data.address");
            sb.AppendLine("ALTER TABLE customer_data.address DROP CONSTRAINT fk_address_city");
            sb.AppendLine("ALTER TABLE customer_data.address DROP COLUMN city_id");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a foreign key that references a different column
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetForeignKeyReferencesDifferentColumn(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE customer_data.address DROP CONSTRAINT fk_address_city");
            sb.AppendLine("ALTER TABLE customer_data.address ADD CONSTRAINT fk_address_city FOREIGN KEY (city_id) REFERENCES customer_data.actor (actor_id) ON DELETE NO ACTION ON UPDATE CASCADE");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a foreign key with different options
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetForeignKeyDifferentOptions(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE customer_data.address DROP CONSTRAINT fk_address_city");
            sb.AppendLine("ALTER TABLE customer_data.address ADD CONSTRAINT fk_address_city FOREIGN KEY (city_id) REFERENCES customer_data.city (city_id) ON DELETE CASCADE ON UPDATE NO ACTION");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a schema with different owner
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetSchemaDifferentOwner(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER AUTHORIZATION ON SCHEMA::[business] TO [guest]");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a long stored procedure
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetMissingStoredProcedure(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP PROCEDURE SeedData");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port, 0);
        }

        /// <summary>
        /// Test migration script when target db doesn't have the table with the period
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetMissingTableWithPeriod(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP TABLE business.TableWithPeriod");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port, 0);
        }

        /// <summary>
        /// Test migration script when target db have the table without the period
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetTableMissingPeriod(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE business.TableWithPeriod DROP PERIOD FOR SYSTEM_TIME");
            sb.AppendLine("ALTER TABLE business.TableWithPeriod DROP COLUMN ValidFrom");
            sb.AppendLine("ALTER TABLE business.TableWithPeriod DROP COLUMN ValidTo");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have the table with the period
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetTableWithPeriod(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE customer_data.actor ADD");
            sb.AppendLine("   ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,");
            sb.AppendLine("   ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,");
            sb.AppendLine("   PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an extra table with the period
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetExtraTableWithPeriod(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE TABLE business.TableWithPeriodExtra (");
            sb.AppendLine("  [TableWithPeriodExtraId] int NOT NULL,");
            sb.AppendLine("  [Name] nvarchar(100) NOT NULL,");
            sb.AppendLine("  [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN,");
            sb.AppendLine("  [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN,");
            sb.AppendLine("  PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo),");
            sb.AppendLine("  CONSTRAINT PK_TableWithPeriodExtra_TableWithPeriodExtraId PRIMARY KEY NONCLUSTERED (TableWithPeriodExtraId)");
            sb.AppendLine(")");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port, 0);
        }

        /// <summary>
        /// Test migration script when target db doesn't have the table with the history
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetMissingTableWithHistory(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE business.TemporalTable SET (SYSTEM_VERSIONING = OFF)");
            sb.AppendLine("DROP TABLE business.TemporalTableHistory");
            sb.AppendLine("DROP TABLE business.TemporalTable");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port, 0);
        }

        /// <summary>
        /// Test migration script when target db have the table without the history
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetTableMissingHistory(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE business.TemporalTable SET (SYSTEM_VERSIONING = OFF)");
            sb.AppendLine("DROP TABLE business.TemporalTableHistory");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have the table with the history
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetTableWithHistory(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE customer_data.actor ADD");
            sb.AppendLine("   ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,");
            sb.AppendLine("   ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,");
            sb.AppendLine("   PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)");
            sb.AppendLine("ALTER TABLE customer_data.actor SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = customer_data.actorHistory))");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an extra table with the history
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetExtraTableWithHistory(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE TABLE business.TemporalTableExtra (");
            sb.AppendLine("  [TemporalTableExtraId] int NOT NULL,");
            sb.AppendLine("  [Name] nvarchar(100) NOT NULL,");
            sb.AppendLine("  [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN,");
            sb.AppendLine("  [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN,");
            sb.AppendLine("  PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo),");
            sb.AppendLine("  CONSTRAINT PK_TemporalTableExtra_TemporalTableExtraId PRIMARY KEY NONCLUSTERED (TemporalTableExtraId)");
            sb.AppendLine(")");
            sb.AppendLine("WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = business.TemporalTableExtraHistory))");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port, 0);
        }

        /// <summary>
        /// Test migration script when target db have the table with a different history table
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentHistoryTable(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE business.TemporalTable SET (SYSTEM_VERSIONING = OFF)");
            sb.AppendLine("DROP TABLE business.TemporalTableHistory");
            sb.AppendLine("ALTER TABLE business.TemporalTable SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = business.TemporalTableDifferentHistory))");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional default constraint on the period
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        [Category("MicrosoftSQL")]
        public void MigrateMicrosoftSqlDatabaseTargetExtraDefaultConstraintOnPeriod(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE business.TableWithPeriod ADD CONSTRAINT DF_TableWithPeriod_ValidFrom DEFAULT SYSUTCDATETIME() FOR ValidFrom");
            sb.AppendLine("ALTER TABLE business.TableWithPeriod ADD CONSTRAINT DF_TableWithPeriod_ValidTo DEFAULT CONVERT(DATETIME2, '9999-12-3123:59:59.9999999') FOR ValidTo");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }
    }
}
