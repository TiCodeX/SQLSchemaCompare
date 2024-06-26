﻿namespace TiCodeX.SQLSchemaCompare.Services
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Implementation class that provides the mechanisms to handle asynchronous Tasks.
    /// </summary>
    public sealed class TaskService : ITaskService, IDisposable
    {
        /// <summary>
        /// The cancellation token source
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <inheritdoc />
        public ReadOnlyCollection<TaskInfo> CurrentTaskInfos { get; private set; }

        /// <inheritdoc />
        [SuppressMessage("Blocker Code Smell", "S4462:Calls to 'async' methods should not be blocking.", Justification = "Necessary")]
        [SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "TODO")]
        public void ExecuteTasks(List<TaskWork> tasks)
        {
            this.cancellationTokenSource = new CancellationTokenSource();

            this.CurrentTaskInfos = tasks.Select(x =>
            {
                x.Info.CancellationToken = this.cancellationTokenSource.Token;
                return x.Info;
            }).ToList().AsReadOnly();

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
                            // Task.WaitAll is not used here because it throws an exception if a Task has been canceled,
                            // instead with WhenAll/ContinueWith it creates an empty task which does nothing but allows
                            // to continue the execution
                            Task.WhenAll(runningTasks.Select(x => x.Item2).ToArray()).ContinueWith(t => { }, TaskScheduler.Default).Wait();
                            terminatedTasks.AddRange(runningTasks.Select(x =>
                            {
                                // If a Task is completed but the TaskInfo status is not a final state, it means that
                                // the Task is not even started, hence the status should be Canceled
                                if (x.Item1.Info.Status != TaskStatus.RanToCompletion &&
                                    x.Item1.Info.Status != TaskStatus.Faulted)
                                {
                                    x.Item1.Info.Status = TaskStatus.Canceled;
                                    x.Item1.Info.Exception = new OperationCanceledException(Localization.ErrorOperationNotExecuted);
                                }

                                return x.Item1;
                            }));
                            runningTasks.Clear();

                            if (terminatedTasks.Any(x => x.Info.Status == TaskStatus.Faulted ||
                                                         x.Info.Status == TaskStatus.Canceled))
                            {
                                // Put the current task back to the remaining list and set all as not executed
                                remainingTasks.Enqueue(task);
                                remainingTasks.ToList().ForEach(x =>
                                {
                                    x.Info.Status = TaskStatus.Canceled;
                                    x.Info.Exception = new OperationCanceledException(Localization.ErrorOperationNotExecuted);
                                });
                                break;
                            }
                        }

                        runningTasks.Add(new Tuple<TaskWork, Task>(task, this.PerformTaskAsync(task)));
                    }
                },
                this.cancellationTokenSource.Token,
                TaskCreationOptions.None,
                TaskScheduler.Default);
        }

        /// <inheritdoc />
        public void Abort()
        {
            this.cancellationTokenSource?.Cancel();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.cancellationTokenSource?.Dispose();
        }

        /// <summary>
        /// Perform the task
        /// </summary>
        /// <param name="taskWork">The task work</param>
        /// <returns>The task</returns>
        private Task PerformTaskAsync(TaskWork taskWork)
        {
            return Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        taskWork.Info.Status = TaskStatus.Running;
                        taskWork.Work.Invoke(taskWork.Info);
                        taskWork.Info.Status = TaskStatus.RanToCompletion;
                        taskWork.Info.Percentage = 100;
                    }
                    catch (Exception ex)
                    {
                        taskWork.Info.Status = TaskStatus.Faulted;
                        taskWork.Info.Exception = ex is OperationCanceledException ?
                            new OperationCanceledException(Localization.ErrorOperationCanceled) :
                            ex;
                    }
                    finally
                    {
                        taskWork.Info.CompleteTime = DateTime.Now;
                    }
                },
                this.cancellationTokenSource.Token,
                TaskCreationOptions.None,
                TaskScheduler.Default);
        }
    }
}
