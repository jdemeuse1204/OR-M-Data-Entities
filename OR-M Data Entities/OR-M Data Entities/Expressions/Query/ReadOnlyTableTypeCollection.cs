using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OR_M_Data_Entities.Expressions.Query
{
    public class ReadOnlyTableTypeCollection : IEnumerable<TableType>
    {
        #region Fields
        protected readonly List<TableType> Internal;
        #endregion

        #region Properties
        public int Count { get { return Internal.Count; } }
        #endregion

        #region Constructor
        public ReadOnlyTableTypeCollection()
        {
            Internal = new List<TableType>();
        }
        #endregion

        public TableType Find(Type type)
        {
            return Internal.FirstOrDefault(w => w.Type == type);
        }

        public TableType Find(string alias)
        {
            return Internal.FirstOrDefault(w => w.Alias == alias);
        }

        public string FindAlias(Type type)
        {
            return Internal.First(w => w.Type == type).Alias;
        }

        public bool ContainsType(Type type)
        {
            return Internal.Select(w => w.Type).Contains(type);
        }

        public IEnumerator<TableType> GetEnumerator()
        {
            return Internal.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
