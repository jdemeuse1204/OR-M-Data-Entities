using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Lite.Data
{
    internal interface IPeekDataReader : IDataReader
    {
        bool WasPeeked { get; }

        bool HasRows { get; }

        T ToObjectDefault<T>();

        T ToObject<T>();

        bool Peek();
    }
}
