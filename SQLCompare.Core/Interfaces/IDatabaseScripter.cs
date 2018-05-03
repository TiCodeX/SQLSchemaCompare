using SQLCompare.Core.Entities.Database;

namespace SQLCompare.Core.Interfaces
{
    public interface IDatabaseScripter
    {
        BaseTable ScriptCreateTable();
    }
}