using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.DatabaseProvider;
using SQLCompare.Infrastructure.EntityFramework;
using System.Collections.Generic;
using System.Text;

namespace SQLCompare.Infrastructure.DatabaseProviders
{
    /// <summary>
    /// Retrieves various information from a PostgreSQL Server
    /// </summary>
    internal class PostgreSqlDatabaseProvider : ADatabaseProvider<PostgreSqlDatabaseProviderOptions, PostgreSqlDatabaseContext, PostgreSqlDb, PostgreSqlTable, PostgreSqlColumn>
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
            query.AppendLine("       TABLE_CATALOG as \"CatalogName\",");
            query.AppendLine("       TABLE_SCHEMA as \"SchemaName\",");
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

            query.AppendLine("SELECT table_catalog as \"CatalogName\",");
            query.AppendLine("       table_schema as \"SchemaName\",");
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
    }
}
