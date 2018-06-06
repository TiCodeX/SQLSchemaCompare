using SQLCompare.Core.Entities;
using System;

namespace SQLCompare.Core.Interfaces.Services
{
    /// <summary>
    /// Defines a class that provides the mechanisms to handle asynchronous Tasks.
    /// </summary>
    public interface ITaskService
    {
        /// <summary>
        /// Gets the information about the current Task
        /// </summary>
        /// <returns>The updated Task information</returns>
        TaskInfo CurrentTaskInfo { get; }

        /// <summary>
        /// Create an asynchronous Task
        /// </summary>
        /// <param name="name">The name of the Task</param>
        /// <param name="work">The function to be executed by the Task</param>
        /// <returns>The initial Task information</returns>
        TaskInfo CreateNewTask(string name, Func<TaskInfo, bool> work);
    }
}
