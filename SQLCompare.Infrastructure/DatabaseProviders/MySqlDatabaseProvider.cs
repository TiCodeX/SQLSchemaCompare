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
    internal class MySqlDatabaseProvider : ADatabaseProvider<MySqlDatabaseProviderOptions>
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
        public override BaseDb GetDatabase()
        {
            using (var context = new MySqlDatabaseContext(this.LoggerFactory, this.Options))
            {
                MySqlDb db = new MySqlDb() { Name = this.Options.Database };

                var tables = GetTables(db.Name, context);

                foreach (var table in tables)
                {
                    table.Columns.AddRange(GetColumns(table, context));
                }

                db.Tables.AddRange(tables);
                return db;
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

        private static List<MySqlTable> GetTables(string databaseName, MySqlDatabaseContext context)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT TABLE_NAME as Name,");
            query.AppendLine("       TABLE_CATALOG as CatalogName,");
            query.AppendLine("       TABLE_SCHEMA as SchemaName,");
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
            query.AppendLine($"WHERE TABLE_TYPE = 'BASE TABLE' and TABLE_SCHEMA = '{databaseName}'");

            return context.Query<MySqlTable>(query.ToString());
        }

        private static List<MySqlColumn> GetColumns(BaseDbTable table, MySqlDatabaseContext context)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT a.COLUMN_NAME as Name,");
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
            query.AppendLine($"WHERE TABLE_SCHEMA = '{table.SchemaName}' and TABLE_NAME = '{table.Name}' and TABLE_CATALOG = '{table.CatalogName}'");
            return context.Query<MySqlColumn>(query.ToString());
        }
    }
}
