using System;
using System.Linq;
using System.Text;
using FluentAssertions;
using TiCodeX.SQLSchemaCompare.Core.Entities;
using TiCodeX.SQLSchemaCompare.Core.Enums;
using TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace TiCodeX.SQLSchemaCompare.Test.Integration
{
    /// <summary>
    /// Integration tests for MariaDB
    /// </summary>
    public class MariaDbTests : BaseTests<MariaDbTests>, IClassFixture<DatabaseFixtureMariaDb>
    {
        private readonly DatabaseFixtureMariaDb dbFixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="MariaDbTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        /// <param name="dbFixture">The database fixture</param>
        public MariaDbTests(ITestOutputHelper output, DatabaseFixtureMariaDb dbFixture)
            : base(output)
        {
            if (dbFixture == null)
            {
                throw new ArgumentNullException(nameof(dbFixture));
            }

            this.dbFixture = dbFixture;
            this.dbFixture.SetLoggerFactory(this.LoggerFactory);
            this.dbFixture.InitServers(DatabaseFixtureMariaDb.ServerPorts);
        }

        /// <summary>
        /// Test the retrieval of database list
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void GetMariaDbDatabaseList(ushort port)
        {
            var mariadbdbp = this.dbFixture.GetDatabaseProvider(string.Empty, port);
            var dbList = mariadbdbp.GetDatabaseList();
            dbList.Should().Contain("mysql");
            dbList.Should().Contain("sakila");
        }

        /// <summary>
        /// Test the retrieval of the  MariaDB 'sakila' database
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void GetMariaDbSakilaDatabase(ushort port)
        {
            var mariadbdbp = this.dbFixture.GetDatabaseProvider("sakila", port);
            var db = mariadbdbp.GetDatabase(new TaskInfo("test"));
            db.Should().NotBeNull();
            db.Name.Should().Be("sakila");

            db.DataTypes.Should().BeEmpty();

            db.Tables.Count.Should().Be(17);
            var table = db.Tables.FirstOrDefault(x => x.Name == "film");
            table.Should().NotBeNull();
            table?.Columns.Count.Should().Be(13);
            table?.Columns.Select(x => x.Name).Should().Contain("language_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "film_actor");
            table.Should().NotBeNull();
            table?.PrimaryKeys.Count.Should().Be(1);
            table?.PrimaryKeys.First().ColumnNames.Should().Contain("actor_id");
            table?.PrimaryKeys.First().ColumnNames.Should().Contain("film_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "payment");
            table?.ForeignKeys.Count.Should().Be(3);
            table?.ForeignKeys.Should().Contain(x => x.Name == "fk_payment_customer");
            table?.ForeignKeys.Should().Contain(x => x.Name == "fk_payment_rental");
            table?.ForeignKeys.Should().Contain(x => x.Name == "fk_payment_staff");

            db.Views.Count.Should().Be(7);
            db.Views.Should().ContainSingle(x => x.Name == "film_list");

            db.Functions.Count.Should().Be(3);
            db.Functions.Should().ContainSingle(x => x.Name == "get_customer_balance");

            db.StoredProcedures.Count.Should().Be(3);
            db.StoredProcedures.Should().ContainSingle(x => x.Name == "film_in_stock");
        }

        /// <summary>
        /// Test cloning MariaDB 'sakila' database
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void CloneMariaDbDatabase(ushort port)
        {
            const string databaseName = "sakila";
            var clonedDatabaseName = DatabaseFixture.GenerateDatabaseName();

            try
            {
                var mariadbdbp = this.dbFixture.GetDatabaseProvider(databaseName, port);
                var db = mariadbdbp.GetDatabase(new TaskInfo("test"));

                var scripterFactory = new DatabaseScripterFactory(this.LoggerFactory);
                var scripter = scripterFactory.Create(db, new TiCodeX.SQLSchemaCompare.Core.Entities.Project.ProjectOptions());
                var fullScript = scripter.GenerateFullCreateScript(db);

                this.dbFixture.DropAndCreateDatabase(clonedDatabaseName, port);

                this.dbFixture.ExecuteScript(fullScript, clonedDatabaseName, port);

                this.dbFixture.CompareDatabases(DatabaseType.MariaDb, clonedDatabaseName, databaseName, port);
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
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseSourceEmpty(ushort port)
        {
            var sourceDatabaseName = DatabaseFixture.GenerateDatabaseName();
            var targetDatabaseName = DatabaseFixture.GenerateDatabaseName();

            try
            {
                // Create the empty database
                this.dbFixture.DropAndCreateDatabase(sourceDatabaseName, port);

                // Create the database with sakila to be migrated to empty
                this.dbFixture.CreateSakilaDatabase(targetDatabaseName, port);

                this.dbFixture.ExecuteFullAlterScriptAndCompare(DatabaseType.MariaDb, sourceDatabaseName, targetDatabaseName, port);
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
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetEmpty(ushort port)
        {
            const string sourceDatabaseName = "sakila";
            var targetDatabaseName = DatabaseFixture.GenerateDatabaseName();

            try
            {
                this.dbFixture.DropAndCreateDatabase(targetDatabaseName, port);

                this.dbFixture.ExecuteFullAlterScriptAndCompare(DatabaseType.MariaDb, sourceDatabaseName, targetDatabaseName, port);
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
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetMissingColumn(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE staff DROP COLUMN email");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a column
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetMissingColumns(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE staff DROP COLUMN email,");
            sb.Append("DROP COLUMN picture");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an extra column
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetExtraColumn(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE staff ADD COLUMN middle_name VARCHAR(45) NOT NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an extra column
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetExtraColumns(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE staff ADD COLUMN middle_name VARCHAR(45) NOT NULL,");
            sb.Append("ADD COLUMN middle_name2 VARCHAR(45) NOT NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a column with different type
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetColumnDifferentType(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE country CHANGE COLUMN country country char NOT NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a column without the default
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetColumnMissingDefault(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE country ALTER COLUMN country DROP DEFAULT");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a column with the default
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetColumnExtraDefault(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE actor ALTER COLUMN last_name SET DEFAULT 'MyLastName'");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a column with different default
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetColumnDifferentDefault(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE country ALTER COLUMN country SET DEFAULT 'test'");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different view
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetDifferentView(ushort port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER VIEW customer_list AS SELECT NULL AS 'test'");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different function
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetDifferentFunction(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP FUNCTION inventory_held_by_customer;");
            sb.AppendLine("DELIMITER $$$$");
            sb.AppendLine("CREATE FUNCTION `inventory_held_by_customer`(p_inventory_id INT) RETURNS INT");
            sb.AppendLine("    READS SQL DATA");
            sb.AppendLine("BEGIN");
            sb.AppendLine("  DECLARE v_customer_id INT;");
            sb.AppendLine("  DECLARE EXIT HANDLER FOR NOT FOUND RETURN NULL;");
            sb.AppendLine();
            sb.AppendLine("  SELECT customer_id INTO v_customer_id");
            sb.AppendLine("  FROM rental");
            sb.AppendLine("  WHERE return_date IS NOT NULL");
            sb.AppendLine("  AND inventory_id = p_inventory_id;");
            sb.AppendLine();
            sb.AppendLine("  RETURN v_customer_id;");
            sb.AppendLine("END$$$$");
            sb.AppendLine("DELIMITER ;");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different stored procedure
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetDifferentStoredProcedure(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP PROCEDURE film_not_in_stock;");
            sb.AppendLine("DELIMITER $$$$");
            sb.AppendLine("CREATE PROCEDURE `film_not_in_stock`(IN p_film_id INT, IN p_store_id INT, OUT p_film_count INT)");
            sb.AppendLine("    READS SQL DATA");
            sb.AppendLine("BEGIN");
            sb.AppendLine("     SELECT inventory_id");
            sb.AppendLine("     FROM inventory");
            sb.AppendLine("     WHERE film_id = p_film_id");
            sb.AppendLine("     AND store_id = p_store_id;");
            sb.AppendLine("     SELECT FOUND_ROWS() INTO p_film_count;");
            sb.AppendLine("END$$$$");
            sb.AppendLine("DELIMITER ;");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a index
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetMissingIndex(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX idx_last_name ON customer");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional index
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetExtraIndex(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE UNIQUE INDEX idx_staff_email ON staff (email)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different index
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetDifferentIndex(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX idx_last_name ON customer;");
            sb.AppendLine("CREATE UNIQUE INDEX idx_last_name ON customer(last_name);");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a trigger
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetMissingTrigger(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP TRIGGER upd_film");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional trigger
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetExtraTrigger(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DELIMITER ;;");
            sb.AppendLine("CREATE TRIGGER ins_film_text AFTER INSERT ON film_text FOR EACH ROW BEGIN");
            sb.AppendLine("   DELETE FROM film_actor WHERE film_id = new.film_id;");
            sb.AppendLine("END;;");
            sb.AppendLine("DELIMITER ;");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different trigger
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetDifferentTrigger(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP TRIGGER upd_film;");
            sb.AppendLine("DELIMITER $$$$");
            sb.AppendLine("CREATE TRIGGER `upd_film` AFTER UPDATE ON `film` FOR EACH ROW BEGIN");
            sb.AppendLine("    IF (old.title != new.title) OR (old.description != new.description) OR (old.film_id != new.film_id)");
            sb.AppendLine("    THEN");
            sb.AppendLine("        UPDATE film_text");
            sb.AppendLine("            SET title=new.description,");
            sb.AppendLine("                description=new.title,");
            sb.AppendLine("                film_id=new.film_id");
            sb.AppendLine("        WHERE film_id=old.film_id;");
            sb.AppendLine("    END IF;");
            sb.AppendLine("  END$$$$");
            sb.AppendLine("DELIMITER ;");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a primary key
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetMissingPrimaryKey(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE payment MODIFY payment_id SMALLINT UNSIGNED NOT NULL;");
            sb.AppendLine("ALTER TABLE payment DROP PRIMARY KEY");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional primary key
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetExtraPrimaryKey(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE film_actor_description ADD PRIMARY KEY (film_actor_description_id)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional primary key with a foreing key that reference it
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetExtraPrimaryKeyWithReferencingForeignKey(ushort port)
        {
            // Skip test for MariaDB version 5.5 because adding a new foreign key it creates automatically an index and
            // since we skip loading the indexes of type 'FOREIGN KEY' it will fail because it doesn't get dropped.
            // Loading the 'FOREIGN KEY' indexes is not a solution because other tests will fail when trying to create those indexes.
            if (port == 29001)
            {
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE film_actor_description ADD PRIMARY KEY (film_actor_description_id);");
            sb.AppendLine("ALTER TABLE film_actor ADD CONSTRAINT FK_film_film_actor_description FOREIGN KEY (film_actor_description_id) REFERENCES film_actor_description (film_actor_description_id) ON DELETE CASCADE");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port, 2, true);
        }

        /// <summary>
        /// Test migration script when target db have an primary key on a different colum
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetPrimaryKeyOnDifferentColumn(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE payment MODIFY payment_id SMALLINT UNSIGNED NOT NULL;");
            sb.AppendLine("ALTER TABLE payment DROP PRIMARY KEY;");
            sb.AppendLine("ALTER TABLE payment ADD CONSTRAINT PK_payment_payment_id PRIMARY KEY (payment_id_new);");
            sb.AppendLine("ALTER TABLE payment MODIFY payment_id_new SMALLINT UNSIGNED NOT NULL AUTO_INCREMENT;");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional foreign key
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetExtraForeignKey(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE film_text ADD CONSTRAINT FK_film_text_category FOREIGN KEY (category_id) REFERENCES category (category_id);");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a foreign key
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetMissingForeignKey(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE film_actor DROP FOREIGN KEY fk_film_actor_film;");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a foreign key that references a different column
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetForeignKeyReferencesDifferentColumn(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE address DROP FOREIGN KEY fk_address_city;");
            sb.AppendLine("ALTER TABLE address ADD CONSTRAINT fk_address_city FOREIGN KEY (city_id) REFERENCES actor (actor_id) ON DELETE RESTRICT ON UPDATE CASCADE");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a foreign key with different options
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMariaDb.ServerPorts), MemberType = typeof(DatabaseFixtureMariaDb))]
        [IntegrationTest]
        [Category("MariaDB")]
        public void MigrateMariaDbDatabaseTargetForeignKeyDifferentOptions(ushort port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE address DROP FOREIGN KEY fk_address_city;");
            sb.AppendLine("ALTER TABLE address ADD CONSTRAINT fk_address_city FOREIGN KEY (city_id) REFERENCES city (city_id) ON DELETE CASCADE ON UPDATE NO ACTION");
            this.dbFixture.AlterTargetDatabaseExecuteFullAndAllAlterScriptsAndCompare(DatabaseType.MariaDb, sb.ToString(), port);
        }
    }
}
