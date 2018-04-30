using System.ComponentModel.DataAnnotations.Schema;

namespace SQLCompare.Core.Entities.EntityFramework
{
    public class InformationSchemaColumn
    {
        [Column("table_catalog")]
        public string TableCatalog { get; set; }

        [Column("table_schema")]
        public string TableSchema { get; set; }

        [Column("table_name")]
        public string TableName { get; set; }

        [Column("column_name")]
        public string ColumnName { get; set; }

        [Column("ordinal_position")]
        public int? OrdinalPosition { get; set; }

        [Column("column_default")]
        public string ColumnDefault { get; set; }

        [Column("is_nullable")]
        public string IsNullable { get; set; }

        [Column("data_type")]
        public string DataType { get; set; }

        [Column("character_maximum_length")]
        public int? CharacterMaximumLength { get; set; }

        [Column("character_octet_length")]
        public int? CharacterOctectLength { get; set; }

        [Column("numeric_scale")]
        public int? NumericScale { get; set; }

        [Column("datetime_precision")]
        public short? DatetimePrecision { get; set; }

        [Column("character_set_name")]
        public string CharacterSetName { get; set; }

        [Column("collation_name")]
        public string CollationName { get; set; }

        public InformationSchemaTable Table { get; set; }
    }
}
