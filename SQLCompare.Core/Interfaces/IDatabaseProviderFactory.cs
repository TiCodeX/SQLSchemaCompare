using SQLCompare.Core.Entities;

namespace SQLCompare.Core.Interfaces
{
    public interface IDatabaseProviderFactory
    {
        IDatabaseProvider Create(DatabaseProviderOptions dbpo);
    }

}