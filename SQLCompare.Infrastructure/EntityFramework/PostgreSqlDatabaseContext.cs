using Microsoft.EntityFrameworkCore;
using SQLCompare.Core.Entities.EntityFramework;

namespace SQLCompare.Infrastructure.EntityFramework
{
    internal class PostgreSqlDatabaseContext : GenericDatabaseContext
    {
        public PostgreSqlDatabaseContext(string server, string databaseName, string username, string password)
            : base(server, databaseName, username, password)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var table = modelBuilder.Entity<InformationSchemaTable>();
            table.HasQueryFilter(x => x.TableType == "BASE TABLE" && x.TableSchema == "public");
        }
    }
}
