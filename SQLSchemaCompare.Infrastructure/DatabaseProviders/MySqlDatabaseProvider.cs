using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using TiCodeX.SQLSchemaCompare.Core.Entities;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database.MySql;
using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;
using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;
using TiCodeX.SQLSchemaCompare.Infrastructure.EntityFramework;

namespace TiCodeX.SQLSchemaCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves various information from a MySQL Server
    /// </summary>
    internal class MySqlDatabaseProvider
        : ADatabaseProvider<MySqlDatabaseProviderOptions, MySqlDatabaseContext, MySqlDb>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlDatabaseProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="cipherService">The injected cipher service</param>
        /// <param name="options">The options to connect to the MySQL Database</param>
        public MySqlDatabaseProvider(ILoggerFactory loggerFactory, ICipherService cipherService, MySqlDatabaseProviderOptions options)
            : base(loggerFactory, loggerFactory.CreateLogger(nameof(MySqlDatabaseProvider)), cipherService, options)
        {
        }

        /// <inheritdoc />
        public override ABaseDb GetDatabase(TaskInfo taskInfo)
        {
            using (var context = new MySqlDatabaseContext(this.LoggerFactory, this.CipherService, this.Options))
            {
                return this.DiscoverDatabase(context, taskInfo);
            }
        }

        /// <inheritdoc />
        public override List<string> GetDatabaseList()
        {
            using (var context = new MySqlDatabaseContext(this.LoggerFactory, this.CipherService, this.Options))
            {
                return context.QuerySingleColumn<string>("SHOW DATABASES");
            }
        }

        /// <inheritdoc/>
        protected override string GetServerVersion(MySqlDatabaseContext context)
        {
            return context.QuerySingleColumn<string>("SELECT VERSION()").FirstOrDefault() ?? string.Empty;
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbSchema> GetSchemas(MySqlDatabaseContext context)
        {
            // An empty list is returned because MySQL doesn't have schemas
            return Enumerable.Empty<ABaseDbSchema>();
        }

        /// <inheritdoc />
        protected override IEnumerable<ABaseDbTable> GetTables(MySqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT t.TABLE_NAME as Name,");
            query.AppendLine("       t.ENGINE as Engine,");
            query.AppendLine("       t.UPDATE_TIME as ModifyDate,");
            query.AppendLine("       c.CHARACTER_SET_NAME as TableCharacterSet");
            query.AppendLine("FROM INFORMATION_SCHEMA.TABLES t");
            query.AppendLine("INNER JOIN INFORMATION_SCHEMA.COLLATION_CHARACTER_SET_APPLICABILITY c ON c.collation_name = t.table_collation");
            query.AppendLine($"WHERE t.TABLE_TYPE = 'BASE TABLE' and t.TABLE_SCHEMA = '{context.DatabaseName}'");

            return context.Query<MySqlTable>(query.ToString());
        }

        /// <inheritdoc />
        protected override IEnumerable<ABaseDbColumn> GetColumns(MySqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT a.TABLE_NAME as TableName,");
            query.AppendLine("       a.COLUMN_NAME as Name,");
            query.AppendLine("       CAST(a.ORDINAL_POSITION AS SIGNED) as OrdinalPosition,");
            query.AppendLine("       a.COLUMN_DEFAULT as ColumnDefault,");
            query.AppendLine("       a.IS_NULLABLE as IsNullable,");
            query.AppendLine("       a.DATA_TYPE as DataType,");
            query.AppendLine("       a.CHARACTER_SET_NAME as CharacterSetName,");
            query.AppendLine("       a.COLLATION_NAME as CollationName,");

            if (this.CurrentServerVersion.Major >= 5 && this.CurrentServerVersion.Minor >= 7)
            {
                query.AppendLine("       a.GENERATION_EXPRESSION as GenerationExpression,");
            }

            query.AppendLine("       a.EXTRA as Extra,");
            query.AppendLine("       a.COLUMN_TYPE as ColumnType");
            query.AppendLine("FROM INFORMATION_SCHEMA.COLUMNS a");
            query.AppendLine($"WHERE TABLE_SCHEMA = '{context.DatabaseName}'");
            return context.Query<MySqlColumn>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbPrimaryKey> GetPrimaryKeys(MySqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT s.index_name AS Name,");
            query.AppendLine("       s.TABLE_NAME AS 'TableName',");
            query.AppendLine("       s.COLUMN_NAME AS 'ColumnName',");
            query.AppendLine("       CAST(s.SEQ_IN_INDEX AS SIGNED) AS 'OrdinalPosition',");
            query.AppendLine("       CASE WHEN s.COLLATION = 'D' THEN TRUE ELSE FALSE END as 'IsDescending',");
            query.AppendLine("       COALESCE(tc.CONSTRAINT_TYPE, 'INDEX') AS 'ConstraintType'");
            query.AppendLine("FROM INFORMATION_SCHEMA.STATISTICS s");
            query.AppendLine("LEFT OUTER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc");
            query.AppendLine("  ON tc.table_name = s.table_name AND tc.table_schema = s.table_schema AND tc.constraint_name = s.index_name");
            query.AppendLine($"WHERE s.table_schema = '{context.DatabaseName}' AND COALESCE(tc.CONSTRAINT_TYPE, 'INDEX') = 'PRIMARY KEY'");

            return context.Query<ABaseDbPrimaryKey>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbForeignKey> GetForeignKeys(MySqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT kcu.CONSTRAINT_NAME as Name,");
            query.AppendLine("       kcu.TABLE_NAME as TableName,");
            query.AppendLine("       kcu.COLUMN_NAME as ColumnName,");
            query.AppendLine("       tc.constraint_type as ConstraintType,");
            query.AppendLine("       CAST(kcu.ORDINAL_POSITION AS SIGNED) as OrdinalPosition,");
            query.AppendLine("       null as ReferencedTableSchema,");
            query.AppendLine("       kcu.REFERENCED_TABLE_NAME as ReferencedTableName,");
            query.AppendLine("       kcu.REFERENCED_COLUMN_NAME as ReferencedColumnName,");
            query.AppendLine("       rc.UPDATE_RULE as UpdateRule,");
            query.AppendLine("       rc.DELETE_RULE as DeleteRule");
            query.AppendLine("FROM information_schema.key_column_usage kcu");
            query.AppendLine("INNER JOIN information_schema.table_constraints tc ON tc.table_name = kcu.table_name AND tc.table_schema = kcu.table_schema AND tc.constraint_name = kcu.constraint_name");
            query.AppendLine("INNER JOIN information_schema.REFERENTIAL_CONSTRAINTS rc ON rc.constraint_catalog = kcu.constraint_catalog AND rc.constraint_schema = kcu.constraint_schema AND rc.constraint_name = kcu.constraint_name ");
            query.AppendLine($"WHERE kcu.TABLE_SCHEMA = '{context.DatabaseName}' AND tc.constraint_type = 'FOREIGN KEY'");

            return context.Query<MySqlForeignKey>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbConstraint> GetConstraints(MySqlDatabaseContext context)
        {
            // An empty list is returned because:
            // - CHECK constraints doesn't exist
            // - UNIQUE constraints are the same things as UNIQUE indexes
            return Enumerable.Empty<ABaseDbConstraint>();
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbIndex> GetIndexes(MySqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT s.index_name AS Name,");
            query.AppendLine("       s.TABLE_NAME AS 'TableName',");
            query.AppendLine("       s.COLUMN_NAME AS 'ColumnName',");
            query.AppendLine("       CAST(s.SEQ_IN_INDEX AS SIGNED) AS 'OrdinalPosition',");
            query.AppendLine("       CASE WHEN s.COLLATION = 'D' THEN TRUE ELSE FALSE END as 'IsDescending',");
            query.AppendLine("       s.INDEX_TYPE AS 'IndexType',");
            query.AppendLine("       COALESCE(tc.CONSTRAINT_TYPE, 'INDEX') AS 'ConstraintType'");
            query.AppendLine("FROM INFORMATION_SCHEMA.STATISTICS s");
            query.AppendLine("LEFT OUTER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc");
            query.AppendLine("  ON tc.table_name = s.table_name AND tc.table_schema = s.table_schema AND tc.constraint_name = s.index_name");
            query.AppendLine($"WHERE s.table_schema = '{context.DatabaseName}' AND COALESCE(tc.CONSTRAINT_TYPE, 'INDEX') != 'PRIMARY KEY'");

            return context.Query<MySqlIndex>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbView> GetViews(MySqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT TABLE_NAME as Name");
            query.AppendLine("FROM INFORMATION_SCHEMA.VIEWS");
            query.AppendLine($"WHERE TABLE_SCHEMA = '{context.DatabaseName}'");

            var result = context.Query<MySqlView>(query.ToString());
            foreach (var view in result)
            {
                view.ViewDefinition = context.QuerySingleColumn<string>($"SHOW CREATE VIEW `{context.DatabaseName}`.`{view.Name}`", 1).FirstOrDefault();
            }

            return result;
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbFunction> GetFunctions(MySqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT ROUTINE_NAME as Name");
            query.AppendLine("FROM INFORMATION_SCHEMA.ROUTINES");
            query.AppendLine($"WHERE routine_type = 'FUNCTION' and ROUTINE_SCHEMA = '{context.DatabaseName}'");

            var result = context.Query<MySqlFunction>(query.ToString());
            foreach (var function in result)
            {
                function.Definition = context.QuerySingleColumn<string>($"SHOW CREATE FUNCTION `{context.DatabaseName}`.`{function.Name}`", 2).FirstOrDefault();
            }

            return result;
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbStoredProcedure> GetStoredProcedures(MySqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT ROUTINE_NAME as Name");
            query.AppendLine("FROM INFORMATION_SCHEMA.ROUTINES");
            query.AppendLine($"WHERE routine_type = 'PROCEDURE' and ROUTINE_SCHEMA = '{context.DatabaseName}'");

            var result = context.Query<MySqlStoredProcedure>(query.ToString());
            foreach (var procedure in result)
            {
                procedure.Definition = context.QuerySingleColumn<string>($"SHOW CREATE PROCEDURE `{context.DatabaseName}`.`{procedure.Name}`", 2).FirstOrDefault();
            }

            return result;
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbTrigger> GetTriggers(MySqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT t.TRIGGER_NAME AS 'Name',");
            query.AppendLine("       t.EVENT_OBJECT_TABLE AS 'TableName'");
            query.AppendLine("FROM INFORMATION_SCHEMA.TRIGGERS t");
            query.AppendLine($"WHERE t.TRIGGER_SCHEMA = '{context.DatabaseName}'");

            var result = context.Query<MySqlTrigger>(query.ToString());
            foreach (var trigger in result)
            {
                trigger.Definition = context.QuerySingleColumn<string>($"SHOW CREATE TRIGGER `{context.DatabaseName}`.`{trigger.Name}`", 2).FirstOrDefault();
            }

            return result;
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbDataType> GetDataTypes(MySqlDatabaseContext context)
        {
            // An empty list is returned because MySQL doesn't have user defined data types
            return Enumerable.Empty<ABaseDbDataType>();
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbSequence> GetSequences(MySqlDatabaseContext context)
        {
            // An empty list is returned because MySQL doesn't have sequences
            return Enumerable.Empty<ABaseDbSequence>();
        }
    }
}