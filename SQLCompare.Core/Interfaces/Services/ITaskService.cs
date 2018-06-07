﻿using SQLCompare.Core.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        ReadOnlyCollection<TaskInfo> CurrentTaskInfos { get; }

        /// <summary>
        /// Perform the execution of the Tasks
        /// </summary>
        /// <param name="tasks">The list of Tasks to be executed</param>
        void ExecuteTasks(List<TaskWork> tasks);
    }
}
