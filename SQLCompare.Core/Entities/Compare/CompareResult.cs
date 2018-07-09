using System.Collections.Generic;
using SQLCompare.Core.Entities.Database;

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

        /// <summary>
        /// Gets the list of View results
        /// </summary>
        public List<CompareResultItem<ABaseDbView>> Views { get; } = new List<CompareResultItem<ABaseDbView>>();
    }
}
