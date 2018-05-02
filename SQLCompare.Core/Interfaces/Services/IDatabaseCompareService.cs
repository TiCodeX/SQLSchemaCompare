using SQLCompare.Core.Entities.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLCompare.Core.Interfaces.Services
{
    public interface IDatabaseCompareService
    {
        void Compare(BaseDb Source, BaseDb Target);
    }
}