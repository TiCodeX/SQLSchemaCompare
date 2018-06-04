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
    internal class MicrosoftSqlDatabaseProvider
        : ADatabaseProvider<MicrosoftSqlDatabaseProviderOptions, MicrosoftSqlDatabaseContext, MicrosoftSqlDb, MicrosoftSqlTable, MicrosoftSqlColumn, MicrosoftSqlPrimaryKey, MicrosoftSqlForeignKey>
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
                return this.DiscoverDatabase(context);
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

        /// <inheritdoc/>
        protected override List<MicrosoftSqlTable> GetTables(MicrosoftSqlDb database, MicrosoftSqlDatabaseContext context)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT a.TABLE_NAME as Name,");
            query.AppendLine("       a.TABLE_CATALOG as TableCatalog,");
            query.AppendLine("       a.TABLE_SCHEMA as TableSchema,");
            query.AppendLine("       b.object_id as ObjectId,");
            query.AppendLine("       b.create_date as CreateDate,");
            query.AppendLine("       b.modify_date as ModifyDate");
            query.AppendLine("FROM INFORMATION_SCHEMA.TABLES a");
            query.AppendLine("JOIN SYS.objects b ON b.object_id = object_id(a.TABLE_NAME)");

            return context.Query<MicrosoftSqlTable>(query.ToString());
        }

        /// <inheritdoc/>
        protected override List<MicrosoftSqlColumn> GetColumns(MicrosoftSqlDb database, MicrosoftSqlDatabaseContext context)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT a.TABLE_CATALOG as TableCatalog,");
            query.AppendLine("       a.TABLE_SCHEMA as TableSchema,");
            query.AppendLine("       a.TABLE_NAME as TableName,");
            query.AppendLine("       a.COLUMN_NAME as Name,");
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
            query.AppendLine($"WHERE a.TABLE_CATALOG = '{database.Name}'");

            return context.Query<MicrosoftSqlColumn>(query.ToString());
        }

        /// <inheritdoc/>
        protected override List<MicrosoftSqlPrimaryKey> GetPrimaryKeys(MicrosoftSqlDb database, MicrosoftSqlDatabaseContext context)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT a.CONSTRAINT_CATALOG as ConstraintCatalog,");
            query.AppendLine("       a.CONSTRAINT_SCHEMA as ConstraintSchema,");
            query.AppendLine("       a.CONSTRAINT_NAME as Name,");
            query.AppendLine("       a.TABLE_CATALOG as TableCatalog,");
            query.AppendLine("       a.TABLE_SCHEMA as TableSchema,");
            query.AppendLine("       a.TABLE_NAME as TableName,");
            query.AppendLine("       a.COLUMN_NAME as ColumnName,");
            query.AppendLine("       a.ORDINAL_POSITION as OrdinalPosition,");
            query.AppendLine("       b.type as Type,");
            query.AppendLine("       b.type_desc as TypeDescription,");
            query.AppendLine("       b.is_unique as IsUnique,");
            query.AppendLine("       b.ignore_dup_key as IgnoreDupKey,");
            query.AppendLine("       b.is_primary_key as IsPrimaryKey,");
            query.AppendLine("       b.is_unique_constraint as IsUniqueContraint,");
            query.AppendLine("       b.fill_factor as IndexFillFactor,");
            query.AppendLine("       b.is_padded as IsPadded,");
            query.AppendLine("       b.is_disabled as IsDisabled,");
            query.AppendLine("       b.is_hypothetical as IsHypothetical,");
            query.AppendLine("       b.is_ignored_in_optimization as IsIgnoredInOptimization,");
            query.AppendLine("       b.allow_row_locks as AllowRowLocks,");
            query.AppendLine("       b.allow_page_locks as AllowPageLocks,");
            query.AppendLine("       b.has_filter as HasFilter,");
            query.AppendLine("       b.filter_definition as FilterDefinition,");
            query.AppendLine("       b.compression_delay as CompressionDelay,");
            query.AppendLine("       b.suppress_dup_key_messages as SuppressDupKeyMessages,");
            query.AppendLine("       b.auto_created as AutoCreated");
            query.AppendLine("FROM brokerpro.INFORMATION_SCHEMA.KEY_COLUMN_USAGE a ");
            query.AppendLine("JOIN Sys.Indexes b ON a.CONSTRAINT_NAME = b.name ");
            query.AppendLine($"WHERE a.TABLE_CATALOG = '{database.Name}' and b.is_primary_key = 'true'");

            return context.Query<MicrosoftSqlPrimaryKey>(query.ToString());
        }

        /// <inheritdoc/>
        protected override List<MicrosoftSqlForeignKey> GetForeignKeys(MicrosoftSqlDb database, MicrosoftSqlDatabaseContext context)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT tc.CONSTRAINT_CATALOG as ConstraintCatalog,");
            query.AppendLine("       tc.CONSTRAINT_SCHEMA as ConstraintSchema,");
            query.AppendLine("       tc.CONSTRAINT_NAME as Name,");
            query.AppendLine("       tc.TABLE_CATALOG as TableCatalog,");
            query.AppendLine("       tc.TABLE_SCHEMA as TableSchema,");
            query.AppendLine("       tc.TABLE_NAME as TableName,");
            query.AppendLine("       col.name as TableColumn,");
            query.AppendLine("       reftb.name as ReferencedTableName,");
            query.AppendLine("       refcol.name as ReferencedTableColumn,");
            query.AppendLine("       CAST(IIF(tc.IS_DEFERRABLE = 'NO', 0, 1) as BIT) as IsDeferrable,");
            query.AppendLine("       CAST(IIF(tc.INITIALLY_DEFERRED = 'NO', 0, 1) as BIT) as InitiallyDeferred,");
            query.AppendLine("       fk.is_ms_shipped as IsMsShipped,");
            query.AppendLine("       fk.is_published as IsPublished,");
            query.AppendLine("       fk.is_schema_published as IsSchemaPublished,");
            query.AppendLine("       fk.key_index_id as KeyIndexId,");
            query.AppendLine("       fk.is_disabled as IsDisabled,");
            query.AppendLine("       fk.is_not_for_replication as IsNotForReplication,");
            query.AppendLine("       fk.is_not_trusted as IsNotTrusted,");
            query.AppendLine("       fk.delete_referential_action as DeleteReferentialAction,");
            query.AppendLine("       fk.delete_referential_action_desc as DeleteReferentialActionDescription,");
            query.AppendLine("       fk.update_referential_action as UpdateReferentialAction,");
            query.AppendLine("       fk.update_referential_action_desc as UpdateReferentialActionDescription,");
            query.AppendLine("       fk.is_system_named as IsSystemNamed");
            query.AppendLine("FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc");
            query.AppendLine("INNER JOIN sys.foreign_keys fk");
            query.AppendLine("	ON fk.name = tc.CONSTRAINT_NAME");
            query.AppendLine("INNER JOIN sys.foreign_key_columns fkc");
            query.AppendLine("	ON fkc.constraint_object_id = fk.object_id");
            query.AppendLine("INNER JOIN sys.tables tb");
            query.AppendLine("	ON tb.object_id = fkc.parent_object_id");
            query.AppendLine("INNER JOIN sys.columns col");
            query.AppendLine("	ON col.column_id = fkc.parent_column_id and col.object_id = tb.object_id");
            query.AppendLine("INNER JOIN sys.tables reftb");
            query.AppendLine("	ON reftb.object_id = fkc.referenced_object_id");
            query.AppendLine("INNER JOIN sys.columns refcol");
            query.AppendLine("	ON refcol.column_id = fkc.referenced_column_id and refcol.object_id = reftb.object_id");
            query.AppendLine($"WHERE tc.TABLE_CATALOG = '{database.Name}'");
            return context.Query<MicrosoftSqlForeignKey>(query.ToString());
        }
    }
}