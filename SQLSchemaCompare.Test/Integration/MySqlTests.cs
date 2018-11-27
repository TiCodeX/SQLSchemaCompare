using System;
using System.IO;
using System.Text;
using TiCodeX.SQLSchemaCompare.Core.Entities;
using TiCodeX.SQLSchemaCompare.Core.Enums;
using TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace TiCodeX.SQLSchemaCompare.Test.Integration
{
    /// <summary>
    /// Integration tests for MySQL
    /// </summary>
    public class MySqlTests : BaseTests<MySqlTests>, IClassFixture<DatabaseFixture>
    {
        private readonly bool exportGeneratedFullScript = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ExportGeneratedFullScript"));
        private readonly DatabaseFixture dbFixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        /// <param name="dbFixture">The database fixture</param>
        public MySqlTests(ITestOutputHelper output, DatabaseFixture dbFixture)
            : base(output)
        {
            this.dbFixture = dbFixture;
            this.dbFixture.SetLoggerFactory(this.LoggerFactory);
        }

        /// <summary>
        /// Test cloning MySQL 'sakila' database
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void CloneMySqlDatabase()
        {
            const string databaseName = "sakila";
            var clonedDatabaseName = $"tcx_test_{Guid.NewGuid():N}";

            try
            {
                var mysqldbp = this.dbFixture.GetMySqlDatabaseProvider(databaseName);
                var db = mysqldbp.GetDatabase(new TaskInfo("test"));

                var scripterFactory = new DatabaseScripterFactory(this.LoggerFactory);
                var scripter = scripterFactory.Create(db, new TiCodeX.SQLSchemaCompare.Core.Entities.Project.ProjectOptions());
                var fullScript = scripter.GenerateFullCreateScript(db);

                if (this.exportGeneratedFullScript)
                {
                    File.WriteAllText("c:\\temp\\FullScriptMySQL.sql", fullScript);
                }

                this.dbFixture.DropAndCreateMySqlDatabase(clonedDatabaseName);

                var mySqlFullScript = new StringBuilder();
                mySqlFullScript.AppendLine("SET @OLD_UNIQUE_CHECKS =@@UNIQUE_CHECKS, UNIQUE_CHECKS = 0;");
                mySqlFullScript.AppendLine("SET @OLD_FOREIGN_KEY_CHECKS =@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS = 0;");
                mySqlFullScript.AppendLine("SET @OLD_SQL_MODE =@@SQL_MODE, SQL_MODE = 'TRADITIONAL';");
                mySqlFullScript.AppendLine($"USE {clonedDatabaseName};");
                mySqlFullScript.AppendLine(fullScript.Replace($"`{databaseName}`.`", $"`{clonedDatabaseName}`.`", StringComparison.InvariantCulture));
                mySqlFullScript.AppendLine("SET SQL_MODE = @OLD_SQL_MODE;");
                mySqlFullScript.AppendLine("SET FOREIGN_KEY_CHECKS = @OLD_FOREIGN_KEY_CHECKS;");
                mySqlFullScript.AppendLine("SET UNIQUE_CHECKS = @OLD_UNIQUE_CHECKS;");

                var pathMySql = Path.GetTempFileName();
                File.WriteAllText(pathMySql, mySqlFullScript.ToString());
                this.dbFixture.ExecuteMySqlScript(pathMySql);

                this.dbFixture.CompareDatabases(DatabaseType.MySql, clonedDatabaseName, databaseName);
            }
            finally
            {
                this.dbFixture.DropMySqlDatabase(clonedDatabaseName);
            }
        }

        /// <summary>
        /// Test migration script when source db is empty (a.k.a. drop whole target)
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseSourceEmpty()
        {
            var sourceDatabaseName = $"tcx_test_{Guid.NewGuid():N}";
            var targetDatabaseName = $"tcx_test_{Guid.NewGuid():N}";

            try
            {
                // Create the empty database
                this.dbFixture.DropAndCreateMySqlDatabase(sourceDatabaseName);

                // Create the database with sakila to be migrated to empty
                this.dbFixture.CreateMySqlSakilaDatabase(targetDatabaseName);

                this.dbFixture.ExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sourceDatabaseName, targetDatabaseName, "FullDropScriptMySQL.sql");
            }
            finally
            {
                this.dbFixture.DropMySqlDatabase(sourceDatabaseName);
                this.dbFixture.DropMySqlDatabase(targetDatabaseName);
            }
        }

        /// <summary>
        /// Test migration script when target db is empty (a.k.a. re-create whole source)
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetEmpty()
        {
            const string sourceDatabaseName = "sakila";
            var targetDatabaseName = $"tcx_test_{Guid.NewGuid():N}";

            try
            {
                this.dbFixture.DropAndCreateMySqlDatabase(targetDatabaseName);

                this.dbFixture.ExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sourceDatabaseName, targetDatabaseName);
            }
            finally
            {
                this.dbFixture.DropMySqlDatabase(targetDatabaseName);
            }
        }

        /// <summary>
        /// Test migration script when target db doesn't have a column
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetMissingColumn()
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE staff DROP COLUMN last_name");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have an extra column
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetExtraColumn()
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE staff ADD middle_name VARCHAR(45) NOT NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a column with different type
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetColumnDifferentType()
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE country CHANGE COLUMN country country JSON NOT NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a column without the default
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetColumnMissingDefault()
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE country ALTER COLUMN country DROP DEFAULT");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a column with the default
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetColumnExtraDefault()
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE actor ALTER COLUMN last_name SET DEFAULT 'MyLastName'");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a column with different default
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetColumnDifferentDefault()
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE country ALTER COLUMN country SET DEFAULT 'test'");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a different view
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetDifferentView()
        {
            var sb = new StringBuilder();
            sb.Append("ALTER VIEW customer_list AS SELECT NULL AS 'test'");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a different function
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetDifferentFunction()
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
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a different stored procedure
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetDifferentStoredProcedure()
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
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db doesn't have a index
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetMissingIndex()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX idx_last_name ON customer");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have an additional index
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetExtraIndex()
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE UNIQUE INDEX idx_staff_email ON staff (email)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a different index
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetDifferentIndex()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX idx_last_name ON customer;");
            sb.AppendLine("CREATE UNIQUE INDEX idx_last_name ON customer(last_name);");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db doesn't have a trigger
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetMissingTrigger()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP TRIGGER upd_film");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have an additional trigger
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetExtraTrigger()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DELIMITER ;;");
            sb.AppendLine("CREATE TRIGGER ins_film_text AFTER INSERT ON film_text FOR EACH ROW BEGIN");
            sb.AppendLine("   DELETE FROM film_actor WHERE film_id = new.film_id;");
            sb.AppendLine("END;;");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a different trigger
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetDifferentTrigger()
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
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db doesn't have a primary key
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetMissingPrimaryKey()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE payment MODIFY payment_id SMALLINT UNSIGNED NOT NULL;");
            sb.AppendLine("ALTER TABLE payment DROP PRIMARY KEY");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have an additional primary key
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetExtraPrimaryKey()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE film_actor_description ADD PRIMARY KEY (film_actor_description_id)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have an additional primary key with a foreing key that reference it
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetExtraPrimaryKeyWithReferencingForeignKey()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE film_actor_description ADD PRIMARY KEY (film_actor_description_id);");
            sb.AppendLine("ALTER TABLE film_actor ADD CONSTRAINT FK_film_film_actor_description FOREIGN KEY (film_actor_description_id) REFERENCES film_actor_description (film_actor_description_id) ON DELETE CASCADE");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString(), 2);
        }

        /// <summary>
        /// Test migration script when target db have an primary key on a different colum
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetPrimaryKeyOnDifferentColumn()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE payment MODIFY payment_id SMALLINT UNSIGNED NOT NULL;");
            sb.AppendLine("ALTER TABLE payment DROP PRIMARY KEY;");
            sb.AppendLine("ALTER TABLE payment ADD CONSTRAINT PK_payment_payment_id PRIMARY KEY (payment_id_new);");
            sb.AppendLine("ALTER TABLE payment MODIFY payment_id_new SMALLINT UNSIGNED NOT NULL AUTO_INCREMENT;");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have an additional foreign key
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetExtraForeignKey()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE film_text ADD CONSTRAINT FK_film_text_category FOREIGN KEY (category_id) REFERENCES category (category_id);");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db doesn't have a foreign key
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetMissingForeignKey()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE film_actor DROP FOREIGN KEY fk_film_actor_film;");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a foreign key that references a different column
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetForeignKeyReferencesDifferentColumn()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE address DROP FOREIGN KEY fk_address_city;");
            sb.AppendLine("ALTER TABLE address ADD CONSTRAINT fk_address_city FOREIGN KEY (city_id) REFERENCES actor (actor_id) ON DELETE RESTRICT ON UPDATE CASCADE");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a foreign key with different options
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetForeignKeyDifferentOptions()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE address DROP FOREIGN KEY fk_address_city;");
            sb.AppendLine("ALTER TABLE address ADD CONSTRAINT fk_address_city FOREIGN KEY (city_id) REFERENCES city (city_id) ON DELETE CASCADE ON UPDATE NO ACTION");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MySql, sb.ToString());
        }
    }
}
