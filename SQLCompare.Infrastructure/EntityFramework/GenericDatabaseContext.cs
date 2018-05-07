using Microsoft.EntityFrameworkCore;
using SQLCompare.Core.Entities.EntityFramework;
using System.Collections.Generic;

namespace SQLCompare.Infrastructure.EntityFramework
{
    /// <inheritdoc />
    /// <summary>
    /// Common EF database context
    /// </summary>
    internal class GenericDatabaseContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDatabaseContext"/> class.
        /// </summary>
        /// <param name="server">The IP address/hostname of the server</param>
        /// <param name="databaseName">The name of the database</param>
        /// <param name="username">The username for the login</param>
        /// <param name="password">The password for the login</param>
        protected GenericDatabaseContext(string server, string databaseName, string username, string password)
        {
            this.DatabaseName = databaseName;
            this.ConnectionString = $"Server={server};Database={this.DatabaseName};User Id={username};Password={password}";
        }

        /// <summary>
        /// Gets or sets the tables of the InformationSchema
        /// </summary>
        public virtual DbSet<InformationSchemaTable> Tables { get; protected set; }

        /// <summary>
        /// Gets or sets the columns of the InformationSchema
        /// </summary>
        public virtual DbSet<InformationSchemaColumn> Columns { get; protected set; }

        /// <summary>
        /// Gets the name of the database
        /// </summary>
        protected string DatabaseName { get; }

        /// <summary>
        /// Gets the string used for the connection
        /// </summary>
        protected string ConnectionString { get; }

        /// <summary>
        /// Performs a query
        /// </summary>
        /// <typeparam name="T">The type of the result</typeparam>
        /// <param name="query">The SQL query</param>
        /// <returns>The list of specified type</returns>
        public List<T> Query<T>(string query)
            where T : new()
        {
            var result = new List<T>();
            this.Database.OpenConnection();
            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var t = new T();
                        var type = t.GetType();
                        for (var inc = 0; inc < reader.FieldCount; inc++)
                        {
                            var prop = type.GetProperty(reader.GetName(inc));
                            prop.SetValue(t, reader.GetValue(inc), null);
                        }

                        result.Add(t);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Performs a query
        /// </summary>
        /// <param name="query">The SQL query</param>
        /// <returns>The list of the first column</returns>
        public List<string> Query(string query)
        {
            var result = new List<string>();
            this.Database.OpenConnection();
            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var table = modelBuilder.Entity<InformationSchemaTable>();
            table.ToTable("tables", "information_schema");
            table.HasKey(a => new { a.TableName, a.TableCatalog, a.TableSchema });
            table.HasQueryFilter(x => string.Equals(x.TableType, "BASE TABLE", System.StringComparison.Ordinal));

            var column = modelBuilder.Entity<InformationSchemaColumn>();
            column.ToTable("columns", "information_schema");
            column.HasKey(a => new { a.TableName, a.TableCatalog, a.TableSchema, a.ColumnName });

            // Create table/column relationship
            table.HasMany(x => x.Columns).WithOne(x => x.Table).HasForeignKey(a => new { a.TableName, a.TableCatalog, a.TableSchema });
        }
    }
}
