using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.PostgreSql;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Infrastructure.EntityFramework;
using System.Collections.Generic;
using System.Text;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves various information from a PostgreSQL Server
    /// </summary>
    internal class PostgreSqlDatabaseProvider
        : ADatabaseProvider<PostgreSqlDatabaseProviderOptions, PostgreSqlDatabaseContext, PostgreSqlDb, PostgreSqlTable, PostgreSqlColumn, PostgreSqlPrimaryKey, PostgreSqlForeignKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlDatabaseProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="options">The options to connect to the PostgreSQL Database</param>
        public PostgreSqlDatabaseProvider(ILoggerFactory loggerFactory, PostgreSqlDatabaseProviderOptions options)
            : base(loggerFactory, loggerFactory.CreateLogger("PostgreSqlDatabaseProvider"), options)
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
        protected override List<PostgreSqlTable> GetTables(PostgreSqlDb database, PostgreSqlDatabaseContext context)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT TABLE_NAME as \"Name\",");
            query.AppendLine("       TABLE_CATALOG as \"TableCatalog\",");
            query.AppendLine("       TABLE_SCHEMA as \"TableSchema\",");
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
        protected override List<PostgreSqlColumn> GetColumns(PostgreSqlDb database, PostgreSqlDatabaseContext context)
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine("SELECT table_catalog as \"TableCatalog\",");
            query.AppendLine("       table_schema as \"TableSchema\",");
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
            query.AppendLine("       character_set_catalog as \"CharachterSetCatalog\",");
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
        protected override List<PostgreSqlPrimaryKey> GetPrimaryKeys(PostgreSqlDb database, PostgreSqlDatabaseContext context)
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine("SELECT kcu.CONSTRAINT_CATALOG as \"ConstraintCatalog\",");
            query.AppendLine("       kcu.CONSTRAINT_SCHEMA as \"ConstraintSchema\",");
            query.AppendLine("       kcu.CONSTRAINT_NAME as \"Name\",");
            query.AppendLine("       kcu.TABLE_CATALOG as \"TableCatalog\",");
            query.AppendLine("       kcu.TABLE_SCHEMA as \"TableSchema\",");
            query.AppendLine("       kcu.TABLE_NAME as \"TableName\",");
            query.AppendLine("       kcu.COLUMN_NAME as \"ColumnName\",");
            query.AppendLine("       kcu.ORDINAL_POSITION as \"OrdinalPosition\",");
            query.AppendLine("       kcu.POSITION_IN_UNIQUE_CONSTRAINT as \"PositionInUniqueConstraint\"");
            query.AppendLine("FROM information_schema.key_column_usage kcu");
            query.AppendLine("INNER JOIN information_schema.table_constraints tc ");
            query.AppendLine("   ON tc.table_name = kcu.table_name AND tc.table_schema = kcu.table_schema AND tc.table_catalog = kcu.table_catalog AND tc.constraint_name = kcu.constraint_name");
            query.AppendLine($"WHERE kcu.TABLE_CATALOG = '{database.Name}' AND tc.constraint_type = 'PRIMARY KEY'");
            return context.Query<PostgreSqlPrimaryKey>(query.ToString());
        }

        /// <inheritdoc/>
        protected override List<PostgreSqlForeignKey> GetForeignKeys(PostgreSqlDb database, PostgreSqlDatabaseContext context)
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine("SELECT kcu.constraint_catalog AS \"ConstraintCatalog\",");
            query.AppendLine("       kcu.constraint_schema AS \"ConstraintSchema\",");
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
            query.AppendLine("       ccu.column_name AS \"ReferencedColumnName\"");
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
    }
}
