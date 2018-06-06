using SQLCompare.Core.Entities;
using SQLCompare.Core.Interfaces.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SQLCompare.Services
{
    /// <summary>
    /// Implementation class that provides the mechanisms to handle asynchronous Tasks.
    /// </summary>
    public class TaskService : ITaskService
    {
        /// <inheritdoc />
        public TaskInfo CurrentTaskInfo { get; private set; }

        /// <inheritdoc />
        public TaskInfo CreateNewTask(string name, Func<TaskInfo, bool> work)
        {
            this.CurrentTaskInfo = new TaskInfo
            {
                Name = name
            };

            var task = Task.Factory.StartNew(
                () =>
                {
                    this.CurrentTaskInfo.Status = TaskStatus.Running;
                    work.Invoke(this.CurrentTaskInfo);
                    this.CurrentTaskInfo.Status = TaskStatus.RanToCompletion;
                    this.CurrentTaskInfo.CompleteTime = DateTime.Now;
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default);

            return this.CurrentTaskInfo;
        }
    }
}
