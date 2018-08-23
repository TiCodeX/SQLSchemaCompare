using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.MicrosoftSql;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Infrastructure.EntityFramework;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves various information from a Microsoft SQL Server
    /// </summary>
    internal class MicrosoftSqlDatabaseProvider
        : ADatabaseProvider<MicrosoftSqlDatabaseProviderOptions, MicrosoftSqlDatabaseContext, MicrosoftSqlDb>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlDatabaseProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="options">The options to connect to the Microsoft SQL Database</param>
        public MicrosoftSqlDatabaseProvider(ILoggerFactory loggerFactory, MicrosoftSqlDatabaseProviderOptions options)
            : base(loggerFactory, loggerFactory.CreateLogger(nameof(MicrosoftSqlDatabaseProvider)), options)
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
        protected override IEnumerable<ABaseDbTable> GetTables(MicrosoftSqlDb database, MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT a.TABLE_NAME as Name,");
            query.AppendLine("       a.TABLE_CATALOG as 'Catalog',");
            query.AppendLine("       a.TABLE_SCHEMA as 'Schema',");
            query.AppendLine("       b.object_id as ObjectId,");
            query.AppendLine("       b.create_date as CreateDate,");
            query.AppendLine("       b.modify_date as ModifyDate");
            query.AppendLine("FROM INFORMATION_SCHEMA.TABLES a");
            query.AppendLine("JOIN SYS.objects b ON b.object_id = object_id(a.TABLE_SCHEMA + '.' + a.TABLE_NAME)");
            query.AppendLine("WHERE b.type = 'U'");

            return context.Query<MicrosoftSqlTable>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbColumn> GetColumns(MicrosoftSqlDb database, MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT a.TABLE_CATALOG as 'Catalog',");
            query.AppendLine("       a.TABLE_SCHEMA as 'Schema',");
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
            query.AppendLine("       IsNull(b.increment_value, 0) as IdentityIncrement,");
            query.AppendLine("       IsNull(c.is_computed, 0) as IsComputed,");
            query.AppendLine("       c.definition as Definition");
            query.AppendLine("FROM INFORMATION_SCHEMA.COLUMNS a");
            query.AppendLine("LEFT JOIN sys.identity_columns b ON object_id(a.TABLE_SCHEMA + '.' + a.TABLE_NAME) = b.object_id AND a.COLUMN_NAME = b.name");
            query.AppendLine("LEFT JOIN sys.computed_columns c ON object_id(a.TABLE_SCHEMA + '.' + a.TABLE_NAME) = c.object_id AND a.COLUMN_NAME = c.name");
            query.AppendLine($"WHERE a.TABLE_CATALOG = '{database.Name}'");

            return context.Query<MicrosoftSqlColumn>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbConstraint> GetForeignKeys(MicrosoftSqlDb database, MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT tc.CONSTRAINT_CATALOG as 'Catalog',");
            query.AppendLine("       tc.CONSTRAINT_SCHEMA as 'Schema',");
            query.AppendLine("       tc.CONSTRAINT_NAME as Name,");
            query.AppendLine("       tc.TABLE_CATALOG as TableCatalog,");
            query.AppendLine("       tc.TABLE_SCHEMA as TableSchema,");
            query.AppendLine("       tc.TABLE_NAME as TableName,");
            query.AppendLine("       col.name as ColumnName,");
            query.AppendLine("       tc.CONSTRAINT_TYPE as ConstraintType,");
            query.AppendLine("       reftb.name as ReferencedTableName,");
            query.AppendLine("       refs.name as ReferencedTableSchema,");
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
            query.AppendLine("       fk.update_referential_action as UpdateReferentialAction,");
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
            query.AppendLine("INNER JOIN sys.schemas refs");
            query.AppendLine("	ON reftb.schema_id = refs.schema_id");
            query.AppendLine("INNER JOIN sys.columns refcol");
            query.AppendLine("	ON refcol.column_id = fkc.referenced_column_id and refcol.object_id = reftb.object_id");
            query.AppendLine($"WHERE tc.TABLE_CATALOG = '{database.Name}'");
            return context.Query<MicrosoftSqlForeignKey>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbConstraint> GetConstraints(MicrosoftSqlDb database, MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT tc.TABLE_CATALOG AS TableCatalog,");
            query.AppendLine("       tc.TABLE_SCHEMA AS TableSchema,");
            query.AppendLine("       tc.TABLE_NAME AS TableName,");
            query.AppendLine("       tc.CONSTRAINT_CATALOG AS Catalog,");
            query.AppendLine("       tc.CONSTRAINT_SCHEMA AS 'Schema',");
            query.AppendLine("       tc.CONSTRAINT_NAME AS Name,");
            query.AppendLine("       ccu.COLUMN_NAME AS ColumnName,");
            query.AppendLine("       tc.CONSTRAINT_TYPE AS ConstraintType,");
            query.AppendLine("       cc.definition AS Definition");
            query.AppendLine("FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc");
            query.AppendLine("JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu ON tc.CONSTRAINT_NAME = ccu.CONSTRAINT_NAME");
            query.AppendLine("JOIN sys.objects o ON tc.CONSTRAINT_NAME = o.name");
            query.AppendLine("LEFT OUTER JOIN sys.check_constraints cc ON o.object_id = cc.object_id");
            query.AppendLine("WHERE tc.CONSTRAINT_TYPE != 'PRIMARY KEY' and tc.CONSTRAINT_TYPE != 'FOREIGN KEY'");
            return context.Query<ABaseDbConstraint>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbIndex> GetIndexes(MicrosoftSqlDb database, MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT DB_NAME() AS 'Catalog',");
            query.AppendLine("       object_schema_name(i.object_id) AS 'Schema',");
            query.AppendLine("       i.name AS 'Name',");
            query.AppendLine("       DB_NAME() AS 'TableCatalog',");
            query.AppendLine("       object_schema_name(i.object_id) AS 'TableSchema',");
            query.AppendLine("       object_name(i.object_id) AS 'TableName',");
            query.AppendLine("       c.name AS 'ColumnName',");
            query.AppendLine("       CASE");
            query.AppendLine("           WHEN i.is_primary_key = 1 THEN 'PRIMARY KEY'");
            query.AppendLine("           WHEN i.is_unique = 1 THEN 'UNIQUE'");
            query.AppendLine("           ELSE 'INDEX'");
            query.AppendLine("       END AS 'ConstraintType',");
            query.AppendLine("       i.is_primary_key AS 'IsPrimaryKey',");
            query.AppendLine("       ic.is_descending_key as 'IsDescending',");
            query.AppendLine("       ic.key_ordinal AS 'OrdinalPosition',");
            query.AppendLine("       i.type AS Type,");
            query.AppendLine("       i.type_desc AS 'TypeDescription',");
            query.AppendLine("       i.is_unique AS 'IsUnique',");
            query.AppendLine("       i.ignore_dup_key AS 'IgnoreDupKey',");
            query.AppendLine("       i.is_unique_constraint AS 'IsUniqueContraint',");
            query.AppendLine("       i.fill_factor AS 'IndexFillFactor',");
            query.AppendLine("       i.is_padded AS 'IsPadded',");
            query.AppendLine("       i.is_disabled AS 'IsDisabled',");
            query.AppendLine("       i.is_hypothetical AS 'IsHypothetical',");
            query.AppendLine("       i.is_ignored_in_optimization AS 'IsIgnoredInOptimization',");
            query.AppendLine("       i.allow_row_locks AS 'AllowRowLocks',");
            query.AppendLine("       i.allow_page_locks AS 'AllowPageLocks',");
            query.AppendLine("       i.has_filter AS 'HasFilter',");
            query.AppendLine("       i.filter_definition AS 'FilterDefinition',");
            query.AppendLine("       i.compression_delay AS 'CompressionDelay',");
            query.AppendLine("       i.suppress_dup_key_messages AS 'SuppressDupKeyMessages',");
            query.AppendLine("       i.auto_created AS 'AutoCreated'");
            query.AppendLine("FROM sys.indexes i");
            query.AppendLine("JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id");
            query.AppendLine("JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id");
            query.AppendLine("WHERE object_schema_name(i.object_id) <> 'sys'");

            return context.Query<MicrosoftSqlIndex>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbView> GetViews(MicrosoftSqlDb db, MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT TABLE_NAME as Name,");
            query.AppendLine("       TABLE_CATALOG as 'Catalog',");
            query.AppendLine("       TABLE_SCHEMA as 'Schema',");
            query.AppendLine("       VIEW_DEFINITION as ViewDefinition");
            query.AppendLine("FROM INFORMATION_SCHEMA.VIEWS");

            return context.Query<MicrosoftSqlView>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbRoutine> GetFunctions(MicrosoftSqlDb db, MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT ROUTINE_NAME as Name,");
            query.AppendLine("       ROUTINE_CATALOG as 'Catalog',");
            query.AppendLine("       ROUTINE_SCHEMA as 'Schema',");
            query.AppendLine("       ROUTINE_DEFINITION as RoutineDefinition");
            query.AppendLine("FROM INFORMATION_SCHEMA.ROUTINES");
            query.AppendLine("WHERE routine_type = 'FUNCTION'");

            return context.Query<MicrosoftSqlFunction>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbRoutine> GetStoredProcedures(MicrosoftSqlDb db, MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT ROUTINE_NAME as Name,");
            query.AppendLine("       ROUTINE_CATALOG as 'Catalog',");
            query.AppendLine("       ROUTINE_SCHEMA as 'Schema',");
            query.AppendLine("       ROUTINE_DEFINITION as RoutineDefinition");
            query.AppendLine("FROM INFORMATION_SCHEMA.ROUTINES");
            query.AppendLine("WHERE routine_type = 'PROCEDURE'");

            return context.Query<MicrosoftSqlFunction>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbDataType> GetDataTypes(MicrosoftSqlDb database, MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT DB_NAME() AS 'Catalog',");
            query.AppendLine("       sc.name AS 'Schema',");
            query.AppendLine("       t.name AS 'Name',");
            query.AppendLine("       t.user_type_id AS 'TypeId',");
            query.AppendLine("       t.system_type_id AS 'SystemTypeId',");
            query.AppendLine("       t.is_user_defined AS 'IsUserDefined',");
            query.AppendLine("       t.is_nullable AS 'IsNullable',");
            query.AppendLine("       t.precision AS 'Precision',");
            query.AppendLine("       t.scale AS 'Scale',");
            query.AppendLine("       t.max_length AS 'MaxLength'");
            query.AppendLine("FROM sys.types t");
            query.AppendLine("INNER JOIN sys.schemas sc ON t.schema_id = sc.schema_id");

            var types = context.Query<MicrosoftSqlDataType>(query.ToString());

            // Get all the user defined and set the reference to the related system type
            foreach (var t in types.Where(x => x.IsUserDefined))
            {
                t.SystemType = types.FirstOrDefault(x => x.TypeId == t.SystemTypeId);
            }

            return types;
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbSequence> GetSequences(MicrosoftSqlDb database, MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT DB_NAME() AS 'Catalog',");
            query.AppendLine("       object_schema_name(s.object_id) AS 'Schema',");
            query.AppendLine("       s.name AS 'Name',");
            query.AppendLine("       type_name(s.system_type_id) AS 'DataType',");
            query.AppendLine("       CONVERT(VARCHAR, s.start_value) AS 'StartValue',");
            query.AppendLine("       CONVERT(VARCHAR, s.increment) AS 'Increment',");
            query.AppendLine("       CONVERT(VARCHAR, s.minimum_value) AS 'MinValue',");
            query.AppendLine("       CONVERT(VARCHAR, s.maximum_value) AS 'MaxValue'");
            query.AppendLine("FROM sys.sequences s");
            return context.Query<ABaseDbSequence>(query.ToString());
        }
    }
}