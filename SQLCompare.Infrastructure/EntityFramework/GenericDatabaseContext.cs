using Microsoft.EntityFrameworkCore;
using SQLCompare.Core.Entities.EntityFramework;

namespace SQLCompare.Infrastructure.EntityFramework
{

    internal class GenericDatabaseContext : DbContext
    {
        protected string DatabaseName { get; }
        protected string ConnectionString { get; }

        public GenericDatabaseContext(string server, string databaseName, string username, string password)
        {
            DatabaseName = databaseName;
            ConnectionString = $"Server={server};Database={DatabaseName};User Id={username};Password={password}";
        }

        public virtual DbSet<InformationSchemaTable> Tables { get; protected set; }
        public virtual DbSet<InformationSchemaColumn> Columns { get; protected set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var table = modelBuilder.Entity<InformationSchemaTable>();
            table.ToTable("tables", "information_schema");
            table.HasKey(a => new { a.TableName, a.TableCatalog, a.TableSchema });
            table.HasQueryFilter(x => string.Equals(x.TableType, "BASE TABLE", System.StringComparison.Ordinal));

            var column = modelBuilder.Entity<InformationSchemaColumn>();
            column.ToTable("columns", "information_schema");
            column.HasKey(a => new { a.TableName, a.TableCatalog, a.TableSchema, a.ColumnName });

            //Create table/column relationship
            table.HasMany(x => x.Columns).WithOne(x => x.Table).HasForeignKey(a => new { a.TableName, a.TableCatalog, a.TableSchema });
        }

    }
}
