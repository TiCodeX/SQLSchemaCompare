using SQLCompare.Core.Entities.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLCompare.Core.Entities.Project
{
    public class Project
    {
        public DatabaseProviderOptions Source { get; set; }

        public DatabaseProviderOptions Target { get; set; }

        public ProjectOptions Options { get; set; }

        // private BaseDb _sourceDB;

        // private BaseDb _targetDB;

        // public int _Compareresult { get; set; }
    }
}