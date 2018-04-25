using Microsoft.EntityFrameworkCore;
using SQLCompare.Core.Entities.EntityFramework;

namespace SQLCompare.Infrastructure.EntityFramework
{
    public class GenericDatabaseContext : DbContext
    {
        protected string Server;
        protected string DatabaseName;
        protected string Username;
        protected string Password;

        public GenericDatabaseContext(string server, string databaseName, string username, string password)
        {
            Server = server;
            DatabaseName = databaseName;
            Username = username;
            Password = password;
        }

        public virtual DbSet<InformationSchemaTable> Tables { get; set; }
        public virtual DbSet<InformationSchemaColumn> Columns { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var table = modelBuilder.Entity<InformationSchemaTable>();
            table.ToTable("tables", "information_schema");
            table.HasKey(a => new { a.TableName, a.TableCatalog, a.TableSchema });
            table.HasQueryFilter(x => x.TableType == "BASE TABLE");

            var column = modelBuilder.Entity<InformationSchemaColumn>();
            column.ToTable("columns", "information_schema");
            column.HasKey(a => new { a.TableName, a.TableCatalog, a.TableSchema, a.ColumnName });

            //Create table/column relationship
            table.HasMany(x => x.Columns).WithOne(x => x.Table).HasForeignKey(a => new { a.TableName, a.TableCatalog, a.TableSchema });
        }

    }
}
