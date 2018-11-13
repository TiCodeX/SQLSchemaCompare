using System;
using System.IO;
using System.Linq;
using System.Text;
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
            var clonedDatabaseName = $"tcx_test_{Guid.NewGuid():N}";

            try
            {
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
            finally
            {
                this.dbFixture.DropMicrosoftSqlDatabase(clonedDatabaseName);
            }
        }

        /// <summary>
        /// Test migration script when source db is empty (a.k.a. drop whole target)
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseSourceEmpty()
        {
            var sourceDatabaseName = $"tcx_test_{Guid.NewGuid():N}";
            var targetDatabaseName = $"tcx_test_{Guid.NewGuid():N}";

            try
            {
                // Create the empty database
                this.dbFixture.DropAndCreateMicrosoftSqlDatabase(sourceDatabaseName);

                // Create the database with sakila to be migrated to empty
                this.dbFixture.CreateMicrosoftSqlSakilaDatabase(targetDatabaseName);

                this.dbFixture.ExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sourceDatabaseName, targetDatabaseName, "FullDropScriptMicrosoftSQL.sql");
            }
            finally
            {
                this.dbFixture.DropMicrosoftSqlDatabase(sourceDatabaseName);
                this.dbFixture.DropMicrosoftSqlDatabase(targetDatabaseName);
            }
        }

        /// <summary>
        /// Test migration script when target db is empty (a.k.a. re-create whole source)
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetEmpty()
        {
            const string sourceDatabaseName = "sakila";
            var targetDatabaseName = $"tcx_test_{Guid.NewGuid():N}";

            try
            {
                this.dbFixture.DropAndCreateMicrosoftSqlDatabase(targetDatabaseName);

                this.dbFixture.ExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sourceDatabaseName, targetDatabaseName);
            }
            finally
            {
                this.dbFixture.DropMicrosoftSqlDatabase(targetDatabaseName);
            }
        }

        /// <summary>
        /// Test migration script when target db doesn't have a column
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetMissingColumn()
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE staff DROP COLUMN last_name");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have an extra column
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetExtraColumn()
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE staff ADD middle_name VARCHAR(45) NOT NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a column with different type
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetColumnDifferentType()
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE country ALTER COLUMN country NVARCHAR(80) NOT NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a different view
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentView()
        {
            var sb = new StringBuilder();
            sb.Append("ALTER VIEW customer_list AS SELECT NULL AS 'test'");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a different function
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentFunction()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER FUNCTION StripWWWandCom (@input VARCHAR(250))");
            sb.AppendLine("RETURNS VARCHAR(250)");
            sb.AppendLine("AS BEGIN");
            sb.AppendLine("    DECLARE @Work VARCHAR(250)");
            sb.AppendLine();
            sb.AppendLine("    SET @Work = @Input");
            sb.AppendLine();
            sb.AppendLine("    SET @Work = REPLACE(@Work, 'www.', '')");
            sb.AppendLine("    SET @Work = REPLACE(@Work, '.net', '')");
            sb.AppendLine();
            sb.AppendLine("    RETURN @work");
            sb.AppendLine("END");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a different stored procedure
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentStoredProcedure()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER PROCEDURE uspGetAddress @City nvarchar(30) = NULL");
            sb.AppendLine("AS");
            sb.AppendLine("SELECT *");
            sb.AppendLine("FROM city");
            sb.AppendLine("WHERE city = @City");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a different sequence
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentSequence()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER SEQUENCE actor_seq");
            sb.AppendLine("INCREMENT BY 5");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a different type
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentType()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP TYPE custom_decimal");
            sb.AppendLine("CREATE TYPE custom_decimal FROM DECIMAL(21, 6) NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db doesn't have a index
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetMissingIndex()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX idx_uq ON rental");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have an additional index
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetExtraIndex()
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE INDEX idx_replacement_cost ON film (replacement_cost)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db doesn't have a trigger
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetMissingTrigger()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP TRIGGER reminder1");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have an additional trigger
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetExtraTrigger()
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE TRIGGER reminder2");
            sb.AppendLine("ON dbo.country");
            sb.AppendLine("AFTER DELETE");
            sb.AppendLine("AS RAISERROR ('BLABLABLA', 16, 10)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString());
        }
    }
}
