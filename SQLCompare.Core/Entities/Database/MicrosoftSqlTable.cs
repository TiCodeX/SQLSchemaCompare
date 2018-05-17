﻿using System;

namespace SQLCompare.Core.Entities.Database
{
    /// <summary>
    /// Specific MicrosoftSql table definition
    /// </summary>
    public class MicrosoftSqlTable : BaseDbTable
    {
#pragma warning disable SA1600 // Elements must be documented
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public DateTime? CreateDate { get; set; }

        public long ObjectId { get; set; }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements must be documented
    }
}