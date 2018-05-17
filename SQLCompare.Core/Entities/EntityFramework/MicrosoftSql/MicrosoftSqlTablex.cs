using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SQLCompare.Core.Entities.EntityFramework.MicrosoftSql
{
    /// <summary>
    /// Database entity class that describes Microsoft Sql sys.objects view
    /// </summary>
    public class MicrosoftSqlTablex
    {
#pragma warning disable SA1600 // Elements must be documented
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        [Column("name")]
        public string Name { get; set; }

        [Column("schema_id")]

        public string SchemaName { get; set; }

        [Column("parent_object_id")]
        public string Catalog { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("type_desc")]
        public string TypeDescription { get; set; }

        [Column("create_date")]
        public DateTime? CreateDate { get; set; }

        [Column("modify_date")]
        public DateTime? ModifyDate { get; set; }

        public virtual InformationSchemaTable Table { get; set; }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements must be documented
    }
}
