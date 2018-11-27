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
        /// Test migration script when target db have a column without the default
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetColumnMissingDefault()
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE customer ALTER COLUMN activebool DROP DEFAULT");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a column with the default
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetColumnExtraDefault()
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE customer ALTER COLUMN last_name SET DEFAULT 'MyLastName'");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a column with a different default
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetColumnDifferentDefault()
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE film ALTER COLUMN replacement_cost SET DEFAULT 11.45");
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
        /// Test migration script when target db have a different index
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetDifferentIndex()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX idx_unq_manager_staff_id;");
            sb.AppendLine("CREATE INDEX idx_unq_manager_staff_id ON store USING btree (address_id);");
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

        /// <summary>
        /// Test migration script when target db have a different trigger
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetDifferentTrigger()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP TRIGGER last_updated ON actor;");
            sb.AppendLine("CREATE TRIGGER last_updated AFTER INSERT ON actor FOR EACH ROW EXECUTE PROCEDURE last_updated();");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db doesn't have a check constraint
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetMissingCheckConstraint()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE payment_p2017_01 DROP CONSTRAINT payment_p2017_01_payment_date_check");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have an additional check constraint
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetExtraCheckConstraint()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE staff ADD CONSTRAINT staff_check_email CHECK(email LIKE '_%@_%._%')");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a different check constraint
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetDifferentCheckConstraint()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE payment_p2017_01 DROP CONSTRAINT payment_p2017_01_payment_date_check;");
            sb.AppendLine("ALTER TABLE payment_p2017_01 ADD CONSTRAINT payment_p2017_01_payment_date_check CHECK (((payment_date >= '2017-01-01 00:00:00'::timestamp without time zone) AND (payment_date < '2017-01-31 00:00:00'::timestamp without time zone)));");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db doesn't have a primary key
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetMissingPrimaryKey()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE payment DROP CONSTRAINT payment_pkey");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have an additional primary key
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetExtraPrimaryKey()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE film_text ADD CONSTRAINT PK_film_text_film_id PRIMARY KEY (film_id)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have an additional primary key with a foreing key that reference it
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetExtraPrimaryKeyWithReferencingForeignKey()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE film_text ADD CONSTRAINT PK_film_text_film_id PRIMARY KEY (film_id);");
            sb.AppendLine("ALTER TABLE film_category ADD CONSTRAINT FK_film_category_extra_film FOREIGN KEY (film_text_id) REFERENCES film_text (film_id) ON DELETE CASCADE");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString(), 2);
        }

        /// <summary>
        /// Test migration script when target db have an primary key on a different colum
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetPrimaryKeyOnDifferentColumn()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE payment DROP CONSTRAINT payment_pkey;");
            sb.AppendLine("ALTER TABLE payment ADD CONSTRAINT payment_pkey PRIMARY KEY (payment_id_new)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have an additional foreign key
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetExtraForeignKey()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE category ADD CONSTRAINT FK_category_language FOREIGN KEY (language_id) REFERENCES language (language_id)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db doesn't have a foreign key
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetMissingForeignKey()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE film_actor DROP CONSTRAINT film_actor_film_id_fkey");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a foreign key that references a different column
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetForeignKeyReferencesDifferentColumn()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE address DROP CONSTRAINT address_city_id_fkey;");
            sb.AppendLine("ALTER TABLE address ADD CONSTRAINT address_city_id_fkey FOREIGN KEY (city_id) REFERENCES category (category_id) ON UPDATE CASCADE ON DELETE RESTRICT");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }

        /// <summary>
        /// Test migration script when target db have a foreign key with different options
        /// </summary>
        [Fact]
        [IntegrationTest]
        public void MigratePostgreSqlDatabaseTargetForeignKeyDifferentOptions()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE address DROP CONSTRAINT address_city_id_fkey;");
            sb.AppendLine("ALTER TABLE address ADD CONSTRAINT address_city_id_fkey FOREIGN KEY (city_id) REFERENCES city (city_id) ON UPDATE RESTRICT ON DELETE NO ACTION");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.PostgreSql, sb.ToString());
        }
    }
}
