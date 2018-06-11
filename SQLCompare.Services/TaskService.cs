﻿using SQLCompare.Core.Entities;
using SQLCompare.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        public ReadOnlyCollection<TaskInfo> CurrentTaskInfos { get; private set; }

        /// <inheritdoc />
        public void ExecuteTasks(List<TaskWork> tasks)
        {
            this.CurrentTaskInfos = tasks.Select(x => x.Info).ToList().AsReadOnly();

            Task.Factory.StartNew(
                () =>
                {
                    var parallelTasks = new List<Task>();
                    foreach (var task in tasks)
                    {
                        if (!task.RunInParallel)
                        {
                            Task.WaitAll(parallelTasks.ToArray());
                        }
                        parallelTasks.Add(PerformTask(task));
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
                    task.Info.Status = TaskStatus.Running;
                    task.Work.Invoke(task.Info);
                    task.Info.Status = TaskStatus.RanToCompletion;
                    task.Info.CompleteTime = DateTime.Now;
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default);
        }
    }
}