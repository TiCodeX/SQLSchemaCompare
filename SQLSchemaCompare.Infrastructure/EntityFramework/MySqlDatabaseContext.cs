﻿namespace TiCodeX.SQLSchemaCompare.Infrastructure.EntityFramework
{
    /// <summary>
    /// Defines the MySql database context
    /// </summary>
    internal class MySqlDatabaseContext : ADatabaseContext<MySqlDatabaseProviderOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlDatabaseContext"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        /// <param name="cipherService">The injected cipher service</param>
        /// <param name="dbpo">The MySql database provider options</param>
        public MySqlDatabaseContext(ILoggerFactory loggerFactory, ICipherService cipherService, MySqlDatabaseProviderOptions dbpo)
            : base(loggerFactory, dbpo)
        {
            var connStr = $"Server={dbpo.Hostname};Port={dbpo.Port};Database={dbpo.Database};User Id={dbpo.Username};Password={cipherService.DecryptString(dbpo.Password)};";

            connStr += dbpo.UseSsl
                ? dbpo.IgnoreServerCertificate
                    ? "SslMode=Required;"
                    : "SslMode=VerifyFull;"
                : "SslMode=None;AllowPublicKeyRetrieval=true;";

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
            optionsBuilder.UseMySql(this.ConnectionString, ServerVersion.AutoDetect(this.ConnectionString));
        }
    }
}
