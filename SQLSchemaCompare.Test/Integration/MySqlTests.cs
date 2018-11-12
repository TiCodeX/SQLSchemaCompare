﻿using System;
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
    }
}
