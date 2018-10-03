using System;
using System.Threading;
using System.Threading.Tasks;

namespace SQLCompare.Core.Entities
{
    /// <summary>
    /// Information about a Task
    /// </summary>
    public class TaskInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskInfo"/> class.
        /// </summary>
        /// <param name="name">The name of the Task</param>
        public TaskInfo(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the id of the Task
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the name of the Task
        /// </summary>
        public string Name { get; }

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
        /// Gets or sets the cancellation token
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Gets or sets the exception in case of faulted task
        /// </summary>
        public Exception Exception { get; set; }
    }
}
