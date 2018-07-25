using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.MySql;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Infrastructure.EntityFramework;

namespace SQLCompare.Infrastructure.DatabaseProviders
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
        /// <param name="options">The options to connect to the MySQL Database</param>
        public MySqlDatabaseProvider(ILoggerFactory loggerFactory, MySqlDatabaseProviderOptions options)
            : base(loggerFactory, loggerFactory.CreateLogger(nameof(MySqlDatabaseProvider)), options)
        {
        }

        /// <inheritdoc />
        public override ABaseDb GetDatabase()
        {
            using (var context = new MySqlDatabaseContext(this.LoggerFactory, this.Options))
            {
                return this.DiscoverDatabase(context);
            }
        }

        /// <inheritdoc />
        public override List<string> GetDatabaseList()
        {
            using (var context = new MySqlDatabaseContext(this.LoggerFactory, this.Options))
            {
                return context.Query("SHOW DATABASES");
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ABaseDbTable> GetTables(MySqlDb database, MySqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT TABLE_NAME as Name,");
            query.AppendLine("       TABLE_CATALOG as 'Catalog',");
            query.AppendLine("       TABLE_SCHEMA as 'Schema',");
            query.AppendLine("       ENGINE as Engine,");
            query.AppendLine("       VERSION as Version,");
            query.AppendLine("       ROW_FORMAT as RowFormat,");
            query.AppendLine("       AUTO_INCREMENT as AutoIncrement,");
            query.AppendLine("       CREATE_TIME as CreateDate,");
            query.AppendLine("       UPDATE_TIME as ModifyDate,");
            query.AppendLine("       TABLE_COLLATION as TableCollation,");
            query.AppendLine("       CREATE_OPTIONS as CreateOptions,");
            query.AppendLine("       TABLE_COMMENT as TableComment");
            query.AppendLine("FROM INFORMATION_SCHEMA.TABLES");
            query.AppendLine($"WHERE TABLE_TYPE = 'BASE TABLE' and TABLE_SCHEMA = '{database.Name}'");

            return context.Query<MySqlTable>(query.ToString());
        }

        /// <inheritdoc />
        protected override IEnumerable<ABaseDbColumn> GetColumns(MySqlDb database, MySqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT a.TABLE_CATALOG as 'Catalog',");
            query.AppendLine("       a.TABLE_SCHEMA as 'Schema',");
            query.AppendLine("       a.TABLE_NAME as TableName,");
            query.AppendLine("       a.COLUMN_NAME as Name,");
            query.AppendLine("       a.ORDINAL_POSITION as OrdinalPosition,");
            query.AppendLine("       a.COLUMN_DEFAULT as ColumnDefault,");
            query.AppendLine("       a.IS_NULLABLE as IsNullable,");
            query.AppendLine("       a.DATA_TYPE as DataType,");
            query.AppendLine("       a.CHARACTER_MAXIMUM_LENGTH as CharacterMaxLenght,");
            query.AppendLine("       a.CHARACTER_OCTET_LENGTH as CharacterOctetLenght,");
            query.AppendLine("       a.NUMERIC_SCALE as NumericScale,");
            query.AppendLine("       a.DATETIME_PRECISION as DateTimePrecision,");
            query.AppendLine("       a.CHARACTER_SET_NAME as CharacterSetName,");
            query.AppendLine("       a.COLLATION_NAME as CollationName,");
            query.AppendLine("       a.SRS_ID as SrsId,");
            query.AppendLine("       a.PRIVILEGES as Privileges,");
            query.AppendLine("       a.NUMERIC_PRECISION as NumericPrecision,");
            query.AppendLine("       a.GENERATION_EXPRESSION as GenerationExpression,");
            query.AppendLine("       a.EXTRA as Extra,");
            query.AppendLine("       a.COLUMN_TYPE as ColumnType,");
            query.AppendLine("       a.COLUMN_KEY as ColumnKey,");
            query.AppendLine("       a.COLUMN_COMMENT as ColumnComment");
            query.AppendLine("FROM INFORMATION_SCHEMA.COLUMNS a");
            query.AppendLine($"WHERE TABLE_SCHEMA = '{database.Name}'");
            return context.Query<MySqlColumn>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbConstraint> GetForeignKeys(MySqlDb database, MySqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT kcu.CONSTRAINT_CATALOG as 'Catalog',");
            query.AppendLine("       kcu.CONSTRAINT_SCHEMA as 'Schema',");
            query.AppendLine("       kcu.CONSTRAINT_NAME as Name,");
            query.AppendLine("       kcu.TABLE_CATALOG as TableCatalog,");
            query.AppendLine("       kcu.TABLE_SCHEMA as TableSchema,");
            query.AppendLine("       kcu.TABLE_NAME as TableName,");
            query.AppendLine("       kcu.COLUMN_NAME as ColumnName,");
            query.AppendLine("       kcu.ORDINAL_POSITION as OrdinalPosition,");
            query.AppendLine("       kcu.POSITION_IN_UNIQUE_CONSTRAINT as PositionInUniqueConstraint,");
            query.AppendLine("       kcu.REFERENCED_TABLE_SCHEMA as ReferencedTableSchema,");
            query.AppendLine("       kcu.REFERENCED_TABLE_NAME as ReferencedTableName,");
            query.AppendLine("       kcu.REFERENCED_COLUMN_NAME as ReferencedColumnName,");
            query.AppendLine("       rc.MATCH_OPTION as MatchOption,");
            query.AppendLine("       rc.UPDATE_RULE as UpdateRule,");
            query.AppendLine("       rc.DELETE_RULE as DeleteRule");
            query.AppendLine("FROM information_schema.key_column_usage kcu");
            query.AppendLine("INNER JOIN information_schema.table_constraints tc ON tc.table_name = kcu.table_name and tc.table_schema = kcu.table_schema and tc.constraint_name = kcu.constraint_name");
            query.AppendLine("INNER JOIN information_schema.REFERENTIAL_CONSTRAINTS rc ON rc.constraint_name = kcu.constraint_name ");
            query.AppendLine($"WHERE kcu.TABLE_SCHEMA = '{database.Name}' AND tc.constraint_type = 'FOREIGN KEY'");

            return context.Query<MySqlForeignKey>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbIndex> GetIndexes(MySqlDb database, MySqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT s.TABLE_CATALOG AS 'Catalog',");
            query.AppendLine("       s.TABLE_SCHEMA AS 'Schema',");
            query.AppendLine("       s.index_name AS Name,");
            query.AppendLine("       s.TABLE_CATALOG AS 'TableCatalog',");
            query.AppendLine("       s.TABLE_SCHEMA AS 'TableSchema',");
            query.AppendLine("       s.TABLE_NAME AS 'TableName',");
            query.AppendLine("       s.COLUMN_NAME AS 'ColumnName',");
            query.AppendLine("       CASE WHEN tc.CONSTRAINT_TYPE = 'PRIMARY KEY' THEN TRUE ELSE FALSE END AS 'IsPrimaryKey',");
            query.AppendLine("       s.SEQ_IN_INDEX AS 'OrdinalPosition',");
            query.AppendLine("       CASE WHEN s.COLLATION = 'D' THEN TRUE ELSE FALSE END as 'IsDescending',");
            query.AppendLine("       s.INDEX_TYPE AS 'IndexType',");
            query.AppendLine("       tc.CONSTRAINT_TYPE AS 'ConstraintType'");
            query.AppendLine("FROM INFORMATION_SCHEMA.STATISTICS s");
            query.AppendLine("LEFT OUTER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc");
            query.AppendLine("  ON tc.table_name = s.table_name AND tc.table_schema = s.table_schema AND tc.constraint_name = s.index_name");
            query.AppendLine($"WHERE s.table_schema = '{database.Name}'");

            return context.Query<MySqlIndex>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbView> GetViews(MySqlDb database, MySqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT TABLE_NAME as Name,");
            query.AppendLine("       TABLE_CATALOG as 'Catalog',");
            query.AppendLine("       TABLE_SCHEMA as 'Schema'");
            query.AppendLine("FROM INFORMATION_SCHEMA.VIEWS");
            query.AppendLine($"WHERE TABLE_SCHEMA = '{database.Name}'");

            var result = context.Query<MySqlView>(query.ToString());
            foreach (var view in result)
            {
                var createView = context.Query($"SHOW CREATE VIEW {view.Schema}.{view.Name}", 1).FirstOrDefault();
                view.ViewDefinition = createView;
            }

            return result;
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbRoutine> GetFunctions(MySqlDb database, MySqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT ROUTINE_NAME as Name,");
            query.AppendLine("       ROUTINE_CATALOG as 'Catalog',");
            query.AppendLine("       ROUTINE_SCHEMA as 'Schema'");
            query.AppendLine("FROM INFORMATION_SCHEMA.ROUTINES");
            query.AppendLine($"WHERE routine_type = 'FUNCTION' and ROUTINE_SCHEMA = '{database.Name}'");

            var result = context.Query<MySqlFunction>(query.ToString());
            foreach (var function in result)
            {
                var createFunction = context.Query($"SHOW CREATE FUNCTION {function.Schema}.{function.Name}", 2).FirstOrDefault();
                function.RoutineDefinition = createFunction;
            }

            return result;
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbRoutine> GetStoreProcedures(MySqlDb database, MySqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT ROUTINE_NAME as Name,");
            query.AppendLine("       ROUTINE_CATALOG as 'Catalog',");
            query.AppendLine("       ROUTINE_SCHEMA as 'Schema'");
            query.AppendLine("FROM INFORMATION_SCHEMA.ROUTINES");
            query.AppendLine($"WHERE routine_type = 'PROCEDURE' and ROUTINE_SCHEMA = '{database.Name}'");

            var result = context.Query<MySqlStoreProcedure>(query.ToString());
            foreach (var procedure in result)
            {
                var createProcedure = context.Query($"SHOW CREATE PROCEDURE {procedure.Schema}.{procedure.Name}", 2).FirstOrDefault();
                procedure.RoutineDefinition = createProcedure;
            }

            return result;
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbObject> GetDataTypes(MySqlDb database, MySqlDatabaseContext context)
        {
            return Enumerable.Empty<ABaseDbObject>();
        }
    }
}