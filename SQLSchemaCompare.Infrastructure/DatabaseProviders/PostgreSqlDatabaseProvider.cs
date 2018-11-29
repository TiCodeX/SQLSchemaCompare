using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using TiCodeX.SQLSchemaCompare.Core.Entities;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database.PostgreSql;
using TiCodeX.SQLSchemaCompare.Core.Entities.DatabaseProvider;
using TiCodeX.SQLSchemaCompare.Core.Entities.Project;
using TiCodeX.SQLSchemaCompare.Core.Interfaces.Services;
using TiCodeX.SQLSchemaCompare.Infrastructure.EntityFramework;
using TiCodeX.SQLSchemaCompare.Infrastructure.SqlScripters;

namespace TiCodeX.SQLSchemaCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves various information from a PostgreSQL Server
    /// </summary>
    internal class PostgreSqlDatabaseProvider
        : ADatabaseProvider<PostgreSqlDatabaseProviderOptions, PostgreSqlDatabaseContext, PostgreSqlDb>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlDatabaseProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="cipherService">The injected cipher service</param>
        /// <param name="options">The options to connect to the PostgreSQL Database</param>
        public PostgreSqlDatabaseProvider(ILoggerFactory loggerFactory, ICipherService cipherService, PostgreSqlDatabaseProviderOptions options)
            : base(loggerFactory, loggerFactory.CreateLogger(nameof(PostgreSqlDatabaseProvider)), cipherService, options)
        {
        }

        /// <inheritdoc />
        public override ABaseDb GetDatabase(TaskInfo taskInfo)
        {
            using (var context = new PostgreSqlDatabaseContext(this.LoggerFactory, this.CipherService, this.Options))
            {
                return this.DiscoverDatabase(context, taskInfo);
            }
        }

        /// <inheritdoc />
        public override List<string> GetDatabaseList()
        {
            using (var context = new PostgreSqlDatabaseContext(this.LoggerFactory, this.CipherService, this.Options))
            {
                return context.QuerySingleColumn<string>("SELECT datname FROM pg_catalog.pg_database WHERE datistemplate = FALSE");
            }
        }

        /// <inheritdoc/>
        protected override string GetServerVersion(PostgreSqlDatabaseContext context)
        {
            return context.QuerySingleColumn<string>("SHOW server_version").FirstOrDefault() ?? string.Empty;
        }

        /// <inheritdoc />
        protected override IEnumerable<ABaseDbTable> GetTables(PostgreSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT nsp.nspname AS \"Schema\",");
            query.AppendLine("       cls.relname AS \"Name\",");
            query.AppendLine("       ihnsp.nspname AS \"InheritedTableSchema\",");
            query.AppendLine("       ihcls.relname AS \"InheritedTableName\"");
            query.AppendLine("FROM pg_catalog.pg_class cls");
            query.AppendLine("JOIN pg_catalog.pg_roles rol ON rol.oid = cls.relowner");
            query.AppendLine("JOIN pg_catalog.pg_namespace nsp ON nsp.oid = cls.relnamespace");
            query.AppendLine("LEFT JOIN pg_catalog.pg_inherits ih ON ih.inhrelid = cls.oid");
            query.AppendLine("LEFT JOIN pg_catalog.pg_class ihcls ON ih.inhparent = ihcls.oid");
            query.AppendLine("LEFT JOIN pg_catalog.pg_namespace ihnsp ON ihnsp.oid = ihcls.relnamespace");
            query.AppendLine("WHERE cls.relkind IN ('r', 'p') AND nsp.nspname = 'public'");

            return context.Query<PostgreSqlTable>(query.ToString());
        }

        /// <inheritdoc />
        protected override IEnumerable<ABaseDbColumn> GetColumns(PostgreSqlDatabaseContext context)
        {
            var query = new StringBuilder();

            query.AppendLine("SELECT table_schema as \"Schema\",");
            query.AppendLine("       table_name as \"TableName\",");
            query.AppendLine("       column_name as \"Name\",");
            query.AppendLine("       ordinal_position as \"OrdinalPosition\",");
            query.AppendLine("       column_default as \"ColumnDefault\",");
            query.AppendLine("       CASE ");
            query.AppendLine("            WHEN is_nullable = 'YES' THEN true");
            query.AppendLine("            ELSE false");
            query.AppendLine("       END as \"IsNullable\",");
            query.AppendLine("       data_type as \"DataType\",");
            query.AppendLine("       character_maximum_length as \"CharacterMaxLenght\",");
            query.AppendLine("       numeric_precision as \"NumericPrecision\",");
            query.AppendLine("       numeric_scale as \"NumericScale\",");
            query.AppendLine("       datetime_precision as \"DateTimePrecision\",");
            query.AppendLine("       interval_type as \"IntervalType\",");
            query.AppendLine("       character_set_name as \"CharacterSetName\",");
            query.AppendLine("       collation_name as \"CollationName\",");
            query.AppendLine("       udt_name as \"UdtName\"");
            query.AppendLine("FROM INFORMATION_SCHEMA.COLUMNS");
            query.AppendLine($"WHERE TABLE_CATALOG = '{context.DatabaseName}'");

            return context.Query<PostgreSqlColumn>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbForeignKey> GetForeignKeys(PostgreSqlDatabaseContext context)
        {
            var query = new StringBuilder();

            query.AppendLine("SELECT kcu.constraint_schema AS \"Schema\",");
            query.AppendLine("       kcu.constraint_name AS \"Name\",");
            query.AppendLine("       kcu.table_schema AS \"TableSchema\",");
            query.AppendLine("       kcu.table_name AS \"TableName\",");
            query.AppendLine("       kcu.column_name AS \"ColumnName\",");
            query.AppendLine("       tu.constraint_type AS \"ConstraintType\",");
            query.AppendLine("       CAST(kcu.ordinal_position AS bigint) AS \"OrdinalPosition\",");
            query.AppendLine("       rc.match_option AS \"MatchOption\",");
            query.AppendLine("       rc.update_rule AS \"UpdateRule\",");
            query.AppendLine("       rc.delete_rule AS \"DeleteRule\",");
            query.AppendLine("       ccu.table_schema AS \"ReferencedTableSchema\",");
            query.AppendLine("       ccu.table_name AS \"ReferencedTableName\",");
            query.AppendLine("       ccu.column_name AS \"ReferencedColumnName\",");
            query.AppendLine("       CASE ");
            query.AppendLine("            WHEN tu.is_deferrable = 'YES' THEN true");
            query.AppendLine("            ELSE false");
            query.AppendLine("       END AS \"IsDeferrable\",");
            query.AppendLine("       CASE ");
            query.AppendLine("            WHEN tu.initially_deferred = 'YES' THEN true");
            query.AppendLine("            ELSE false");
            query.AppendLine("       END AS \"IsInitiallyDeferred\"");
            query.AppendLine("FROM information_schema.key_column_usage kcu");
            query.AppendLine("INNER JOIN information_schema.table_constraints tu");
            query.AppendLine("    ON kcu.constraint_name = tu.constraint_name AND kcu.constraint_schema = tu.constraint_schema AND kcu.constraint_catalog = tu.constraint_catalog");
            query.AppendLine("INNER JOIN information_schema.referential_constraints rc");
            query.AppendLine("    ON kcu.constraint_name = rc.constraint_name AND kcu.constraint_schema = rc.constraint_schema AND kcu.constraint_catalog = rc.constraint_catalog");
            query.AppendLine("INNER JOIN information_schema.constraint_column_usage ccu");
            query.AppendLine("    ON kcu.constraint_name = ccu.constraint_name AND kcu.constraint_schema = ccu.constraint_schema AND kcu.constraint_catalog = ccu.constraint_catalog");
            query.AppendLine($"WHERE kcu.constraint_catalog = '{context.DatabaseName}' AND tu.constraint_type = 'FOREIGN KEY'");
            return context.Query<PostgreSqlForeignKey>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbConstraint> GetConstraints(PostgreSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT nc.nspname AS \"Schema\",");
            query.AppendLine("       c.conname AS \"Name\",");
            query.AppendLine("       nt.nspname AS \"TableSchema\",");
            query.AppendLine("       ct.relname AS \"TableName\",");
            query.AppendLine("       ccu.COLUMN_NAME as \"ColumnName\",");
            query.AppendLine("       CASE");
            query.AppendLine("           WHEN c.contype = 'c' THEN 'CHECK'");
            query.AppendLine("           WHEN c.contype = 'u' THEN 'UNIQUE'");
            query.AppendLine("           WHEN c.contype = 'x' THEN 'EXCLUDE'");
            query.AppendLine("       END AS \"ConstraintType\",");
            query.AppendLine("       pg_catalog.pg_get_constraintdef(c.oid) AS \"Definition\"");
            query.AppendLine("FROM pg_catalog.pg_constraint c");
            query.AppendLine("JOIN pg_catalog.pg_class ct ON c.conrelid = ct.oid");
            query.AppendLine("LEFT JOIN pg_catalog.pg_class cc ON c.conindid = cc.oid");
            query.AppendLine("JOIN pg_catalog.pg_namespace nc ON c.connamespace = nc.oid");
            query.AppendLine("JOIN pg_catalog.pg_namespace nt ON ct.relnamespace = nt.oid");
            query.AppendLine("LEFT JOIN information_schema.constraint_column_usage ccu ON c.conname = ccu.constraint_name");
            query.AppendLine("WHERE c.contype IN ('c', 'u', 'x') AND contypid = 0");
            return context.Query<ABaseDbConstraint>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbIndex> GetIndexes(PostgreSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT ni.nspname AS \"Schema\",");
            query.AppendLine("       ci.relname AS \"Name\",");
            query.AppendLine("       nt.nspname AS \"TableSchema\",");
            query.AppendLine("       ct.relname AS \"TableName\",");
            query.AppendLine("       a.attname AS \"ColumnName\",");
            query.AppendLine("       CASE");
            query.AppendLine("           WHEN i.indisprimary IS TRUE THEN 'PRIMARY KEY'");
            query.AppendLine("           WHEN i.indisunique IS TRUE THEN 'UNIQUE'");
            query.AppendLine("           ELSE 'INDEX'");
            query.AppendLine("       END AS \"ConstraintType\",");
            query.AppendLine("       i.indisprimary AS \"IsPrimaryKey\",");
            query.AppendLine("       CAST(a.attnum AS bigint) AS \"OrdinalPosition\",");
            query.AppendLine("       CASE");
            query.AppendLine("           WHEN i.indoption[a.attnum-1] & 1 = 1 THEN TRUE");
            query.AppendLine("           ELSE FALSE");
            query.AppendLine("       END AS \"IsDescending\",");
            query.AppendLine("       i.indisunique AS \"IsUnique\",");
            query.AppendLine("       am.amname AS \"Type\"");
            query.AppendLine("FROM pg_catalog.pg_index i");
            query.AppendLine("JOIN pg_catalog.pg_class ct ON i.indrelid = ct.oid");
            query.AppendLine("JOIN pg_catalog.pg_class ci ON i.indexrelid = ci.oid");
            query.AppendLine("JOIN pg_catalog.pg_namespace nt ON ct.relnamespace = nt.oid");
            query.AppendLine("JOIN pg_catalog.pg_namespace ni ON ci.relnamespace = ni.oid");
            query.AppendLine("JOIN pg_catalog.pg_attribute a ON i.indexrelid = a.attrelid");
            query.AppendLine("JOIN pg_catalog.pg_am am ON ci.relam = am.oid");
            query.AppendLine("WHERE nt.nspname = 'public'");

            return context.Query<PostgreSqlIndex>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbView> GetViews(PostgreSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT TABLE_NAME as \"Name\",");
            query.AppendLine("       TABLE_SCHEMA as \"Schema\",");
            query.AppendLine("       VIEW_DEFINITION as \"ViewDefinition\",");
            query.AppendLine("       CHECK_OPTION as \"CheckOption\"");
            query.AppendLine("FROM INFORMATION_SCHEMA.VIEWS");
            query.AppendLine($"WHERE TABLE_CATALOG = '{context.DatabaseName}' AND TABLE_SCHEMA = 'public'");

            return context.Query<PostgreSqlView>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbFunction> GetFunctions(PostgreSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT n.nspname as \"Schema\",");
            query.AppendLine("       p.proname as \"Name\",");
            query.AppendLine("       p.prosrc as \"Definition\",");
            query.AppendLine("       upper(l.lanname) as \"ExternalLanguage\",");
            query.AppendLine("       CASE WHEN p.prosecdef THEN 'DEFINER' ELSE 'INVOKER' END as \"SecurityType\",");
            query.AppendLine("       p.procost as \"Cost\",");
            query.AppendLine("       p.prorows as \"Rows\",");
            query.AppendLine("       p.proisstrict as \"IsStrict\",");
            query.AppendLine("       p.proretset as \"ReturnSet\",");
            query.AppendLine("       p.provolatile as \"Volatile\",");
            query.AppendLine("       p.prorettype as \"ReturnType\",");
            query.AppendLine("       p.proargtypes as \"ArgTypes\",");
            query.AppendLine("       p.proallargtypes as \"AllArgTypes\",");
            query.AppendLine("       p.proargmodes as \"ArgModes\",");
            query.AppendLine("       p.proargnames as \"ArgNames\",");

            query.AppendLine(this.CurrentServerVersion.Major >= 11
                ? "       CASE WHEN p.prokind = 'a' THEN true ELSE false END as \"IsAggregate\","
                : "       p.proisagg as \"IsAggregate\",");

            query.AppendLine("       a.aggtransfn::regproc::name as \"AggregateTransitionFunction\",");
            query.AppendLine("       a.aggtranstype as \"AggregateTransitionType\",");
            query.AppendLine("       CASE WHEN a.aggfinalfn::regproc::oid = 0 THEN null ELSE a.aggfinalfn::regproc::name END as \"AggregateFinalFunction\",");
            query.AppendLine("       a.agginitval as \"AggregateInitialValue\"");
            query.AppendLine("FROM pg_catalog.pg_namespace n");
            query.AppendLine("INNER JOIN pg_catalog.pg_proc p ON n.oid = p.pronamespace");
            query.AppendLine("INNER JOIN pg_catalog.pg_language l ON p.prolang = l.oid");
            query.AppendLine("LEFT JOIN pg_catalog.pg_aggregate a ON p.oid = a.aggfnoid");

            if (this.CurrentServerVersion.Major >= 11)
            {
                query.AppendLine("WHERE (n.nspname = 'public' AND p.prokind != 'a' AND upper(l.lanname) != 'INTERNAL') OR");
                query.AppendLine("      (n.nspname = 'public' AND p.prokind = 'a')");
            }
            else
            {
                query.AppendLine("WHERE (n.nspname = 'public' AND p.proisagg = 'false' AND upper(l.lanname) != 'INTERNAL') OR");
                query.AppendLine("      (n.nspname = 'public' AND p.proisagg = 'true')");
            }

            return context.Query<PostgreSqlFunction>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbStoredProcedure> GetStoredProcedures(PostgreSqlDatabaseContext context)
        {
            // In PostgreSql Stored Procedures doesn't exists. Therefore we will return an empty list;
            return Enumerable.Empty<ABaseDbStoredProcedure>();
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbTrigger> GetTriggers(PostgreSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT nt.nspname AS \"Schema\",");
            query.AppendLine("       t.tgname AS \"Name\",");
            query.AppendLine("       nt.nspname AS \"TableSchema\",");
            query.AppendLine("       ct.relname AS \"TableName\",");
            query.AppendLine("       pg_catalog.pg_get_triggerdef(t.oid) AS \"Definition\"");
            query.AppendLine("FROM pg_catalog.pg_trigger t");
            query.AppendLine("JOIN pg_catalog.pg_class ct ON t.tgrelid = ct.oid");
            query.AppendLine("JOIN pg_catalog.pg_namespace nt ON ct.relnamespace = nt.oid");
            query.AppendLine("WHERE t.tgisinternal = false");

            return context.Query<PostgreSqlTrigger>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbDataType> GetDataTypes(PostgreSqlDatabaseContext context)
        {
            var commonSelect = new StringBuilder();
            commonSelect.AppendLine("SELECT n.nspname AS \"Schema\",");
            commonSelect.AppendLine("       t.typname AS \"Name\",");
            commonSelect.AppendLine("       t.oid AS \"TypeId\", ");
            commonSelect.AppendLine("       t.typarray AS \"ArrayTypeId\",");
            commonSelect.AppendLine("       CASE");
            commonSelect.AppendLine("              WHEN t.typcategory = 'A' THEN true");
            commonSelect.AppendLine("              ELSE false");
            commonSelect.AppendLine("       END AS \"IsArray\",");

            var commonWhereIsUserDefined = new StringBuilder();
            commonWhereIsUserDefined.AppendLine("WHERE (");
            commonWhereIsUserDefined.AppendLine("          (t.typrelid = 0 OR c.relkind = 'c')");
            commonWhereIsUserDefined.AppendLine("          AND NOT EXISTS (SELECT 1");
            commonWhereIsUserDefined.AppendLine("                          FROM pg_catalog.pg_type el");
            commonWhereIsUserDefined.AppendLine("                          WHERE el.oid = t.typelem AND el.typarray = t.oid)");
            commonWhereIsUserDefined.AppendLine("          AND n.nspname <> 'pg_catalog'");
            commonWhereIsUserDefined.AppendLine("          AND n.nspname <> 'information_schema'");
            commonWhereIsUserDefined.AppendLine("          AND pg_catalog.pg_type_is_visible(t.oid)");
            commonWhereIsUserDefined.Append("      ) = ");

            // System Types
            var query = new StringBuilder();
            query.Append(commonSelect);
            query.AppendLine("       false AS \"IsUserDefined\"");
            query.AppendLine("FROM pg_catalog.pg_type t");
            query.AppendLine("INNER JOIN pg_catalog.pg_namespace n ON t.typnamespace = n.oid");
            query.AppendLine("LEFT JOIN pg_catalog.pg_class c ON c.oid = t.typrelid");
            query.AppendLine($"{commonWhereIsUserDefined} false");
            var types = context.Query<PostgreSqlDataType>(query.ToString());

            // User-Defined Enumerated Types
            query = new StringBuilder();
            query.Append(commonSelect);
            query.AppendLine("       true AS \"IsUserDefined\",");
            query.AppendLine("       array_agg(e.enumlabel ORDER BY e.enumsortorder) AS \"Labels\"");
            query.AppendLine("FROM pg_catalog.pg_type t");
            query.AppendLine("INNER JOIN pg_catalog.pg_namespace n ON t.typnamespace = n.oid");
            query.AppendLine("LEFT JOIN pg_catalog.pg_class c ON c.oid = t.typrelid");
            query.AppendLine("INNER JOIN pg_catalog.pg_enum e ON e.enumtypid = t.oid");
            query.AppendLine($"{commonWhereIsUserDefined} true");
            query.AppendLine("AND t.typtype = 'e'");
            query.AppendLine("GROUP BY n.nspname, t.typname, t.oid, t.typtype, t.typcategory, t.typarray");
            types.AddRange(context.Query<PostgreSqlDataTypeEnumerated>(query.ToString()));

            // User-Defined Composite Types
            query = new StringBuilder();
            query.Append(commonSelect);
            query.AppendLine("       true AS \"IsUserDefined\",");
            query.AppendLine("       array_agg(a.attname ORDER BY a.attnum) AS \"AttributeNames\",");
            query.AppendLine("       array_agg(a.atttypid ORDER BY a.attnum) AS \"AttributeTypeIds\"");
            query.AppendLine("FROM pg_catalog.pg_type t");
            query.AppendLine("INNER JOIN pg_catalog.pg_namespace n ON t.typnamespace = n.oid");
            query.AppendLine("LEFT JOIN pg_catalog.pg_class c ON c.oid = t.typrelid");
            query.AppendLine("INNER JOIN pg_catalog.pg_attribute a ON a.attrelid = t.typrelid");
            query.AppendLine($"{commonWhereIsUserDefined} true");
            query.AppendLine("AND t.typtype = 'c'");
            query.AppendLine("GROUP BY n.nspname, t.typname, t.oid, t.typtype, t.typcategory, t.typarray");
            types.AddRange(context.Query<PostgreSqlDataTypeComposite>(query.ToString()));

            // User-Defined Range Types
            query = new StringBuilder();
            query.Append(commonSelect);
            query.AppendLine("       true AS \"IsUserDefined\",");
            query.AppendLine("       r.rngsubtype AS \"SubTypeId\",");
            query.AppendLine("       CASE WHEN r.rngcanonical::regproc::oid = 0 THEN null ELSE r.rngcanonical::regproc::name END AS \"Canonical\",");
            query.AppendLine("       CASE WHEN r.rngsubdiff::regproc::oid = 0 THEN null ELSE r.rngsubdiff::regproc::name END AS \"SubTypeDiff\"");
            query.AppendLine("FROM pg_catalog.pg_type t");
            query.AppendLine("INNER JOIN pg_catalog.pg_namespace n ON t.typnamespace = n.oid");
            query.AppendLine("LEFT JOIN pg_catalog.pg_class c ON c.oid = t.typrelid");
            query.AppendLine("INNER JOIN pg_catalog.pg_range r ON r.rngtypid = t.oid");
            query.AppendLine($"{commonWhereIsUserDefined} true");
            query.AppendLine("AND t.typtype = 'r'");
            types.AddRange(context.Query<PostgreSqlDataTypeRange>(query.ToString()));

            // User-Defined Domain Types
            query = new StringBuilder();
            query.Append(commonSelect);
            query.AppendLine("       true AS \"IsUserDefined\",");
            query.AppendLine("       t.typbasetype AS \"BaseTypeId\",");
            query.AppendLine("       t.typnotnull AS \"NotNull\",");
            query.AppendLine("       co.conname AS \"ConstraintName\",");
            query.AppendLine("       pg_catalog.pg_get_constraintdef(co.oid, true) AS \"ConstraintDefinition\"");
            query.AppendLine("FROM pg_catalog.pg_type t");
            query.AppendLine("INNER JOIN pg_catalog.pg_namespace n ON t.typnamespace = n.oid");
            query.AppendLine("LEFT JOIN pg_catalog.pg_class c ON c.oid = t.typrelid");
            query.AppendLine("LEFT JOIN pg_catalog.pg_constraint co ON t.oid = co.contypid");
            query.AppendLine($"{commonWhereIsUserDefined} true");
            query.AppendLine("AND t.typtype = 'd'");
            types.AddRange(context.Query<PostgreSqlDataTypeDomain>(query.ToString()));

            // Get all types that have an ArrayTypeId, those types need to be referenced by the array type
            // E.g.: type 'bool' has id 1 and ArrayTypeId 1000
            // type '_bool' has id 1000 and will reference type 'bool'
            var referencedTypes = types.Where(x => x.ArrayTypeId != 0).ToList();

            foreach (var t in types)
            {
                // If the current type is referenced by some other type as ArrayTypeId, set the array tape to it or null.
                t.ArrayType = referencedTypes.FirstOrDefault(x => x.ArrayTypeId == t.TypeId);
            }

            return types;
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbSequence> GetSequences(PostgreSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT s.sequence_schema AS \"Schema\",");
            query.AppendLine("       s.sequence_name AS \"Name\",");
            query.AppendLine("       s.data_type AS \"DataType\",");
            query.AppendLine("       s.start_value AS \"StartValue\",");
            query.AppendLine("       s.increment AS \"Increment\",");
            query.AppendLine("       s.minimum_value AS \"MinValue\",");
            query.AppendLine("       s.maximum_value AS \"MaxValue\",");
            query.AppendLine("       CASE WHEN s.cycle_option = 'YES' THEN true ELSE false END AS \"IsCycling\"");

            if (this.CurrentServerVersion.Major >= 10)
            {
                query.AppendLine("       ,ps.seqcache AS \"Cache\"");
            }

            query.AppendLine("FROM information_schema.sequences s");

            if (this.CurrentServerVersion.Major >= 10)
            {
                query.AppendLine("JOIN pg_catalog.pg_sequence ps ON s.sequence_name::REGCLASS::OID = ps.seqrelid");
            }

            var sequences = context.Query<PostgreSqlSequence>(query.ToString());

            if (this.CurrentServerVersion.Major < 10)
            {
                var scriptHelper = new PostgreSqlScriptHelper(new ProjectOptions());
                foreach (var sequence in sequences)
                {
                    sequence.Cache = context.QuerySingleColumn<long>($"SELECT cache_value FROM {scriptHelper.ScriptObjectName(sequence)}").First();
                }
            }

            return sequences;
        }
    }
}
