using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Interfaces.Services;

namespace SQLCompare.Services
{
    /// <summary>
    /// Implementation class that provides the mechanisms to handle asynchronous Tasks.
    /// </summary>
    public class TaskService : ITaskService
    {
        /// <inheritdoc />
        public ReadOnlyCollection<TaskInfo> CurrentTaskInfos { get; private set; }

        /// <inheritdoc />
        public void ExecuteTasks(List<TaskWork> tasks)
        {
            this.CurrentTaskInfos = tasks.Select(x => x.Info).ToList().AsReadOnly();

            Task.Factory.StartNew(
                () =>
                {
                    var remainingTasks = new Queue<TaskWork>(tasks);
                    var runningTasks = new List<Tuple<TaskWork, Task>>();
                    var terminatedTasks = new List<TaskWork>();

                    while (remainingTasks.Count > 0)
                    {
                        var task = remainingTasks.Dequeue();
                        if (!task.RunInParallel)
                        {
                            Task.WaitAll(runningTasks.Select(x => x.Item2).ToArray());
                            terminatedTasks.AddRange(runningTasks.Select(x => x.Item1));
                            runningTasks.Clear();

                            if (terminatedTasks.Any(x => x.Info.Status == TaskStatus.Faulted))
                            {
                                // Set the current and the remaining tasks as not executed
                                task.Info.Status = TaskStatus.Canceled;
                                remainingTasks.ToList().ForEach(x => { x.Info.Status = TaskStatus.Canceled; });
                                break;
                            }
                        }
                        runningTasks.Add(new Tuple<TaskWork, Task>(task, PerformTask(task)));
                    }
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default);
        }

        private static Task PerformTask(TaskWork task)
        {
            return Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        task.Info.Status = TaskStatus.Running;
                        task.Work.Invoke(task.Info);
                        task.Info.Status = TaskStatus.RanToCompletion;
                    }
                    catch (Exception ex)
                    {
                        task.Info.Status = TaskStatus.Faulted;
                        task.Info.Exception = ex;
                    }
                    finally
                    {
                        task.Info.Percentage = 100;
                        task.Info.CompleteTime = DateTime.Now;
                    }
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default);
        }
    }
}
