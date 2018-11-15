using System;
using System.IO;
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
            var clonedDatabaseName = $"tcx_test_{Guid.NewGuid():N}";

            try
            {
                var postgresqldbp = this.dbFixture.GetPostgreSqlDatabaseProvider(databaseName);
                var db = postgresqldbp.GetDatabase(new TaskInfo("test"));

                var scripterFactory = new DatabaseScripterFactory(this.LoggerFactory);
                var scripter = scripterFactory.Create(db, new TiCodeX.SQLSchemaCompare.Core.Entities.Project.ProjectOptions());
                var fullScript = scripter.GenerateFullCreateScript(db);

                this.dbFixture.DropAndCreatePostgreSqlDatabase(clonedDatabaseName);

                var postgresqldbpo = this.dbFixture.GetPostgreSqlDatabaseProviderOptions(clonedDatabaseName);
                using (var context = new PostgreSqlDatabaseContext(this.LoggerFactory, this.cipherService, postgresqldbpo))
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("SET check_function_bodies = false;");
                    sb.AppendLine(fullScript);

                    if (this.exportGeneratedFullScript)
                    {
                        File.WriteAllText("c:\\temp\\FullScriptPostgreSQL.sql", sb.ToString());
                    }

                    context.ExecuteNonQuery(sb.ToString());
                }

                this.dbFixture.CompareDatabases(DatabaseType.PostgreSql, clonedDatabaseName, databaseName);
            }
            finally
            {
                this.dbFixture.DropPostgreSqlDatabase(clonedDatabaseName);
            }
        }

        /// <summary>
        /// Test migration script when source db is empty (a.k.a. drop whole target)
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseSourceEmpty()
        {
            var sourceDatabaseName = $"tcx_test_{Guid.NewGuid():N}";
            var targetDatabaseName = $"tcx_test_{Guid.NewGuid():N}";

            try
            {
                // Create the empty database
                this.dbFixture.DropAndCreatePostgreSqlDatabase(sourceDatabaseName);

                // Create the database with sakila to be migrated to empty
                this.dbFixture.CreatePostgreSqlSakilaDatabase(targetDatabaseName);

                this.dbFixture.ExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sourceDatabaseName, targetDatabaseName, "FullDropScriptPostgreSQL.sql");
            }
            finally
            {
                this.dbFixture.DropPostgreSqlDatabase(sourceDatabaseName);
                this.dbFixture.DropPostgreSqlDatabase(targetDatabaseName);
            }
        }

        /// <summary>
        /// Test migration script when target db is empty (a.k.a. re-create whole source)
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetEmpty()
        {
            const string sourceDatabaseName = "sakila";
            var targetDatabaseName = $"tcx_test_{Guid.NewGuid():N}";

            try
            {
                this.dbFixture.DropAndCreatePostgreSqlDatabase(targetDatabaseName);

                this.dbFixture.ExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sourceDatabaseName, targetDatabaseName);
            }
            finally
            {
                this.dbFixture.DropPostgreSqlDatabase(targetDatabaseName);
            }
        }

        /// <summary>
        /// Test migration script when target db doesn't have a column
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetMissingColumn()
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE staff DROP COLUMN password");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have an extra column
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetExtraColumn()
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE staff ADD middle_name VARCHAR(45) NOT NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a column with different type
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetColumnDifferentType()
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE country ALTER COLUMN last_update TYPE TEXT");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a different view
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetDifferentView()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP VIEW customer_list;");
            sb.AppendLine("CREATE VIEW customer_list AS SELECT NULL AS \"test\";");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a different function
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetDifferentFunction()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP FUNCTION film_in_stock;");
            sb.AppendLine("CREATE FUNCTION film_in_stock(p_film_id integer, p_store_id integer, OUT p_film_count integer) RETURNS SETOF integer");
            sb.AppendLine("    LANGUAGE sql");
            sb.AppendLine("AS $BODY$");
            sb.AppendLine("     SELECT inventory_id");
            sb.AppendLine("     FROM inventory");
            sb.AppendLine("     WHERE film_id = $1");
            sb.AppendLine("     AND store_id = $2;");
            sb.AppendLine("$BODY$;");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a different sequence
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetDifferentSequence()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER SEQUENCE customer_customer_id_seq");
            sb.AppendLine("INCREMENT BY 5");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a different type
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetDifferentType()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP TYPE bug_status;");
            sb.AppendLine("CREATE TYPE bug_status AS ENUM ('open', 'closed');");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db doesn't have a index
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetMissingIndex()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX idx_actor_last_name");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have an additional index
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetExtraIndex()
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE UNIQUE INDEX idx_staff_email ON staff (email)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db doesn't have a trigger
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetMissingTrigger()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP TRIGGER film_fulltext_trigger ON film");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have an additional trigger
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetExtraTrigger()
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE TRIGGER last_updated2 BEFORE DELETE ON store FOR EACH ROW EXECUTE PROCEDURE last_updated();");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }
    }
}
