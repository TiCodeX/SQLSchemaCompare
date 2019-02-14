using TiCodeX.SQLSchemaCompare.Core.Entities;
using TiCodeX.SQLSchemaCompare.Core.Entities.Database;

namespace TiCodeX.SQLSchemaCompare.Core.Interfaces
{
    /// <summary>
    /// Defines a class that provides the mechanism to map 2 databases
    /// </summary>
    public interface IDatabaseMapper
    {
        /// <summary>
        /// Perform the mapping of 2 databases
        /// </summary>
        /// <param name="source">The source database</param>
        /// <param name="target">The target database</param>
        /// <param name="mappingTable">The mapping table</param>
        /// <param name="taskInfo">The task information</param>
        void PerformMapping(ABaseDb source, ABaseDb target, object mappingTable, TaskInfo taskInfo);
    }
}
