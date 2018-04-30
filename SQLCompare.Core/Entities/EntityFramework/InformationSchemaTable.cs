using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQLCompare.Core.Entities.EntityFramework
{
    public class InformationSchemaTable
    {
        [Column("table_catalog")]
        public string TableCatalog { get; set; }

        [Column("table_schema")]
        public string TableSchema { get; set; }

        [Column("table_type")]
        public string TableType { get; set; }

        [Column("table_name")]
        public string TableName { get; set; }

        public ICollection<InformationSchemaColumn> Columns { get; set; }
    }
}
