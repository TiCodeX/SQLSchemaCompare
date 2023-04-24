﻿namespace TiCodeX.SQLSchemaCompare.Core.Entities.Database.PostgreSql
{
    using System.Collections.Generic;

    /// <summary>
    /// Specific PostgreSql Enum data type definition
    /// </summary>
    public class PostgreSqlDataTypeEnumerated : PostgreSqlDataType
    {
        /// <summary>
        /// Gets or sets the labels
        /// </summary>
        public IEnumerable<string> Labels { get; set; }
    }
}
