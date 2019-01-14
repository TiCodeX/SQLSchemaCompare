using System;
using System.Linq;
using System.Text;
using FluentAssertions;
using SQLSchemaCompare.Test;
using TiCodeX.SQLSchemaCompare.Core.Entities;
using TiCodeX.SQLSchemaCompare.Core.Enums;
using TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace TiCodeX.SQLSchemaCompare.Test.Integration
{
    /// <summary>
    /// Integration tests for Microsoft SQL
    /// </summary>
    public class MicrosoftSqlTests : BaseTests<MicrosoftSqlTests>, IClassFixture<DatabaseFixtureMicrosoftSql>
    {
        private readonly DatabaseFixtureMicrosoftSql dbFixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper</param>
        /// <param name="dbFixture">The database fixture</param>
        public MicrosoftSqlTests(ITestOutputHelper output, DatabaseFixtureMicrosoftSql dbFixture)
            : base(output)
        {
            this.dbFixture = dbFixture;
            this.dbFixture.SetLoggerFactory(this.LoggerFactory);
            this.dbFixture.InitServers(DatabaseFixtureMicrosoftSql.ServerPorts);
        }

        /// <summary>
        /// Test the retrieval of database list
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void GetMicrosoftSqlDatabaseList(short port)
        {
            var mssqldbp = this.dbFixture.GetDatabaseProvider(string.Empty, port);
            var dbList = mssqldbp.GetDatabaseList();
            dbList.Should().Contain("msdb");
            dbList.Should().Contain("master");
            dbList.Should().Contain("sakila");
        }

        /// <summary>
        /// Test the retrieval of the sakila database
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void GetMicrosoftSqlSakilaDatabase(short port)
        {
            var mssqldbp = this.dbFixture.GetDatabaseProvider("sakila", port);
            var db = mssqldbp.GetDatabase(new TaskInfo("test"));
            db.Should().NotBeNull();
            db.Name.Should().Be("sakila");

            db.Schemas.Count.Should().Be(3);

            db.Tables.Count.Should().Be(18);
            var table = db.Tables.FirstOrDefault(x => x.Name == "film");
            table.Should().NotBeNull();
            table?.Columns.Count.Should().Be(13);
            table?.Columns.Select(x => x.Name).Should().Contain("language_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "film_category");
            table.Should().NotBeNull();
            table?.PrimaryKeys.Count.Should().Be(1);
            table?.PrimaryKeys.First().ColumnNames.Should().Contain("film_id");
            table?.PrimaryKeys.First().ColumnNames.Should().Contain("category_id");

            table = db.Tables.FirstOrDefault(x => x.Name == "rental");
            table?.ForeignKeys.Count.Should().Be(3);
            table?.ForeignKeys.Should().Contain(x => x.Name == "fk_rental_customer");
            table?.ForeignKeys.Should().Contain(x => x.Name == "fk_rental_inventory");
            table?.ForeignKeys.Should().Contain(x => x.Name == "fk_rental_staff");

            db.Views.Count.Should().Be(6);
            db.Views.Should().ContainSingle(x => x.Name == "film_list");

            db.Functions.Count.Should().Be(2);
            db.StoredProcedures.Count.Should().Be(1);

            db.DataTypes.Should().NotBeNullOrEmpty();
            db.DataTypes.Count.Should().Be(37);

            if (db.ServerVersion.Major < 11)
            {
                db.Sequences.Should().BeNullOrEmpty();
            }
            else
            {
                db.Sequences.Count.Should().Be(1);
            }

            db.Users.Count.Should().Be(3);
        }

        /// <summary>
        /// Test cloning MicrosoftSQL 'sakila' database
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void CloneMicrosoftSqlDatabase(short port)
        {
            const string databaseName = "sakila";
            var clonedDatabaseName = $"tcx_test_{Guid.NewGuid():N}";

            try
            {
                var mssqldbp = this.dbFixture.GetDatabaseProvider(databaseName, port);
                var db = mssqldbp.GetDatabase(new TaskInfo("test"));

                var scripterFactory = new DatabaseScripterFactory(this.LoggerFactory);
                var scripter = scripterFactory.Create(db, new TiCodeX.SQLSchemaCompare.Core.Entities.Project.ProjectOptions());
                var fullScript = scripter.GenerateFullCreateScript(db);

                this.dbFixture.DropAndCreateDatabase(clonedDatabaseName, port);

                this.dbFixture.ExecuteScript(fullScript, clonedDatabaseName, port);

                this.dbFixture.CompareDatabases(DatabaseType.MicrosoftSql, clonedDatabaseName, databaseName, port);
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
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseSourceEmpty(short port)
        {
            var sourceDatabaseName = $"tcx_test_{Guid.NewGuid():N}";
            var targetDatabaseName = $"tcx_test_{Guid.NewGuid():N}";

            try
            {
                // Create the empty database
                this.dbFixture.DropAndCreateDatabase(sourceDatabaseName, port);

                // Create the database with sakila to be migrated to empty
                this.dbFixture.CreateSakilaDatabase(targetDatabaseName, port);

                this.dbFixture.ExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sourceDatabaseName, targetDatabaseName, port);
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
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetEmpty(short port)
        {
            const string sourceDatabaseName = "sakila";
            var targetDatabaseName = $"tcx_test_{Guid.NewGuid():N}";

            try
            {
                this.dbFixture.DropAndCreateDatabase(targetDatabaseName, port);

                this.dbFixture.ExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sourceDatabaseName, targetDatabaseName, port);
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
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetMissingColumn(short port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE business.staff DROP COLUMN last_name");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a column
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetMissingColumns(short port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE business.staff DROP COLUMN last_name, username");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an extra column
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetExtraColumn(short port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE business.staff ADD middle_name VARCHAR(45) NOT NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an extra column
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetExtraColumns(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE business.staff ADD middle_name VARCHAR(45) NOT NULL");
            sb.AppendLine("ALTER TABLE business.staff ADD middle_name2 VARCHAR(45) NOT NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a column with different type
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetColumnDifferentType(short port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER TABLE customer_data.country ALTER COLUMN country NVARCHAR(80) NOT NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different view
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentView(short port)
        {
            var sb = new StringBuilder();
            sb.Append("ALTER VIEW customer_list AS SELECT NULL AS 'test'");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different function
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentFunction(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER FUNCTION StripWWWandCom (@Input VARCHAR(250))");
            sb.AppendLine("RETURNS VARCHAR(250)");
            sb.AppendLine("AS BEGIN");
            sb.AppendLine("    DECLARE @Work VARCHAR(250)");
            sb.AppendLine();
            sb.AppendLine("    SET @Work = @Input");
            sb.AppendLine();
            sb.AppendLine("    SET @Work = REPLACE(@Work, 'www.', '')");
            sb.AppendLine("    SET @Work = REPLACE(@Work, '.net', '')");
            sb.AppendLine();
            sb.AppendLine("    RETURN @Work");
            sb.AppendLine("END");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different stored procedure
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentStoredProcedure(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER PROCEDURE uspGetAddress @City nvarchar(30) = NULL");
            sb.AppendLine("AS");
            sb.AppendLine("SELECT *");
            sb.AppendLine("FROM city");
            sb.AppendLine("WHERE city = @City");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different sequence
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentSequence(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER SEQUENCE actor_seq");
            sb.AppendLine("INCREMENT BY 5");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different type
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentType(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP TYPE custom_decimal");
            sb.AppendLine("CREATE TYPE custom_decimal FROM DECIMAL(21, 6) NULL");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a index
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetMissingIndex(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX idx_uq ON business.rental");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional index
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetExtraIndex(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE INDEX idx_replacement_cost ON inventory.film (replacement_cost)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different filtered index
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentIndexFilter(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX idx_actor_last_name ON customer_data.actor");
            sb.AppendLine("CREATE INDEX idx_actor_last_name ON customer_data.actor(last_name) WHERE (first_name IS NOT NULL)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different index type
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentIndexType(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP INDEX idx_fk_address_id ON business.store");
            sb.AppendLine("CREATE CLUSTERED INDEX idx_fk_address_id ON business.store(manager_staff_id)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a trigger
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetMissingTrigger(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DROP TRIGGER customer_data.reminder1");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional trigger
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetExtraTrigger(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE TRIGGER reminder2");
            sb.AppendLine("ON customer_data.country");
            sb.AppendLine("AFTER DELETE");
            sb.AppendLine("AS RAISERROR ('BLABLABLA', 16, 10)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different trigger
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentTrigger(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TRIGGER customer_data.reminder1");
            sb.AppendLine("ON customer_data.country");
            sb.AppendLine("AFTER INSERT, DELETE");
            sb.AppendLine("AS RAISERROR ('test', 2, 10)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a default constraint
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetMissingDefaultConstraint(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE customer_data.actor DROP CONSTRAINT DF_actor_last_update");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional default constraint
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetExtraDefaultConstraint(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE customer_data.actor ADD CONSTRAINT DF_actor_last_name DEFAULT 'test' FOR last_name");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different default constraint
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentDefaultConstraint(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE customer_data.actor DROP CONSTRAINT DF_actor_last_update");
            sb.AppendLine("ALTER TABLE customer_data.actor ADD CONSTRAINT DF_actor_last_update DEFAULT (GETDATE()-444) FOR last_update");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a check constraint
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetMissingCheckConstraint(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE inventory.film DROP CONSTRAINT CHECK_special_features");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional check constraint
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetExtraCheckConstraint(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE customer_data.customer ADD CONSTRAINT check_email CHECK(email LIKE '_%@_%._%')");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a different check constraint
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetDifferentCheckConstraint(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE inventory.film DROP CONSTRAINT CHECK_special_features");
            sb.AppendLine("ALTER TABLE inventory.film ADD CONSTRAINT CHECK_special_features CHECK(special_features IS NOT null)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a primary key
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetMissingPrimaryKey(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE inventory.film_actor_description DROP CONSTRAINT PK_film_actor_description_film_actor_description_id");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional primary key
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetExtraPrimaryKey(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE inventory.film_text ADD CONSTRAINT PK_film_text_film_id PRIMARY KEY NONCLUSTERED (film_id)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional primary key with a foreing key that reference it
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetExtraPrimaryKeyWithReferencingForeignKey(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE inventory.film_text ADD CONSTRAINT PK_film_text_film_id PRIMARY KEY NONCLUSTERED (film_id)");
            sb.AppendLine("ALTER TABLE inventory.film_text_extra ADD CONSTRAINT FK_film_text_extra_film FOREIGN KEY (film_id) REFERENCES inventory.film_text (film_id) ON DELETE CASCADE");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port, 2);
        }

        /// <summary>
        /// Test migration script when target db have an primary key on a different colum
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetPrimaryKeyOnDifferentColumn(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE business.payment DROP CONSTRAINT PK_payment_payment_id");
            sb.AppendLine("ALTER TABLE business.payment ADD CONSTRAINT PK_payment_payment_id PRIMARY KEY NONCLUSTERED (payment_id_new)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have an additional foreign key
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetExtraForeignKey(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE category ADD CONSTRAINT FK_category_language FOREIGN KEY (language_id) REFERENCES language (language_id)");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db doesn't have a foreign key
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetMissingForeignKey(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE inventory.film_actor DROP CONSTRAINT fk_film_actor_film");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a foreign key that references a different column
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetForeignKeyReferencesDifferentColumn(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE customer_data.address DROP CONSTRAINT fk_address_city");
            sb.AppendLine("ALTER TABLE customer_data.address ADD CONSTRAINT fk_address_city FOREIGN KEY (city_id) REFERENCES customer_data.actor (actor_id) ON DELETE NO ACTION ON UPDATE CASCADE");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a foreign key with different options
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetForeignKeyDifferentOptions(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER TABLE customer_data.address DROP CONSTRAINT fk_address_city");
            sb.AppendLine("ALTER TABLE customer_data.address ADD CONSTRAINT fk_address_city FOREIGN KEY (city_id) REFERENCES customer_data.city (city_id) ON DELETE CASCADE ON UPDATE NO ACTION");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }

        /// <summary>
        /// Test migration script when target db have a schema with different owner
        /// </summary>
        /// <param name="port">The port of the server</param>
        [Theory]
        [MemberData(nameof(DatabaseFixtureMicrosoftSql.ServerPorts), MemberType = typeof(DatabaseFixtureMicrosoftSql))]
        [IntegrationTest]
        public void MigrateMicrosoftSqlDatabaseTargetSchemaDifferentOwner(short port)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER AUTHORIZATION ON SCHEMA::[business] TO [guest]");
            this.dbFixture.AlterTargetDatabaseExecuteFullAlterScriptAndCompare(DatabaseType.MicrosoftSql, sb.ToString(), port);
        }
    }
}
