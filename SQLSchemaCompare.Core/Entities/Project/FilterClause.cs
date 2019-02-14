using TiCodeX.SQLSchemaCompare.Core.Enums;

namespace TiCodeX.SQLSchemaCompare.Core.Entities.Project
{
    /// <summary>
    /// Represent a single filtering clause
    /// </summary>
    public class FilterClause
    {
        /// <summary>
        /// Gets or sets the group which will be used to join with an AND those
        /// of the same group and with OR between different groups
        /// </summary>
        public int Group { get; set; }

        /// <summary>
        /// Gets or sets the object type
        /// </summary>
        public DatabaseObjectType? ObjectType { get; set; }

        /// <summary>
        /// Gets or sets the field to filter
        /// </summary>
        public FilterField Field { get; set; }

        /// <summary>
        /// Gets or sets the filter operator
        /// </summary>
        public FilterOperator Operator { get; set; }

        /// <summary>
        /// Gets or sets the filtering value
        /// </summary>
        public string Value { get; set; }
    }
}
