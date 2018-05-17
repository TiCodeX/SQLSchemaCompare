using SQLCompare.Core.Entities.EntityFramework.MicrosoftSql;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQLCompare.Core.Entities.EntityFramework
{
    /// <summary>
    /// Database entity class that describes INFORMATION_SCHEMA.TABLE view
    /// </summary>
    public class InformationSchemaTable
    {
        [Column("table_catalog")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements must be documented
        public string TableCatalog { get; set; }

        [Column("table_schema")]
        public string TableSchema { get; set; }

        [Column("table_type")]
        public string TableType { get; set; }

        [Column("table_name")]
        public string TableName { get; set; }

        public ICollection<InformationSchemaColumn> Columns { get; set; }

#pragma warning restore SA1600 // Elements must be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
