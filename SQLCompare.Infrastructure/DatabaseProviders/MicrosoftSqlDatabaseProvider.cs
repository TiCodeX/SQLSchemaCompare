using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Infrastructure.EntityFramework;
using System.Collections.Generic;
using System.Text;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves various information from a Microsoft SQL Server
    /// </summary>
    internal class MicrosoftSqlDatabaseProvider : ADatabaseProvider<MicrosoftSqlDatabaseProviderOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlDatabaseProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="options">The options to connect to the Microsoft SQL Database</param>
        public MicrosoftSqlDatabaseProvider(ILoggerFactory loggerFactory, MicrosoftSqlDatabaseProviderOptions options)
            : base(loggerFactory, loggerFactory.CreateLogger("MicrosoftSqlDatabaseProvider"), options)
        {
        }

        /// <inheritdoc/>
        public override ABaseDb GetDatabase()
        {
            using (var context = new MicrosoftSqlDatabaseContext(this.LoggerFactory, this.Options))
            {
                MicrosoftSqlDb db = new MicrosoftSqlDb() { Name = this.Options.Database };

                var tables = GetTables(context);

                foreach (var table in tables)
                {
                    table.Columns.AddRange(GetColumns(table.ObjectId, context));
                }

                db.Tables.AddRange(tables);
                return db;
            }
        }

        /// <inheritdoc/>
        public override List<string> GetDatabaseList()
        {
            using (var context = new MicrosoftSqlDatabaseContext(this.LoggerFactory, this.Options))
            {
                return context.Query("SELECT name FROM sysdatabases");
            }
        }

        private static List<MicrosoftSqlTable> GetTables(MicrosoftSqlDatabaseContext context)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT a.TABLE_NAME as Name,");
            query.AppendLine("       a.TABLE_CATALOG as CatalogName,");
            query.AppendLine("       a.TABLE_SCHEMA as SchemaName,");
            query.AppendLine("       b.object_id as ObjectId,");
            query.AppendLine("       b.create_date as CreateDate,");
            query.AppendLine("       b.modify_date as ModifyDate");
            query.AppendLine("FROM INFORMATION_SCHEMA.TABLES a");
            query.AppendLine("JOIN SYS.objects b ON b.object_id = object_id(a.TABLE_NAME)");

            return context.Query<MicrosoftSqlTable>(query.ToString());
        }

        private static List<MicrosoftSqlColumn> GetColumns(long objectId, MicrosoftSqlDatabaseContext context)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT a.COLUMN_NAME as Name,");
            query.AppendLine("       a.ORDINAL_POSITION as OrdinalPosition,");
            query.AppendLine("       a.COLUMN_DEFAULT as ColumnDefault,");
            query.AppendLine("       CAST(IIF(a.IS_NULLABLE = 'no',0,1) as BIT) as IsNullable,");
            query.AppendLine("       a.DATA_TYPE as DataType,");
            query.AppendLine("       a.CHARACTER_MAXIMUM_LENGTH as CharacterMaxLenght,");
            query.AppendLine("       a.CHARACTER_OCTET_LENGTH as CharacterOctetLenght,");
            query.AppendLine("       a.NUMERIC_PRECISION as NumericPrecision,");
            query.AppendLine("       a.NUMERIC_PRECISION_RADIX as NumericPrecisionRadix,");
            query.AppendLine("       a.NUMERIC_SCALE as NumericScale,");
            query.AppendLine("       a.DATETIME_PRECISION as DateTimePrecision,");
            query.AppendLine("       a.CHARACTER_SET_CATALOG as CharachterSetCatalog,");
            query.AppendLine("       a.CHARACTER_SET_SCHEMA as CharacterSetSchema,");
            query.AppendLine("       a.CHARACTER_SET_NAME as CharacterSetName,");
            query.AppendLine("       a.COLLATION_CATALOG as CollationCatalog,");
            query.AppendLine("       a.COLLATION_SCHEMA as CollationSchema,");
            query.AppendLine("       a.COLLATION_NAME as CollationName,");
            query.AppendLine("       a.DOMAIN_CATALOG as DomainCatalog,");
            query.AppendLine("       a.DOMAIN_SCHEMA as DomainSchema,");
            query.AppendLine("       a.DOMAIN_NAME as DomainName,");
            query.AppendLine("       IsNull(b.is_identity, 0) as IsIdentity,");
            query.AppendLine("       IsNull(b.seed_value, 0) as IdentitySeed,");
            query.AppendLine("       IsNull(b.increment_value, 0) as IdentityIncrement");
            query.AppendLine("FROM INFORMATION_SCHEMA.COLUMNS a");
            query.AppendLine("LEFT JOIN sys.identity_columns b ON object_id(a.TABLE_NAME) = b.object_id and a.COLUMN_NAME = b.name");
            query.AppendLine($"WHERE object_id(a.TABLE_NAME) = {objectId}");

            return context.Query<MicrosoftSqlColumn>(query.ToString());
        }
    }
}