using System;

namespace OR_M_Data_Entities.Data.Modification
{
    public interface ITableChangeResult
    {
        UpdateType Action { get; }

        Type Table { get; }
    }
}
