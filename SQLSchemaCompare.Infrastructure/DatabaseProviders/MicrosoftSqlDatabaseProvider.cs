namespace TiCodeX.SQLSchemaCompare.Infrastructure.DatabaseProviders
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using TiCodeX.SQLSchemaCompare.Core.Entities;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
    using TiCodeX.SQLSchemaCompare.Core.Entities.Database.MicrosoftSql;
    using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;
    using TiCodeX.SQLSchemaCompare.Infrastructure.EntityFramework;

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
        /// <param name="cipherService">The injected cipher service</param>
        /// <param name="options">The options to connect to the Microsoft SQL Database</param>
        public MicrosoftSqlDatabaseProvider(ILoggerFactory loggerFactory, ICipherService cipherService, MicrosoftSqlDatabaseProviderOptions options)
            : base(loggerFactory, loggerFactory.CreateLogger(nameof(MicrosoftSqlDatabaseProvider)), cipherService, options)
        {
        }

        /// <inheritdoc/>
        public override ABaseDb GetDatabase(TaskInfo taskInfo)
        {
            using (var context = new MicrosoftSqlDatabaseContext(this.LoggerFactory, this.CipherService, this.Options))
            {
                return this.DiscoverDatabase(context, taskInfo);
            }
        }

        /// <inheritdoc/>
        public override List<string> GetDatabaseList()
        {
            using (var context = new MicrosoftSqlDatabaseContext(this.LoggerFactory, this.CipherService, this.Options))
            {
                return context.QuerySingleColumn<string>("SELECT name FROM sysdatabases");
            }
        }

        /// <inheritdoc/>
        protected override string GetServerVersion(MicrosoftSqlDatabaseContext context)
        {
            return context.QuerySingleColumn<string>("SELECT SERVERPROPERTY('ProductVersion')").FirstOrDefault() ?? string.Empty;
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbSchema> GetSchemas(MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT s.name as 'Name',");
            query.AppendLine("       u.name as 'Owner'");
            query.AppendLine("FROM sys.schemas s");
            query.AppendLine("INNER JOIN sys.sysusers u ON u.uid = s.principal_id");
            query.AppendLine("WHERE LOWER(s.name) NOT IN ('dbo', 'guest', 'sys', 'information_schema')");
            query.AppendLine("      AND s.schema_id < 16000");
            return context.Query<ABaseDbSchema>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbTable> GetTables(MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT t.name as Name,");
            query.AppendLine("       s.name as 'Schema',");
            query.AppendLine("       CASE WHEN p.period_type IS NULL THEN 0 ELSE 1 END AS 'HasPeriod',");
            query.AppendLine("       p.name AS 'PeriodName',");
            query.AppendLine("       (SELECT c.name FROM sys.columns c WHERE c.object_id = t.object_id AND c.column_id = p.start_column_id) AS 'PeriodStartColumn',");
            query.AppendLine("       (SELECT c.name FROM sys.columns c WHERE c.object_id = t.object_id AND c.column_id = p.end_column_id) AS 'PeriodEndColumn',");
            query.AppendLine("       CASE t.temporal_type WHEN 2 THEN 1 ELSE 0 END as 'HasHistoryTable',");
            query.AppendLine("       OBJECT_SCHEMA_NAME(t.history_table_id) AS 'HistoryTableSchema',");
            query.AppendLine("       OBJECT_NAME(t.history_table_id) AS 'HistoryTableName',");
            query.AppendLine("       o.modify_date as 'ModifyDate'");
            query.AppendLine("FROM sys.tables t");
            query.AppendLine("JOIN sys.schemas s ON s.schema_id = t.schema_id");
            query.AppendLine("JOIN sys.objects o ON o.object_id = t.object_id");
            query.AppendLine("LEFT JOIN sys.periods p ON p.object_id = t.object_id");
            query.AppendLine("WHERE o.type = 'U' AND s.name <> 'sys' AND t.temporal_type <> 1");

            return context.Query<MicrosoftSqlTable>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbColumn> GetColumns(MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT isc.TABLE_SCHEMA as 'Schema',");
            query.AppendLine("       isc.TABLE_NAME as 'TableName',");
            query.AppendLine("       isc.COLUMN_NAME as 'Name',");
            query.AppendLine("       CAST(isc.ORDINAL_POSITION AS bigint) as 'OrdinalPosition',");
            query.AppendLine("       isc.COLUMN_DEFAULT as 'ColumnDefault',");
            query.AppendLine("       dc.name as 'DefaultConstraintName',");
            query.AppendLine("       CASE UPPER(isc.IS_NULLABLE) WHEN 'NO' THEN 0 ELSE 1 END as 'IsNullable',");
            query.AppendLine("       isc.DATA_TYPE as 'DataType',");
            query.AppendLine("       isc.CHARACTER_MAXIMUM_LENGTH as 'CharacterMaxLength',");
            query.AppendLine("       isc.NUMERIC_PRECISION as 'NumericPrecision',");
            query.AppendLine("       isc.NUMERIC_SCALE as 'NumericScale',");
            query.AppendLine("       isc.DATETIME_PRECISION as 'DateTimePrecision',");
            query.AppendLine("       isc.CHARACTER_SET_NAME as 'CharacterSetName',");
            query.AppendLine("       isc.COLLATION_NAME as 'CollationName',");
            query.AppendLine("       IsNull(ic.is_identity, 0) as 'IsIdentity',");
            query.AppendLine("       IsNull(ic.seed_value, 0) as 'IdentitySeed',");
            query.AppendLine("       IsNull(ic.increment_value, 0) as 'IdentityIncrement',");
            query.AppendLine("       IsNull(cc.is_computed, 0) as 'IsComputed',");
            query.AppendLine("       IsNull(c.is_rowguidcol, 0) as 'IsRowGuidCol',");
            query.AppendLine("       cc.definition as 'Definition',");
            query.AppendLine("       isc.DOMAIN_SCHEMA as 'UserDefinedDataTypeSchema',");
            query.AppendLine("       isc.DOMAIN_NAME as 'UserDefinedDataType',");
            query.AppendLine("       c.generated_always_type as 'GeneratedAlwaysType',");
            query.AppendLine("       c.is_hidden as 'IsHidden'");
            query.AppendLine("FROM INFORMATION_SCHEMA.COLUMNS isc");
            query.AppendLine("JOIN sys.columns c ON object_id(QUOTENAME(isc.TABLE_SCHEMA) + '.' + QUOTENAME(isc.TABLE_NAME)) = c.object_id AND isc.COLUMN_NAME = c.name");
            query.AppendLine("LEFT JOIN sys.identity_columns ic ON c.object_id = ic.object_id AND c.column_id = ic.column_id");
            query.AppendLine("LEFT JOIN sys.computed_columns cc ON c.object_id = cc.object_id AND c.column_id = cc.column_id");
            query.AppendLine("LEFT JOIN sys.default_constraints dc ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id");
            query.AppendLine($"WHERE isc.TABLE_CATALOG = '{context.DatabaseName}' AND isc.TABLE_SCHEMA <> 'sys'");

            return context.Query<MicrosoftSqlColumn>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbPrimaryKey> GetPrimaryKeys(MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT object_schema_name(i.object_id) AS 'Schema',");
            query.AppendLine("       i.name AS 'Name',");
            query.AppendLine("       object_schema_name(i.object_id) AS 'TableSchema',");
            query.AppendLine("       object_name(i.object_id) AS 'TableName',");
            query.AppendLine("       c.name AS 'ColumnName',");
            query.AppendLine("       CASE");
            query.AppendLine("           WHEN i.is_primary_key = 1 THEN 'PRIMARY KEY'");
            query.AppendLine("           WHEN i.is_unique = 1 THEN 'UNIQUE'");
            query.AppendLine("           ELSE 'INDEX'");
            query.AppendLine("       END AS 'ConstraintType',");
            query.AppendLine("       ic.is_descending_key as 'IsDescending',");
            query.AppendLine("       CAST(ic.key_ordinal AS bigint) AS 'OrdinalPosition',");
            query.AppendLine("       i.type_desc AS 'TypeDescription'");
            query.AppendLine("FROM sys.indexes i");
            query.AppendLine("JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id");
            query.AppendLine("JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id");
            query.AppendLine("WHERE object_schema_name(i.object_id) <> 'sys' AND i.is_primary_key = 1");

            return context.Query<MicrosoftSqlPrimaryKey>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbForeignKey> GetForeignKeys(MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT tc.CONSTRAINT_SCHEMA as 'Schema',");
            query.AppendLine("       tc.CONSTRAINT_NAME as 'Name',");
            query.AppendLine("       tc.TABLE_SCHEMA as 'TableSchema',");
            query.AppendLine("       tc.TABLE_NAME as 'TableName',");
            query.AppendLine("       col.name as 'ColumnName',");
            query.AppendLine("       CAST(fkc.constraint_column_id AS bigint) as 'OrdinalPosition',");
            query.AppendLine("       tc.CONSTRAINT_TYPE as 'ConstraintType',");
            query.AppendLine("       reftb.name as 'ReferencedTableName',");
            query.AppendLine("       refs.name as 'ReferencedTableSchema',");
            query.AppendLine("       refcol.name as 'ReferencedColumnName',");
            query.AppendLine("       fk.delete_referential_action as 'DeleteReferentialAction',");
            query.AppendLine("       fk.update_referential_action as 'UpdateReferentialAction',");
            query.AppendLine("       fk.is_disabled as 'Disabled'");
            query.AppendLine("FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc");
            query.AppendLine("INNER JOIN sys.foreign_keys fk ON fk.name = tc.CONSTRAINT_NAME");
            query.AppendLine("INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id");
            query.AppendLine("INNER JOIN sys.tables tb ON tb.object_id = fkc.parent_object_id");
            query.AppendLine("INNER JOIN sys.columns col ON col.column_id = fkc.parent_column_id and col.object_id = tb.object_id");
            query.AppendLine("INNER JOIN sys.tables reftb ON reftb.object_id = fkc.referenced_object_id");
            query.AppendLine("INNER JOIN sys.schemas refs ON reftb.schema_id = refs.schema_id");
            query.AppendLine("INNER JOIN sys.columns refcol ON refcol.column_id = fkc.referenced_column_id and refcol.object_id = reftb.object_id");
            query.AppendLine($"WHERE tc.TABLE_CATALOG = '{context.DatabaseName}' AND tc.CONSTRAINT_SCHEMA <> 'sys'");
            return context.Query<MicrosoftSqlForeignKey>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbConstraint> GetConstraints(MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT tc.TABLE_SCHEMA AS 'TableSchema',");
            query.AppendLine("       tc.TABLE_NAME AS 'TableName',");
            query.AppendLine("       tc.CONSTRAINT_SCHEMA AS 'Schema',");
            query.AppendLine("       tc.CONSTRAINT_NAME AS 'Name',");
            query.AppendLine("       ccu.COLUMN_NAME AS 'ColumnName',");
            query.AppendLine("       tc.CONSTRAINT_TYPE AS 'ConstraintType',");
            query.AppendLine("       cc.definition AS 'Definition'");
            query.AppendLine("FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc");
            query.AppendLine("JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu ON tc.CONSTRAINT_NAME = ccu.CONSTRAINT_NAME");
            query.AppendLine("JOIN sys.objects o ON tc.CONSTRAINT_NAME = o.name");
            query.AppendLine("LEFT OUTER JOIN sys.check_constraints cc ON o.object_id = cc.object_id");
            query.AppendLine("WHERE tc.CONSTRAINT_TYPE = 'CHECK' AND");
            query.AppendLine("      tc.TABLE_SCHEMA <> 'sys'");
            return context.Query<ABaseDbConstraint>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbIndex> GetIndexes(MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT object_schema_name(i.object_id) AS 'Schema',");
            query.AppendLine("       i.name AS 'Name',");
            query.AppendLine("       object_schema_name(i.object_id) AS 'TableSchema',");
            query.AppendLine("       object_name(i.object_id) AS 'TableName',");
            query.AppendLine("       c.name AS 'ColumnName',");
            query.AppendLine("       CASE");
            query.AppendLine("           WHEN i.is_primary_key = 1 THEN 'PRIMARY KEY'");
            query.AppendLine("           WHEN i.is_unique = 1 THEN 'UNIQUE'");
            query.AppendLine("           ELSE 'INDEX'");
            query.AppendLine("       END AS 'ConstraintType',");
            query.AppendLine("       ic.is_descending_key as 'IsDescending',");
            query.AppendLine("       CAST(ic.key_ordinal AS bigint) AS 'OrdinalPosition',");
            query.AppendLine("       i.type AS Type,");
            query.AppendLine("       i.is_unique AS 'IsUnique',");
            query.AppendLine("       i.filter_definition AS 'FilterDefinition'");
            query.AppendLine("FROM sys.indexes i");
            query.AppendLine("JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id");
            query.AppendLine("JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id");
            query.AppendLine("LEFT JOIN sys.tables t ON i.object_id = t.object_id");
            query.AppendLine("WHERE object_schema_name(i.object_id) <> 'sys' AND i.is_primary_key = 0 AND ISNULL(t.temporal_type, 0) <> 1");

            return context.Query<MicrosoftSqlIndex>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbView> GetViews(MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT name as 'Name',");
            query.AppendLine("       SCHEMA_NAME(schema_id) as 'Schema',");
            query.AppendLine("       OBJECT_DEFINITION(object_id) as 'ViewDefinition'");
            query.AppendLine("FROM sys.views");
            query.AppendLine("WHERE NULLIF(OBJECT_DEFINITION(object_id), '') IS NOT NULL AND");
            query.AppendLine("      SCHEMA_NAME(schema_id) <> 'sys'");

            return context.Query<MicrosoftSqlView>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbFunction> GetFunctions(MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT name as 'Name',");
            query.AppendLine("       SCHEMA_NAME(schema_id) as 'Schema',");
            query.AppendLine("       OBJECT_DEFINITION(object_id) as 'Definition'");
            query.AppendLine("FROM sys.objects");
            query.AppendLine("WHERE type IN ('FN', 'TF', 'IF', 'AF', 'FT', 'IS', 'FS') AND");
            query.AppendLine("      NULLIF(OBJECT_DEFINITION(object_id), '') IS NOT NULL AND");
            query.AppendLine("      SCHEMA_NAME(schema_id) <> 'sys'");

            return context.Query<MicrosoftSqlFunction>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbStoredProcedure> GetStoredProcedures(MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT name as 'Name',");
            query.AppendLine("       SCHEMA_NAME(schema_id) as 'Schema',");
            query.AppendLine("       OBJECT_DEFINITION(object_id) as 'Definition'");
            query.AppendLine("FROM sys.objects");
            query.AppendLine("WHERE type IN ('P', 'PC') AND");
            query.AppendLine("      NULLIF(OBJECT_DEFINITION(object_id), '') IS NOT NULL AND");
            query.AppendLine("      SCHEMA_NAME(schema_id) <> 'sys'");

            return context.Query<MicrosoftSqlStoredProcedure>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbTrigger> GetTriggers(MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT object_schema_name(o.id) AS 'Schema',");
            query.AppendLine("       o.name AS 'Name',");
            query.AppendLine("       object_schema_name(o.parent_obj) AS 'TableSchema',");
            query.AppendLine("       object_name(o.parent_obj) AS 'TableName',");
            query.AppendLine("       c.text AS 'Definition'");
            query.AppendLine("FROM sys.sysobjects o");
            query.AppendLine("INNER JOIN sys.syscomments AS c ON o.id = c.id");
            query.AppendLine("WHERE o.type = 'TR' AND");
            query.AppendLine("      NULLIF(c.text, '') IS NOT NULL AND");
            query.AppendLine("      object_schema_name(o.id) <> 'sys'");

            return context.Query<ABaseDbTrigger>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbDataType> GetDataTypes(MicrosoftSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT sc.name AS 'Schema',");
            query.AppendLine("       t.name AS 'Name',");
            query.AppendLine("       t.user_type_id AS 'TypeId',");
            query.AppendLine("       t.system_type_id AS 'SystemTypeId',");
            query.AppendLine("       t.is_user_defined AS 'IsUserDefined',");
            query.AppendLine("       t.is_table_type AS 'IsTableType',");
            query.AppendLine("       t.is_nullable AS 'IsNullable',");
            query.AppendLine("       t.precision AS 'Precision',");
            query.AppendLine("       t.scale AS 'Scale',");
            query.AppendLine("       t.max_length AS 'MaxLength'");
            query.AppendLine("FROM sys.types t");
            query.AppendLine("INNER JOIN sys.schemas sc ON t.schema_id = sc.schema_id");

            var allTypes = context.Query<MicrosoftSqlDataType>(query.ToString());

            var types = new List<MicrosoftSqlDataType>();
            foreach (var type in allTypes)
            {
                if (type.IsTableType)
                {
                    this.Logger.LogError($"Unsupported User-Defined Table Type: {type.Name}");
                    continue;
                }

                if (type.IsUserDefined)
                {
                    type.SystemType = allTypes.FirstOrDefault(x => x.TypeId == type.SystemTypeId);

                    if (type.SystemType == null)
                    {
                        this.Logger.LogError($"Unable to find corresponding system type for '{type.Name}'");
                        continue;
                    }
                }

                types.Add(type);
            }

            return types;
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbSequence> GetSequences(MicrosoftSqlDatabaseContext context)
        {
            // Sequences are supported only from MS SQL Server 2012
            if (this.CurrentServerVersion.Major < 11)
            {
                return Enumerable.Empty<ABaseDbSequence>();
            }

            var query = new StringBuilder();
            query.AppendLine("SELECT object_schema_name(s.object_id) AS 'Schema',");
            query.AppendLine("       s.name AS 'Name',");
            query.AppendLine("       type_name(s.system_type_id) AS 'DataType',");
            query.AppendLine("       CONVERT(VARCHAR, s.start_value) AS 'StartValue',");
            query.AppendLine("       CONVERT(VARCHAR, s.increment) AS 'Increment',");
            query.AppendLine("       CONVERT(VARCHAR, s.minimum_value) AS 'MinValue',");
            query.AppendLine("       CONVERT(VARCHAR, s.maximum_value) AS 'MaxValue',");
            query.AppendLine("       s.is_cycling AS 'IsCycling',");
            query.AppendLine("       s.is_cached AS 'IsCached'");
            query.AppendLine("FROM sys.sequences s");
            query.AppendLine("WHERE object_schema_name(s.object_id) <> 'sys'");
            return context.Query<MicrosoftSqlSequence>(query.ToString());
        }
    }
}
