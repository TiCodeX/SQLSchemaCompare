﻿namespace TiCodeX.SQLSchemaCompare.Infrastructure.EntityFramework
{
    /// <summary>
    /// Defines the PostgresSql database context
    /// </summary>
    internal class PostgreSqlDatabaseContext : ADatabaseContext<PostgreSqlDatabaseProviderOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlDatabaseContext"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="cipherService">The injected cipher service</param>
        /// <param name="dbpo">The PostgreSql database provider options</param>
        public PostgreSqlDatabaseContext(ILoggerFactory loggerFactory, ICipherService cipherService, PostgreSqlDatabaseProviderOptions dbpo)
            : base(loggerFactory, dbpo)
        {
            var connStr = $"Server={dbpo.Hostname};Port={dbpo.Port};Database={dbpo.Database};User Id={dbpo.Username};Password={cipherService.DecryptString(dbpo.Password)};";

            if (dbpo.UseSsl)
            {
                connStr += "SSL Mode=Require;";

                if (dbpo.IgnoreServerCertificate)
                {
                    connStr += "Trust Server Certificate=true;";
                }
            }

            this.ConnectionString = connStr;
        }

        /// <summary>
        /// Gets the string used for the connection
        /// </summary>
        private string ConnectionString { get; }

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseNpgsql(this.ConnectionString);
        }
    }
}
