using System;
using System.Collections.Generic;

namespace SQLCompare.Core.Entities.Database
{
    public abstract class BaseTable
    {
        public string Name { get; set; }

        public DateTime LastModified { get; set; }

        public List<BaseColumn> Columns { get; private set; }
    }
}