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
        /// Gets or sets the source full script
        /// </summary>
        public string SourceFullScript { get; set; }

        /// <summary>
        /// Gets or sets the target full script
        /// </summary>
        public string TargetFullScript { get; set; }

        /// <summary>
        /// Gets the list of Table results
        /// </summary>
        public List<CompareResultItem<ABaseDbTable>> Tables { get; } = new List<CompareResultItem<ABaseDbTable>>();

        /// <summary>
        /// Gets the list of View results
        /// </summary>
        public List<CompareResultItem<ABaseDbView>> Views { get; } = new List<CompareResultItem<ABaseDbView>>();

        /// <summary>
        /// Gets the list of Function results
        /// </summary>
        public List<CompareResultItem<ABaseDbFunction>> Functions { get; } = new List<CompareResultItem<ABaseDbFunction>>();

        /// <summary>
        /// Gets the list of Stored Procedures results
        /// </summary>
        public List<CompareResultItem<ABaseDbStoredProcedure>> StoredProcedures { get; } = new List<CompareResultItem<ABaseDbStoredProcedure>>();

        /// <summary>
        /// Gets the list of Sequences results
        /// </summary>
        public List<CompareResultItem<ABaseDbSequence>> Sequences { get; } = new List<CompareResultItem<ABaseDbSequence>>();

        /// <summary>
        /// Gets the list of user defined types results
        /// </summary>
        public List<CompareResultItem<ABaseDbDataType>> DataTypes { get; } = new List<CompareResultItem<ABaseDbDataType>>();
    }
}
