using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Infrastructure.EntityFramework;
using System.Collections.Generic;
using System.Text;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves various information from a MySQL Server
    /// </summary>
    internal class MySqlDatabaseProvider
        : ADatabaseProvider<MySqlDatabaseProviderOptions, MySqlDatabaseContext, MySqlDb, MySqlTable, MySqlColumn, MySqlPrimaryKey, MySqlForeignKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlDatabaseProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="options">The options to connect to the MySQL Database</param>
        public MySqlDatabaseProvider(ILoggerFactory loggerFactory, MySqlDatabaseProviderOptions options)
            : base(loggerFactory, loggerFactory.CreateLogger("MySqlDatabaseProvider"), options)
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
        protected override List<MySqlTable> GetTables(MySqlDb database, MySqlDatabaseContext context)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT TABLE_NAME as Name,");
            query.AppendLine("       TABLE_CATALOG as TableCatalog,");
            query.AppendLine("       TABLE_SCHEMA as TableSchema,");
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
        protected override List<MySqlColumn> GetColumns(MySqlDb database, MySqlDatabaseContext context)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT a.TABLE_CATALOG as TableCatalog,");
            query.AppendLine("       a.TABLE_SCHEMA as TableSchema,");
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
        protected override List<MySqlPrimaryKey> GetPrimaryKeys(MySqlDb database, MySqlDatabaseContext context)
        {
            return null;
        }

        /// <inheritdoc/>
        protected override List<MySqlForeignKey> GetForeignKeys(MySqlDb database, MySqlDatabaseContext context)
        {
            return null;
        }
    }
}
