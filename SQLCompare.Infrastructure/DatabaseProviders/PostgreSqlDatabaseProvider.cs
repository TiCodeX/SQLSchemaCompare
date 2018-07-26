using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.PostgreSql;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Infrastructure.EntityFramework;

namespace SQLCompare.Infrastructure.DatabaseProviders
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
        /// <param name="options">The options to connect to the PostgreSQL Database</param>
        public PostgreSqlDatabaseProvider(ILoggerFactory loggerFactory, PostgreSqlDatabaseProviderOptions options)
            : base(loggerFactory, loggerFactory.CreateLogger(nameof(PostgreSqlDatabaseProvider)), options)
        {
        }

        /// <inheritdoc />
        public override ABaseDb GetDatabase()
        {
            using (var context = new PostgreSqlDatabaseContext(this.LoggerFactory, this.Options))
            {
                return this.DiscoverDatabase(context);
            }
        }

        /// <inheritdoc />
        public override List<string> GetDatabaseList()
        {
            using (var context = new PostgreSqlDatabaseContext(this.LoggerFactory, this.Options))
            {
                return context.Query("SELECT datname FROM pg_database WHERE datistemplate = FALSE");
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ABaseDbTable> GetTables(PostgreSqlDb database, PostgreSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT TABLE_NAME as \"Name\",");
            query.AppendLine("       TABLE_CATALOG as \"Catalog\",");
            query.AppendLine("       TABLE_SCHEMA as \"Schema\",");
            query.AppendLine("       self_referencing_column_name as \"SelfReferencingColumnName\",");
            query.AppendLine("       reference_generation as \"ReferenceGeneration\",");
            query.AppendLine("       user_defined_type_catalog as \"UserDefinedTypeCatalog\",");
            query.AppendLine("       user_defined_type_schema as \"UserDefinedTypeSchema\",");
            query.AppendLine("       user_defined_type_name as \"UserDefinedTypeName\",");
            query.AppendLine("       CASE ");
            query.AppendLine("            WHEN is_insertable_into = 'YES' THEN true");
            query.AppendLine("            ELSE false");
            query.AppendLine("            END as \"IsInsertableInto\",");
            query.AppendLine("       CASE ");
            query.AppendLine("            WHEN is_typed = 'YES' THEN true");
            query.AppendLine("            ELSE false");
            query.AppendLine("            END as \"IsTyped\",");
            query.AppendLine("       commit_action as \"CommitAction\"");
            query.AppendLine("FROM INFORMATION_SCHEMA.TABLES");
            query.AppendLine("WHERE TABLE_TYPE = 'BASE TABLE' and TABLE_SCHEMA = 'public'");

            return context.Query<PostgreSqlTable>(query.ToString());
        }

        /// <inheritdoc />
        protected override IEnumerable<ABaseDbColumn> GetColumns(PostgreSqlDb database, PostgreSqlDatabaseContext context)
        {
            var query = new StringBuilder();

            query.AppendLine("SELECT table_catalog as \"Catalog\",");
            query.AppendLine("       table_schema as \"Schema\",");
            query.AppendLine("       table_name as \"TableName\",");
            query.AppendLine("       column_name as \"Name\",");
            query.AppendLine("       ordinal_position as \"OrdinalPosition\",");
            query.AppendLine("       column_default as \"ColumnDefault\",");
            query.AppendLine("       CASE ");
            query.AppendLine("            WHEN is_nullable = 'YES' THEN true");
            query.AppendLine("            ELSE false");
            query.AppendLine("            END as \"IsNullable\",");
            query.AppendLine("       data_type as \"DataType\",");
            query.AppendLine("       character_maximum_length as \"CharacterMaxLenght\",");
            query.AppendLine("       character_octet_length as \"CharacterOctetLenght\",");
            query.AppendLine("       numeric_precision as \"NumericPrecision\",");
            query.AppendLine("       numeric_precision_radix as \"NumericPrecisionRadix\",");
            query.AppendLine("       numeric_scale as \"NumericScale\",");
            query.AppendLine("       datetime_precision as \"DateTimePrecision\",");
            query.AppendLine("       interval_type as \"IntervalType\",");
            query.AppendLine("       interval_precision as \"IntervalPrecision\",");
            query.AppendLine("       character_set_catalog as \"CharacterSetCatalog\",");
            query.AppendLine("       character_set_schema as \"CharacterSetSchema\",");
            query.AppendLine("       character_set_name as \"CharacterSetName\",");
            query.AppendLine("       collation_catalog as \"CollationCatalog\",");
            query.AppendLine("       collation_schema as \"CollationSchema\",");
            query.AppendLine("       collation_name as \"CollationName\",");
            query.AppendLine("       domain_catalog as \"DomainCatalog\",");
            query.AppendLine("       domain_schema as \"DomainSchema\",");
            query.AppendLine("       domain_name as \"DomainName\",");
            query.AppendLine("       udt_catalog as \"UdtCatalog\",");
            query.AppendLine("       udt_schema as \"UdtSchema\",");
            query.AppendLine("       udt_name as \"UdtName\",");
            query.AppendLine("       scope_catalog as \"ScopeCatalog\",");
            query.AppendLine("       scope_schema as \"ScopeSchema\",");
            query.AppendLine("       scope_name as \"ScopeName\",");
            query.AppendLine("       maximum_cardinality as \"MaximumCardinality\",");
            query.AppendLine("       dtd_identifier as \"DtdIdentifier\",");
            query.AppendLine("       CASE ");
            query.AppendLine("            WHEN is_self_referencing = 'YES' THEN true");
            query.AppendLine("            ELSE false");
            query.AppendLine("            END as \"IsSelfReferencing\",");
            query.AppendLine("       CASE ");
            query.AppendLine("            WHEN is_identity = 'YES' THEN true");
            query.AppendLine("            ELSE false");
            query.AppendLine("            END as \"IsIdentity\",");
            query.AppendLine("       identity_generation as \"IdentityGeneration\",");
            query.AppendLine("       identity_start as \"IdentitiyStart\",");
            query.AppendLine("       identity_increment as \"IdentityIncrement\",");
            query.AppendLine("       identity_maximum as \"IdentityMaximum\",");
            query.AppendLine("       identity_minimum as \"IdentityMinimum\",");
            query.AppendLine("       identity_cycle as \"IdentityCycle\",");
            query.AppendLine("       CASE ");
            query.AppendLine("            WHEN is_generated = 'YES' THEN true");
            query.AppendLine("            ELSE false");
            query.AppendLine("            END as \"IsGenerated\",");
            query.AppendLine("       generation_expression as \"GenerationExpression\",");
            query.AppendLine("       CASE ");
            query.AppendLine("            WHEN is_updatable = 'YES' THEN true");
            query.AppendLine("            ELSE false");
            query.AppendLine("            END as \"IsUpdatable\"");
            query.AppendLine("FROM INFORMATION_SCHEMA.COLUMNS");
            query.AppendLine($"WHERE TABLE_CATALOG = '{database.Name}'");

            return context.Query<PostgreSqlColumn>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbConstraint> GetForeignKeys(PostgreSqlDb database, PostgreSqlDatabaseContext context)
        {
            var query = new StringBuilder();

            query.AppendLine("SELECT kcu.constraint_catalog AS \"Catalog\",");
            query.AppendLine("       kcu.constraint_schema AS \"Schema\",");
            query.AppendLine("       kcu.constraint_name AS \"Name\",");
            query.AppendLine("       kcu.table_catalog AS \"TableCatalog\",");
            query.AppendLine("       kcu.table_schema AS \"TableSchema\",");
            query.AppendLine("       kcu.table_name AS \"TableName\",");
            query.AppendLine("       kcu.column_name AS \"ColumnName\",");
            query.AppendLine("       kcu.ordinal_position AS \"OrdinalPosition\",");
            query.AppendLine("       kcu.position_in_unique_constraint AS \"PositionInUniqueConstraint\",");
            query.AppendLine("       rc.match_option AS \"MatchOption\",");
            query.AppendLine("       rc.update_rule AS \"UpdateRule\",");
            query.AppendLine("       rc.delete_rule AS \"DeleteRule\",");
            query.AppendLine("       ccu.table_catalog AS \"ReferencedTableCatalog\",");
            query.AppendLine("       ccu.table_schema AS \"ReferencedTableSchema\",");
            query.AppendLine("       ccu.table_name AS \"ReferencedTableName\",");
            query.AppendLine("       ccu.column_name AS \"ReferencedColumnName\",");
            query.AppendLine("       CASE ");
            query.AppendLine("            WHEN tu.is_deferrable = 'YES' THEN true");
            query.AppendLine("            ELSE false");
            query.AppendLine("            END AS \"IsDeferrable\",");
            query.AppendLine("       CASE ");
            query.AppendLine("            WHEN tu.initially_deferred = 'YES' THEN true");
            query.AppendLine("            ELSE false");
            query.AppendLine("            END AS \"IsInitiallyDeferred\"");
            query.AppendLine("FROM information_schema.key_column_usage kcu");
            query.AppendLine("INNER JOIN information_schema.table_constraints tu");
            query.AppendLine("    ON kcu.constraint_name = tu.constraint_name AND kcu.constraint_schema = tu.constraint_schema AND kcu.constraint_catalog = tu.constraint_catalog");
            query.AppendLine("INNER JOIN information_schema.referential_constraints rc");
            query.AppendLine("    ON kcu.constraint_name = rc.constraint_name AND kcu.constraint_schema = rc.constraint_schema AND kcu.constraint_catalog = rc.constraint_catalog");
            query.AppendLine("INNER JOIN information_schema.constraint_column_usage ccu");
            query.AppendLine("    ON kcu.constraint_name = ccu.constraint_name AND kcu.constraint_schema = ccu.constraint_schema AND kcu.constraint_catalog = ccu.constraint_catalog");
            query.AppendLine($"WHERE kcu.constraint_catalog = '{database.Name}' AND tu.constraint_type = 'FOREIGN KEY'");
            return context.Query<PostgreSqlForeignKey>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbIndex> GetIndexes(PostgreSqlDb db, PostgreSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT (current_database())::information_schema.sql_identifier AS \"Catalog\",");
            query.AppendLine("       ni.nspname AS \"Schema\",");
            query.AppendLine("       ci.relname AS \"Name\",");
            query.AppendLine("       (current_database())::information_schema.sql_identifier AS \"TableCatalog\",");
            query.AppendLine("       nt.nspname AS \"TableSchema\",");
            query.AppendLine("       ct.relname AS \"TableName\",");
            query.AppendLine("       a.attname AS \"ColumnName\",");
            query.AppendLine("       i.indisprimary AS \"IsPrimaryKey\",");
            query.AppendLine("       a.attnum AS \"OrdinalPosition\",");
            query.AppendLine("       CASE WHEN i.indoption[a.attnum-1] & 1 = 1 THEN TRUE ELSE FALSE END AS \"IsDescending\",");
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
        protected override IEnumerable<ABaseDbView> GetViews(PostgreSqlDb database, PostgreSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT TABLE_NAME as \"Name\",");
            query.AppendLine("       TABLE_CATALOG as \"Catalog\",");
            query.AppendLine("       TABLE_SCHEMA as \"Schema\",");
            query.AppendLine("       VIEW_DEFINITION as \"ViewDefinition\",");
            query.AppendLine("       CHECK_OPTION as \"CheckOption\"");
            query.AppendLine("FROM INFORMATION_SCHEMA.VIEWS");
            query.AppendLine($"WHERE TABLE_CATALOG = '{database.Name}' AND TABLE_SCHEMA = 'public'");

            return context.Query<PostgreSqlView>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbRoutine> GetFunctions(PostgreSqlDb database, PostgreSqlDatabaseContext context)
        {
            var query = new StringBuilder();
            query.AppendLine("SELECT r.routine_catalog as \"Catalog\",");
            query.AppendLine("       r.routine_schema as \"Schema\",");
            query.AppendLine("       r.routine_name as \"Name\",");
            query.AppendLine("       r.routine_definition as \"RoutineDefinition\",");
            query.AppendLine("       r.external_language as \"ExternalLanguage\",");
            query.AppendLine("       r.security_type as \"SecurityType\",");
            query.AppendLine("       p.procost as \"Cost\",");
            query.AppendLine("       p.prorows as \"Rows\",");
            query.AppendLine("       p.proisstrict as \"IsStrict\",");
            query.AppendLine("       p.proretset as \"ReturnSet\",");
            query.AppendLine("       p.provolatile as \"Volatile\",");
            query.AppendLine("       p.pronargs as \"ArgsCount\",");
            query.AppendLine("       p.prorettype as \"ReturnType\",");
            query.AppendLine("       p.proargtypes as \"ArgTypes\",");
            query.AppendLine("       p.proallargtypes as \"AllArgTypes\",");
            query.AppendLine("       p.proargmodes as \"ArgModes\",");
            query.AppendLine("       p.proargnames as \"ArgNames\"");
            query.AppendLine("FROM information_schema.routines r");
            query.AppendLine("INNER JOIN pg_proc p");
            query.AppendLine("    ON r.routine_name = p.proname");
            query.AppendLine($"WHERE routine_catalog = '{database.Name}' AND r.routine_schema = 'public' AND p.proisagg = 'false'");
            return context.Query<PostgreSqlFunction>(query.ToString());
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbRoutine> GetStoreProcedures(PostgreSqlDb database, PostgreSqlDatabaseContext context)
        {
            // In PostgreSql Store Procedures doesn't exists. Therefore we will return an empty list;
            return Enumerable.Empty<ABaseDbRoutine>();
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbObject> GetDataTypes(PostgreSqlDb database, PostgreSqlDatabaseContext context)
        {
            var query = new StringBuilder();

            query.AppendLine("SELECT current_database()::information_schema.sql_identifier AS \"Catalog\", ");
            query.AppendLine("        nc.nspname AS \"Schema\", ");
            query.AppendLine("        a.oid AS \"TypeId\", ");
            query.AppendLine("        a.typname AS \"Name\",");
            query.AppendLine("        a.typarray AS \"ArrayTypeId\",");
            query.AppendLine("       CASE");
            query.AppendLine("              WHEN a.typcategory = 'A' THEN true");
            query.AppendLine("              ELSE false");
            query.AppendLine("       END AS \"IsArray\"");
            query.AppendLine("FROM pg_type a");
            query.AppendLine("INNER JOIN pg_namespace nc ON a.typnamespace = nc.oid");
            var types = context.Query<PostgreSqlDataType>(query.ToString());

            // Get all types that have an ArrayTypeId, those types need to be referenced by the array type
            // E.g.: type 'bool' has id 1 and ArrayTypeId 1000
            // type '_bool' has id 1000 and will reference type 'bool'
            var referencedTypes = types.Where(x => x.ArrayTypeId != 0);

            foreach (var t in types)
            {
                // If the current type is referenced by some other type as ArrayTypeId, set the array tape to it or null.
                t.ArrayType = referencedTypes.FirstOrDefault(x => x.ArrayTypeId == t.TypeId);
            }

            return types;
        }
    }
}
