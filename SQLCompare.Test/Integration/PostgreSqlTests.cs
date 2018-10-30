using System;
using System.IO;
using System.Text;
using FluentAssertions;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Enums;
using SQLCompare.Core.Interfaces.Services;
using SQLCompare.Infrastructure.EntityFramework;
using SQLCompare.Infrastructure.SqlScripters;
using SQLCompare.Services;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace SQLCompare.Test.Integration
{
    /// <summary>
    /// Integration tests for PostgreSQL
    /// </summary>
    public class PostgreSqlTests : BaseTests<PostgreSqlTests>, IClassFixture<DatabaseFixture>
    {
        private readonly bool exportGeneratedFullScript = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ExportGeneratedFullScript"));
        private readonly ICipherService cipherService = new CipherService();
        private readonly DatabaseFixture dbFixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        /// <param name="dbFixture">The database fixture</param>
        public PostgreSqlTests(ITestOutputHelper output, DatabaseFixture dbFixture)
            : base(output)
        {
            this.dbFixture = dbFixture;
            this.dbFixture.SetLoggerFactory(this.LoggerFactory);
        }

        /// <summary>
        /// Test cloning PostgreSQL 'sakila' database
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void ClonePostgreSqlDatabase()
        {
            const string databaseName = "sakila";
            const string clonedDatabaseName = "sakila_clone";

            var postgresqldbp = this.dbFixture.GetPostgreSqlDatabaseProvider(databaseName);
            var db = postgresqldbp.GetDatabase(new TaskInfo("test"));

            var scripterFactory = new DatabaseScripterFactory(this.LoggerFactory);
            var scripter = scripterFactory.Create(db, new SQLCompare.Core.Entities.Project.ProjectOptions());
            var fullScript = scripter.GenerateFullCreateScript(db);

            if (this.exportGeneratedFullScript)
            {
                File.WriteAllText("c:\\temp\\FullScriptPostgreSQL.sql", fullScript);
            }

            this.dbFixture.DropAndCreatePostgreSqlDatabase(clonedDatabaseName);

            var postgresqldbpo = this.dbFixture.GetPostgreSqlDatabaseProviderOptions(clonedDatabaseName);
            using (var context = new PostgreSqlDatabaseContext(this.LoggerFactory, this.cipherService, postgresqldbpo))
            {
                var sb = new StringBuilder();
                sb.AppendLine("SET check_function_bodies = false;");

                var firstViewFound = false;
                foreach (var line in fullScript.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                {
                    if (line.Contains("CREATE VIEW", StringComparison.Ordinal) && !firstViewFound)
                    {
                        // TODO: implement create aggregate in scripter
                        sb.AppendLine("CREATE AGGREGATE group_concat(text)(");
                        sb.AppendLine("    SFUNC = _group_concat,");
                        sb.AppendLine("    STYPE = text");
                        sb.AppendLine(");");
                        firstViewFound = true;
                    }

                    sb.AppendLine(line);
                }

                context.ExecuteNonQuery(sb.ToString());
            }

            this.dbFixture.CompareDatabases(DatabaseType.PostgreSql, clonedDatabaseName, databaseName);
        }

        /// <summary>
        /// Test migration script when target db is empty (a.k.a. re-create whole source)
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetEmpty()
        {
            const string sourceDatabaseName = "sakila";
            const string targetDatabaseName = "sakila_migrated_from_empty";

            this.dbFixture.DropAndCreatePostgreSqlDatabase(targetDatabaseName);

            // Perform the compare
            var projectService = new ProjectService(null, this.LoggerFactory);
            projectService.NewProject(DatabaseType.PostgreSql);
            projectService.Project.SourceProviderOptions = this.dbFixture.GetPostgreSqlDatabaseProviderOptions(sourceDatabaseName);
            projectService.Project.TargetProviderOptions = this.dbFixture.GetPostgreSqlDatabaseProviderOptions(targetDatabaseName);
            this.dbFixture.PerformCompareAndWaitResult(projectService);
            projectService.Project.Result.FullAlterScript.Should().NotBeNullOrWhiteSpace();

            // Execute the full alter script
            var postgresqldbpo = this.dbFixture.GetPostgreSqlDatabaseProviderOptions(targetDatabaseName);
            using (var context = new PostgreSqlDatabaseContext(this.LoggerFactory, this.cipherService, postgresqldbpo))
            {
                var sb = new StringBuilder();
                sb.AppendLine("SET check_function_bodies = false;");

                var firstViewFound = false;
                foreach (var line in projectService.Project.Result.FullAlterScript.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                {
                    if (line.Contains("CREATE VIEW", StringComparison.Ordinal) && !firstViewFound)
                    {
                        // TODO: implement create aggregate in scripter
                        sb.AppendLine("CREATE AGGREGATE group_concat(text)(");
                        sb.AppendLine("    SFUNC = _group_concat,");
                        sb.AppendLine("    STYPE = text");
                        sb.AppendLine(");");
                        firstViewFound = true;
                    }

                    sb.AppendLine(line);
                }

                context.ExecuteNonQuery(sb.ToString());
            }

            this.dbFixture.CompareDatabases(DatabaseType.PostgreSql, targetDatabaseName, sourceDatabaseName);
        }
    }
}
