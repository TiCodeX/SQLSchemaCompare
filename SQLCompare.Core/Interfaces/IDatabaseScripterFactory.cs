using SQLCompare.Core.Entities.Database;

namespace SQLCompare.Core.Interfaces
{
    public interface IDatabaseScripterFactory
    {
        void Create(BaseDb database);
    }
}