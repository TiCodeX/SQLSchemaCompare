using System;
using System.Collections.Generic;
using System.Text;

namespace SQLCompare.Core.Enums
{
    /// <summary>
    /// List of possible project status
    /// </summary>
    public enum ProjectState
    {
        /// <summary>
        /// The project is new
        /// </summary>
        New = 0,

        /// <summary>
        /// The project is dirty
        /// </summary>
        Dirty = 1,

        /// <summary>
        /// The project has been saved
        /// </summary>
        Saved = 2,
    }
}
