namespace TiCodeX.SQLSchemaCompare.Core.Entities
{
    using System;

    /// <summary>
    /// Defines a Task and it's work to do
    /// </summary>
    public class TaskWork
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskWork"/> class.
        /// </summary>
        /// <param name="info">The Task information</param>
        /// <param name="runInParallel">Perform the Task in parallel</param>
        /// <param name="work">The work that the Task has to do</param>
        public TaskWork(TaskInfo info, bool runInParallel, Func<TaskInfo, bool> work)
        {
            this.Info = info;
            this.RunInParallel = runInParallel;
            this.Work = work;
        }

        /// <summary>
        /// Gets the Task information
        /// </summary>
        public TaskInfo Info { get; }

        /// <summary>
        /// Gets a value indicating whether to run the Task is parallel
        /// </summary>
        public bool RunInParallel { get; }

        /// <summary>
        /// Gets the work function that the Task has to do
        /// </summary>
        public Func<TaskInfo, bool> Work { get; }
    }
}
