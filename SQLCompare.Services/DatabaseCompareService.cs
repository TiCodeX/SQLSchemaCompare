using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Interfaces.Services;
using System;

namespace SQLCompare.Services
{
    /// <summary>
    /// Implementation that provides the mechanisms to compare two database instances
    /// </summary>
    public class DatabaseCompareService : IDatabaseCompareService
    {
        /// <inheritdoc />
        public bool CompareTable(ABaseDbTable source, ABaseDbTable target)
        {
            if (source == null && target == null)
            {
                throw new ArgumentException("Both arguments are null");
            }

            if (source == null || target == null)
            {
                return false;
            }

            if (source.Columns.Count != target.Columns.Count)
            {
                return false;
            }

            if (source.PrimaryKeys.Count != target.PrimaryKeys.Count)
            {
                return false;
            }

            return source.ForeignKeys.Count == target.ForeignKeys.Count;
        }
    }
}
