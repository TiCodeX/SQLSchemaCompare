using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using TiCodeX.SQLSchemaCompare.Core.Entities;
using TiCodeX.SQLSchemaCompare.Core.Enums;
using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;
using TiCodeX.SQLSchemaCompare.Infrastructure.EntityFramework;
using TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters;
using TiCodeX.SQLSchemaCompare.Services;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace TiCodeX.SQLSchemaCompare.Test.Integration
{
    /// <summary>
    /// Integration tests for Microsoft SQL
    /// </summary>
    public class MicrosoftSqlTests : BaseTests<MicrosoftSqlTests>, IClassFixture<DatabaseFixture>
    {
        private readonly bool exportGeneratedFullScript = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ExportGeneratedFullScript"));
        private readonly ICipherService cipherService = new CipherService();
        private readonly DatabaseFixture dbFixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        /// <param name="dbFixture">The database fixture</param>
        public MicrosoftSqlTests(ITestOutputHelper output, DatabaseFixture dbFixture)
            : base(output)
        {
            this.dbFixture = dbFixture;
            this.dbFixture.SetLoggerFactory(this.LoggerFactory);
        }

        /// <summary>
        /// Test cloning MicrosoftSQL 'sakila' database
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void CloneMicrosoftSqlDatabase()
        {
            const string databaseName = "sakila";
            const string clonedDatabaseName = "sakila_clone";

            var mssqldbp = this.dbFixture.GetMicrosoftSqlDatabaseProvider(databaseName);
            var db = mssqldbp.GetDatabase(new TaskInfo("test"));

            var scripterFactory = new DatabaseScripterFactory(this.LoggerFactory);
            var scripter = scripterFactory.Create(db, new TiCodeX.SQLSchemaCompare.Core.Entities.Project.ProjectOptions());
            var fullScript = scripter.GenerateFullCreateScript(db);

            if (this.exportGeneratedFullScript)
            {
                File.WriteAllText("c:\\temp\\FullScriptMicrosoftSQL.sql", fullScript);
            }

            this.dbFixture.DropAndCreateMicrosoftSqlDatabase(clonedDatabaseName);

            var mssqldbpo = this.dbFixture.GetMicrosoftSqlDatabaseProviderOptions(clonedDatabaseName);
            using (var context = new MicrosoftSqlDatabaseContext(this.LoggerFactory, this.cipherService, mssqldbpo))
            {
                var queries = fullScript.Split(new[] { "GO" + Environment.NewLine }, StringSplitOptions.None);
                foreach (var query in queries.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    context.ExecuteNonQuery(query);
                }
            }

            this.dbFixture.CompareDatabases(DatabaseType.MicrosoftSql, clonedDatabaseName, databaseName);
        }

        /// <summary>
        /// Test migration script when source db is empty (a.k.a. drop whole target)
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseSourceEmpty()
        {
            const string sourceDatabaseName = "sakila_empty";
            const string targetDatabaseName = "sakila_migrated_to_empty";

            // Create the empty database
            this.dbFixture.DropAndCreateMicrosoftSqlDatabase(sourceDatabaseName);

            // Create the database with sakila to be migrated to empty
            this.dbFixture.CreateMicrosoftSqlSakilaDatabase(targetDatabaseName);

            // Perform the compare
            var projectService = new ProjectService(null, this.LoggerFactory);
            projectService.NewProject(DatabaseType.MicrosoftSql);
            projectService.Project.SourceProviderOptions = this.dbFixture.GetMicrosoftSqlDatabaseProviderOptions(sourceDatabaseName);
            projectService.Project.TargetProviderOptions = this.dbFixture.GetMicrosoftSqlDatabaseProviderOptions(targetDatabaseName);
            this.dbFixture.PerformCompareAndWaitResult(projectService);
            projectService.Project.Result.FullAlterScript.Should().NotBeNullOrWhiteSpace();

            if (this.exportGeneratedFullScript)
            {
                File.WriteAllText("c:\\temp\\FullDropScriptMicrosoftSQL.sql", projectService.Project.Result.FullAlterScript);
            }

            // Execute the full alter script
            var mssqldbpo = this.dbFixture.GetMicrosoftSqlDatabaseProviderOptions(targetDatabaseName);
            using (var context = new MicrosoftSqlDatabaseContext(this.LoggerFactory, this.cipherService, mssqldbpo))
            {
                var queries = projectService.Project.Result.FullAlterScript.Split(new[] { "GO" + Environment.NewLine }, StringSplitOptions.None);
                foreach (var query in queries.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    context.ExecuteNonQuery(query);
                }
            }

            this.dbFixture.CompareDatabases(DatabaseType.MicrosoftSql, targetDatabaseName, sourceDatabaseName);
        }

        /// <summary>
        /// Test migration script when target db is empty (a.k.a. re-create whole source)
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetEmpty()
        {
            const string sourceDatabaseName = "sakila";
            const string targetDatabaseName = "sakila_migrated_from_empty";

            this.dbFixture.DropAndCreateMicrosoftSqlDatabase(targetDatabaseName);

            // Perform the compare
            var projectService = new ProjectService(null, this.LoggerFactory);
            projectService.NewProject(DatabaseType.MicrosoftSql);
            projectService.Project.SourceProviderOptions = this.dbFixture.GetMicrosoftSqlDatabaseProviderOptions(sourceDatabaseName);
            projectService.Project.TargetProviderOptions = this.dbFixture.GetMicrosoftSqlDatabaseProviderOptions(targetDatabaseName);
            this.dbFixture.PerformCompareAndWaitResult(projectService);
            projectService.Project.Result.FullAlterScript.Should().NotBeNullOrWhiteSpace();

            // Execute the full alter script
            var mssqldbpo = this.dbFixture.GetMicrosoftSqlDatabaseProviderOptions(targetDatabaseName);
            using (var context = new MicrosoftSqlDatabaseContext(this.LoggerFactory, this.cipherService, mssqldbpo))
            {
                var queries = projectService.Project.Result.FullAlterScript.Split(new[] { "GO" + Environment.NewLine }, StringSplitOptions.None);
                foreach (var query in queries.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    context.ExecuteNonQuery(query);
                }
            }

            this.dbFixture.CompareDatabases(DatabaseType.MicrosoftSql, targetDatabaseName, sourceDatabaseName);
        }
    }
}
