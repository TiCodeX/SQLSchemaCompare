namespace TiCodeX.SQLSchemaCompare.Infrastructure.DatabaseProviders
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Database.MySql;
    using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;
    using TiCodeX.SQLSchemaCompare.Infrastructure.EntityFramework;

    /// <summary>
    /// Retrieves various information from a MariaDB Server
    /// </summary>
    internal class MariaDbDatabaseProvider : MySqlDatabaseProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MariaDbDatabaseProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="cipherService">The injected cipher service</param>
        /// <param name="options">The options to connect to the MariaDB Database</param>
        public MariaDbDatabaseProvider(ILoggerFactory loggerFactory, ICipherService cipherService, MariaDbDatabaseProviderOptions options)
            : base(loggerFactory, cipherService, new MySqlDatabaseProviderOptions { Hostname = options.Hostname, Port = options.Port, Username = options.Username, Password = options.Password, SavePassword = options.SavePassword, UseSsl = options.UseSsl, Database = options.Database })
        {
        }

        /// <inheritdoc/>
        protected override string GetServerVersion(MySqlDatabaseContext context)
        {
            return context.QuerySingleColumn<string>("SELECT SUBSTRING_INDEX(VERSION(), '-', 1)").FirstOrDefault() ?? string.Empty;
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
            query.AppendLine(this.CurrentServerVersion.Major >= 11 && this.CurrentServerVersion.Minor >= 4
                ? "INNER JOIN INFORMATION_SCHEMA.COLLATION_CHARACTER_SET_APPLICABILITY c ON c.full_collation_name = t.table_collation"
                : "INNER JOIN INFORMATION_SCHEMA.COLLATION_CHARACTER_SET_APPLICABILITY c ON c.collation_name = t.table_collation");
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

            query.AppendLine(this.CurrentServerVersion.Major > 10 || (this.CurrentServerVersion.Major == 10 && this.CurrentServerVersion.Minor >= 2)
                ? "       CASE WHEN a.COLUMN_DEFAULT = 'NULL' THEN NULL ELSE a.COLUMN_DEFAULT END as ColumnDefault,"
                : "       a.COLUMN_DEFAULT as ColumnDefault,");

            query.AppendLine("       a.IS_NULLABLE as IsNullable,");
            query.AppendLine("       a.DATA_TYPE as DataType,");
            query.AppendLine("       a.CHARACTER_SET_NAME as CharacterSetName,");
            query.AppendLine("       a.COLLATION_NAME as CollationName,");

            if (this.CurrentServerVersion.Major > 10 || (this.CurrentServerVersion.Major == 10 && this.CurrentServerVersion.Minor >= 2))
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

            if (this.CurrentServerVersion.Major < 10)
            {
                query.AppendLine("   AND COALESCE(tc.CONSTRAINT_TYPE, 'INDEX') != 'FOREIGN KEY'");
            }

            return context.Query<MySqlIndex>(query.ToString());
        }
    }
}
