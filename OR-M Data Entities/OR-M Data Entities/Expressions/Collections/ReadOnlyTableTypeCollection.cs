using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Expressions.Query;

namespace OR_M_Data_Entities.Expressions.Collections
{
    public class ReadOnlyTableTypeCollection : IEnumerable<TableType>
    {
        #region Fields
        protected readonly List<TableType> Internal;

        protected readonly Type ExpressionQueryType;
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

        public TableType Find(Type type, Guid expressionQueryId)
        {
            return Internal.FirstOrDefault(w => w.ExpressionQueryId == expressionQueryId && w.Type == type);
        }

        public TableType Find(string alias, Guid expressionQueryId)
        {
            return Internal.FirstOrDefault(w => w.ExpressionQueryId == expressionQueryId && w.Alias == alias);
        }

        public TableType FindByPropertyName(string propertyName, Guid expressionQueryId)
        {
            return Internal.First(w => w.ExpressionQueryId == expressionQueryId && w.PropertyName == propertyName);
        }

        public string FindAlias(Type type, Guid expressionQueryId)
        {
            return Internal.First(w => w.ExpressionQueryId == expressionQueryId && w.Type == type).Alias;
        }

        public bool ContainsType(Type type, Guid expressionQueryId)
        {
            return Internal.Where(w => w.ExpressionQueryId == expressionQueryId).Select(w => w.Type).Contains(type);
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
