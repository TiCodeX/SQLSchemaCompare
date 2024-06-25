namespace TiCodeX.SQLSchemaCompare.Test.Integration
{
    /// <summary>
    /// Integration tests for PostgreSQL
    /// </summary>
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "Limitation: the rule does not perform cross-procedural analysis")]
    public class PostgreSqlTests : BaseTests<PostgreSqlTests>, IClassFixture<DatabaseFixturePostgreSql>
    {
        /// <summary>
        /// The database fixture
        /// </summary>
        private readonly DatabaseFixturePostgreSql dbFixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        /// <param name="dbFixture">The database fixture</param>
        public PostgreSqlTests(ITestOutputHelper output, DatabaseFixturePostgreSql dbFixture)
            : base(output)
        {
            if (dbFixture == null)
            {
                throw new ArgumentNullException(nameof(dbFixture));
            }

            this.dbFixture = dbFixture;
            this.dbFixture.SetLoggerFactory(this.LoggerFactory);
            this.dbFixture.InitServers(DatabaseFixturePostgreSql.ServerPorts);
        }

        /// <summary>
        /// Test the retrieval of database list
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void GetDatabaseList(ushort port)
        {
            var pgsqldbp = this.dbFixture.GetDatabaseProvider(string.Empty, port);
            var dbList = pgsqldbp.GetDatabaseList();
            dbList.Should().Contain("postgres");
            dbList.Should().Contain("sakila");
        }

        /// <summary>
        /// Test the retrieval of PostgreSQL 'sakila' database
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void GetPostgreSqlSakilaDatabase(ushort port)
        {
            var pgsqldbp = this.dbFixture.GetDatabaseProvider("sakila", port);
            var db = pgsqldbp.GetDatabase(new TaskInfo("test"));
            db.Should().NotBeNull();
            db.Name.Should().Be("sakila");

            db.Schemas.Count.Should().Be(3);

            db.DataTypes.Count(x => x.IsUserDefined).Should().Be(8);

            db.Tables.Count.Should().Be(22);
            var table = db.Tables.FirstOrDefault(x => x.Name == "film");
            table.Should().NotBeNull();
            table?.Columns.Count.Should().Be(14);
            table?.Columns.Select(x => x.Name).Should().Contain("language_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "film_category");
            table.Should().NotBeNull();
            table?.PrimaryKeys.Count.Should().Be(1);
            table?.PrimaryKeys.First().ColumnNames.Should().Contain("film_id");
            table?.PrimaryKeys.First().ColumnNames.Should().Contain("category_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "rental");
            table?.ForeignKeys.Count.Should().Be(3);
            table?.ForeignKeys.Should().Contain(x => x.Name == "rental_customer_id_fkey");
            table?.ForeignKeys.Should().Contain(x => x.Name == "rental_inventory_id_fkey");
            table?.ForeignKeys.Should().Contain(x => x.Name == "rental_staff_id_fkey");

            db.Views.Count.Should().Be(7);
            db.Views.Should().ContainSingle(x => x.Name == "film_list");

            db.Functions.Count.Should().Be(13);
            db.Functions.Should().ContainSingle(x => x.Name == "last_day");

            db.StoredProcedures.Should().BeEmpty();
        }

        /// <summary>
        /// Test cloning PostgreSQL 'sakila' database
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void ClonePostgreSqlDatabase(ushort port)
        {
            const string databaseName = "sakila";
            var clonedDatabaseName = DatabaseFixture.GenerateDatabaseName();

            try
            {
                var postgresqldbp = this.dbFixture.GetDatabaseProvider(databaseName, port);
                var db = postgresqldbp.GetDatabase(new TaskInfo("test"));

                var scripterFactory = new DatabaseScripterFactory(this.LoggerFactory);
                var scripter = scripterFactory.Create(db, new TiCodeX.SQLSchemaCompare.Core.Entities.Project.ProjectOptions());
                var fullScript = scripter.GenerateFullCreateScript(db);

                this.dbFixture.DropAndCreateDatabase(clonedDatabaseName, port);

                var sb = new StringBuilder();
                sb.AppendLine("SET check_function_bodies = false;");
                sb.AppendLine(fullScript);

                this.dbFixture.ExecuteScript(sb.ToString(), clonedDatabaseName, port);

                this.dbFixture.CompareDatabases(DatabaseType.PostgreSql, clonedDatabaseName, databaseName, port);
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
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseSourceEmpty(ushort port)
        {
            var sourceDatabaseName = DatabaseFixture.GenerateDatabaseName();
            var targetDatabaseName = DatabaseFixture.GenerateDatabaseName();

            try
            {
                // Create the empty database
                this.dbFixture.DropAndCreateDatabase(sourceDatabaseName, port);

                // Create the database with sakila to be migrated to empty
                this.dbFixture.CreateSakilaDatabase(targetDatabaseName, port);

                this.dbFixture.ExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sourceDatabaseName, targetDatabaseName, port);
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
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetEmpty(ushort port)
        {
            const string sourceDatabaseName = "sakila";
            var targetDatabaseName = DatabaseFixture.GenerateDatabaseName();

            try
            {
                this.dbFixture.DropAndCreateDatabase(targetDatabaseName, port);

                this.dbFixture.ExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sourceDatabaseName, targetDatabaseName, port);
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
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetMissingColumn(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE business.staff DROP COLUMN password");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a column
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetMissingColumns(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE business.staff DROP COLUMN password,");
            sb.Append("DROP COLUMN picture");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an extra column
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetExtraColumn(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE business.staff ADD middle_name VARCHAR(45) NOT NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an extra column
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetExtraColumns(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE business.staff ADD middle_name VARCHAR(45) NOT NULL,");
            sb.AppendLine("ADD middle_name2 VARCHAR(45) NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a column with different type
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetColumnDifferentType(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE language ALTER COLUMN name TYPE character varying(100)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a column without the default
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetColumnMissingDefault(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE customer_data.customer ALTER COLUMN activebool DROP DEFAULT");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a column with the default
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetColumnExtraDefault(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE customer_data.customer ALTER COLUMN last_name SET DEFAULT 'MyLastName'");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a column with a different default
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetColumnDifferentDefault(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE inventory.film ALTER COLUMN replacement_cost SET DEFAULT 11.45");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different view
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetDifferentView(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP VIEW customer_list;");
            sb.AppendLine("CREATE VIEW customer_list AS SELECT NULL AS \"test\";");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different function
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetDifferentFunction(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP FUNCTION film_in_stock(p_film_id integer, p_store_id integer, OUT p_film_count integer);");
            sb.AppendLine("CREATE FUNCTION film_in_stock(p_film_id integer, p_store_id integer, OUT p_film_count integer) RETURNS SETOF integer");
            sb.AppendLine("    LANGUAGE sql");
            sb.AppendLine("AS $BODY$");
            sb.AppendLine("     SELECT inventory_id");
            sb.AppendLine("     FROM inventory");
            sb.AppendLine("     WHERE film_id = $1");
            sb.AppendLine("     AND store_id = $2;");
            sb.AppendLine("$BODY$;");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different sequence
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetDifferentSequence(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER SEQUENCE customer_customer_id_seq");
            sb.AppendLine("INCREMENT BY 5");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different type
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetDifferentType(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP TYPE bug_status;");
            sb.AppendLine("CREATE TYPE bug_status AS ENUM ('open', 'closed');");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a index
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetMissingIndex(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX inventory.idx_actor_last_name");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional index
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetExtraIndex(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE UNIQUE INDEX idx_staff_email ON business.staff (email)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different index
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetDifferentIndex(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX business.idx_unq_manager_staff_id;");
            sb.AppendLine("CREATE INDEX idx_unq_manager_staff_id ON business.store USING btree (address_id);");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a trigger
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetMissingTrigger(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP TRIGGER film_fulltext_trigger ON inventory.film");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional trigger
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetExtraTrigger(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE TRIGGER last_updated2 BEFORE DELETE ON business.store FOR EACH ROW EXECUTE PROCEDURE last_updated();");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different trigger
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetDifferentTrigger(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP TRIGGER last_updated ON inventory.actor;");
            sb.AppendLine("CREATE TRIGGER last_updated AFTER INSERT ON inventory.actor FOR EACH ROW EXECUTE PROCEDURE last_updated();");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a check constraint
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetMissingCheckConstraint(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE payment_p2017_01 DROP CONSTRAINT payment_p2017_01_payment_date_check");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional check constraint
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetExtraCheckConstraint(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE business.staff ADD CONSTRAINT staff_check_email CHECK(email LIKE '_%@_%._%')");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different check constraint
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetDifferentCheckConstraint(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE payment_p2017_01 DROP CONSTRAINT payment_p2017_01_payment_date_check;");
            sb.AppendLine("ALTER TABLE payment_p2017_01 ADD CONSTRAINT payment_p2017_01_payment_date_check CHECK (((payment_date >= '2017-01-01 00:00:00'::timestamp without time zone) AND (payment_date < '2017-01-31 00:00:00'::timestamp without time zone)));");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a primary key
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetMissingPrimaryKey(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE business.payment DROP CONSTRAINT payment_pkey");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional primary key
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetExtraPrimaryKey(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE inventory.film_text ADD CONSTRAINT PK_film_text_film_id PRIMARY KEY (film_id)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional primary key with a foreing key that reference it
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetExtraPrimaryKeyWithReferencingForeignKey(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE inventory.film_text ADD CONSTRAINT PK_film_text_film_id PRIMARY KEY (film_id);");
            sb.AppendLine("ALTER TABLE inventory.film_category ADD CONSTRAINT FK_film_category_extra_film FOREIGN KEY (film_text_id) REFERENCES inventory.film_text (film_id) ON DELETE CASCADE");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port, 2);
        }

        /// <summary>
        /// Test migration script when target db have an primary key on a different colum
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetPrimaryKeyOnDifferentColumn(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE business.payment DROP CONSTRAINT payment_pkey;");
            sb.AppendLine("ALTER TABLE business.payment ADD CONSTRAINT payment_pkey PRIMARY KEY (payment_id_new)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional foreign key
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetExtraForeignKey(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE category ADD CONSTRAINT FK_category_language FOREIGN KEY (language_id) REFERENCES language (language_id)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a foreign key
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetMissingForeignKey(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE inventory.film_actor DROP CONSTRAINT film_actor_film_id_fkey");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a foreign key that references a different column
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetForeignKeyReferencesDifferentColumn(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE customer_data.address DROP CONSTRAINT address_city_id_fkey;");
            sb.AppendLine("ALTER TABLE customer_data.address ADD CONSTRAINT address_city_id_fkey FOREIGN KEY (city_id) REFERENCES category (category_id) ON UPDATE CASCADE ON DELETE RESTRICT");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a foreign key with different options
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetForeignKeyDifferentOptions(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE customer_data.address DROP CONSTRAINT address_city_id_fkey;");
            sb.AppendLine("ALTER TABLE customer_data.address ADD CONSTRAINT address_city_id_fkey FOREIGN KEY (city_id) REFERENCES customer_data.city (city_id) ON UPDATE RESTRICT ON DELETE NO ACTION");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a schema with different owner
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixturePostgreSql.ServerPorts), MemberType = typeof(DatabaseFixturePostgreSql))]
        [IntegrationTest]
        [Category("PostgreSQL")]
        public void MigratePostgreSqlDatabaseTargetSchemaDifferentOwner(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DO $do$");
            sb.AppendLine("BEGIN");
            sb.AppendLine("   IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'sakila_user_test') THEN");
            sb.AppendLine("      CREATE ROLE sakila_user_test LOGIN PASSWORD 'test1234';");
            sb.AppendLine("   END IF;");
            sb.AppendLine("END");
            sb.AppendLine("$do$;");
            sb.AppendLine("ALTER SCHEMA business OWNER TO sakila_user_test;");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.PostgreSql, sb.ToString(), port);
        }
    }
}
