using System;
using System.Collections.Generic;
using System.Linq;

namespace OR_M_Data_Entities.Expressions.Query
{
    public class TableTypeCollection : ReadOnlyTableTypeCollection
    {
        public void Add(Type type)
        {
            Internal.Add(new TableType(type, string.Format("AkA{0}", Internal.Count)));
        }

        public void AddRange(IEnumerable<Type> range)
        {
            Internal.AddRange(range.Select(w => new TableType(w, string.Format("AkA{0}", Internal.Count))));
        }

        public TableType this[int i]
        {
            get { return Internal[i]; }
        }
    }
}
