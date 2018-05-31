﻿namespace SQLCompare.Core.Enums
{
    /// <summary>
    /// List of possible sources for retrieving database data
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// Microsoft SQL Server
        /// </summary>
        MicrosoftSql = 0,

        /// <summary>
        /// Oracle MySQL
        /// </summary>
        MySql = 1,

        /// <summary>
        /// PostgreSQL
        /// </summary>
        PostgreSql = 2
    }
}