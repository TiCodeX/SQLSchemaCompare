using System;
using System.IO;
using System.Text;
using FluentAssertions;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Enums;
using SQLCompare.Infrastructure.SqlScripters;
using SQLCompare.Services;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace SQLCompare.Test.Integration
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
            const string clonedDatabaseName = "sakila_clone";

            var mysqldbp = this.dbFixture.GetMySqlDatabaseProvider(databaseName);
            var db = mysqldbp.GetDatabase(new TaskInfo("test"));

            var scripterFactory = new DatabaseScripterFactory(this.LoggerFactory);
            var scripter = scripterFactory.Create(db, new SQLCompare.Core.Entities.Project.ProjectOptions());
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

        /// <summary>
        /// Test migration script when source db is empty (a.k.a. re-create whole source)
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMySqlDatabaseTargetEmpty()
        {
            const string sourceDatabaseName = "sakila";
            const string targetDatabaseName = "sakila_migrated_from_empty";

            this.dbFixture.DropAndCreateMySqlDatabase(targetDatabaseName);

            // Perform the compare
            var projectService = new ProjectService(null, this.LoggerFactory);
            projectService.NewProject(DatabaseType.MySql);
            projectService.Project.SourceProviderOptions = this.dbFixture.GetMySqlDatabaseProviderOptions(sourceDatabaseName);
            projectService.Project.TargetProviderOptions = this.dbFixture.GetMySqlDatabaseProviderOptions(targetDatabaseName);
            this.dbFixture.PerformCompareAndWaitResult(projectService);
            projectService.Project.Result.FullAlterScript.Should().NotBeNullOrWhiteSpace();

            // Execute the full alter script
            var mySqlFullAlterScript = new StringBuilder();
            mySqlFullAlterScript.AppendLine("SET @OLD_UNIQUE_CHECKS =@@UNIQUE_CHECKS, UNIQUE_CHECKS = 0;");
            mySqlFullAlterScript.AppendLine("SET @OLD_FOREIGN_KEY_CHECKS =@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS = 0;");
            mySqlFullAlterScript.AppendLine("SET @OLD_SQL_MODE =@@SQL_MODE, SQL_MODE = 'TRADITIONAL';");
            mySqlFullAlterScript.AppendLine($"USE {targetDatabaseName};");
            mySqlFullAlterScript.AppendLine(projectService.Project.Result.FullAlterScript);
            mySqlFullAlterScript.AppendLine("SET SQL_MODE = @OLD_SQL_MODE;");
            mySqlFullAlterScript.AppendLine("SET FOREIGN_KEY_CHECKS = @OLD_FOREIGN_KEY_CHECKS;");
            mySqlFullAlterScript.AppendLine("SET UNIQUE_CHECKS = @OLD_UNIQUE_CHECKS;");

            var pathMySql = Path.GetTempFileName();
            File.WriteAllText(pathMySql, mySqlFullAlterScript.ToString());
            this.dbFixture.ExecuteMySqlScript(pathMySql);

            this.dbFixture.CompareDatabases(DatabaseType.MySql, targetDatabaseName, sourceDatabaseName);
        }
    }
}
