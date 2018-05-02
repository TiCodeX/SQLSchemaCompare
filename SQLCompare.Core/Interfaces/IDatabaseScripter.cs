using SQLCompare.Core.Entities.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLCompare.Core.Interfaces
{
    public interface IDatabaseScripter
    {
        BaseTable ScriptCreateTable();
    }
}