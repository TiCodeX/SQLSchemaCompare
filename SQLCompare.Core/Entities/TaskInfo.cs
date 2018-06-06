using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SQLCompare.Core.Entities
{
    /// <summary>
    /// Information about a Task
    /// </summary>
    public class TaskInfo
    {
        /// <summary>
        /// Gets the id of the Task
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the name of the Task
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the status of the Task
        /// </summary>
        public TaskStatus Status { get; set; } = TaskStatus.Created;

        /// <summary>
        /// Gets or sets the message of the Task
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the percentage of the Task
        /// </summary>
        public short Percentage { get; set; }

        /// <summary>
        /// Gets the time when the Task started
        /// </summary>
        public DateTime StartTime { get; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the time when the Task is finished
        /// </summary>
        public DateTime CompleteTime { get; set; }

        /// <summary>
        /// Gets the list of child Tasks
        /// </summary>
        public List<TaskInfo> Tasks { get; } = new List<TaskInfo>();
    }
}
