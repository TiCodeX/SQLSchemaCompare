using SQLCompare.Core.Entities.Database;
using System.Collections.Generic;

namespace SQLCompare.Core.Entities.Compare
{
    /// <summary>
    /// Represent the result of a comparison
    /// </summary>
    public class CompareResult
    {
        /// <summary>
        /// Gets the list of Table results
        /// </summary>
        public List<CompareResultItem<ABaseDbTable>> Tables { get; } = new List<CompareResultItem<ABaseDbTable>>();

        // public List<CompareResultItem<ABaseDbView>> Views { get; set; }
    }
}
