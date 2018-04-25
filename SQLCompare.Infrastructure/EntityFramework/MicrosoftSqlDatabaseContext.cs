using Microsoft.EntityFrameworkCore;

namespace SQLCompare.Infrastructure.EntityFramework
{
    internal class MicrosoftSqlDatabaseContext : GenericDatabaseContext
    {
        public MicrosoftSqlDatabaseContext(string server, string databaseName, string username, string password)
            : base(server, databaseName, username, password)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionStr = $"Server={Server};Database={DatabaseName};User Id={Username};Password={Password}";
            optionsBuilder.UseSqlServer(connectionStr);
        }
    }
}
