using System;
using System.Collections;
using System.Collections.Generic;

namespace OR_M_Data_Entities.Data
{
    public interface IDataTranslator<T> : IEnumerable, IDisposable
    {
        bool HasRows { get; }

        T FirstOrDefault();

        T First();

        List<T> ToList();
    }
}
