namespace TiCodeX.SQLSchemaCompare.Core.Entities.Project
{
    using System.Collections.Generic;

    /// <summary>
    /// The filtering options for the project
    /// </summary>
    public class FilteringOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to consider the clauses as include or exclude
        /// </summary>
        public bool Include { get; set; }

        /// <summary>
        /// Gets the list of filter clauses
        /// </summary>
        public List<FilterClause> Clauses { get; } = new List<FilterClause>();
    }
}
