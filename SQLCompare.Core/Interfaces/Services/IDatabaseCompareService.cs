using SQLCompare.Core.Entities.Database;

namespace SQLCompare.Core.Interfaces.Services
{
    public interface IDatabaseCompareService
    {
        void Compare(BaseDb source, BaseDb sarget);
    }
}