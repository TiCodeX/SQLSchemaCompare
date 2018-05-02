using System;
using System.Collections.Generic;

namespace SQLCompare.Core.Entities.Database
{
    public abstract class BaseDb
    {
        public string Name { get; set; }

        public DateTime LastModified { get; set; }

        public List<BaseTable> Tables { get; private set; }
    }
}