using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Expressions.Query.Tables;

namespace OR_M_Data_Entities.Expressions.Collections
{
    public class ReadOnlyTableCollection : IEnumerable<ForeignKeyTable>
    {
        #region Fields
        protected readonly List<ForeignKeyTable> Internal;

        protected readonly Type ExpressionQueryType;
        #endregion

        #region Properties
        public int Count { get { return Internal.Count; } }

        #endregion

        #region Constructor
        public ReadOnlyTableCollection()
        {
            Internal = new List<ForeignKeyTable>();
        }
        #endregion

        public ForeignKeyTable Find(Type type, Guid expressionQueryId)
        {
            return Internal.FirstOrDefault(w => w.ExpressionQueryId == expressionQueryId && w.Type == type);
        }

        public ForeignKeyTable Find(string alias, Guid expressionQueryId)
        {
            return Internal.FirstOrDefault(w => w.ExpressionQueryId == expressionQueryId && w.Alias == alias);
        }

        public ForeignKeyTable FindByPropertyName(string propertyName, Guid expressionQueryId)
        {
            return Internal.First(w => w.ExpressionQueryId == expressionQueryId && w.ForeignKeyTableName == propertyName);
        }

        public string FindAlias(Type type, Guid expressionQueryId)
        {
            return Internal.First(w => w.ExpressionQueryId == expressionQueryId && w.Type == type).Alias;
        }

        public bool ContainsType(Type type, Guid expressionQueryId)
        {
            return Internal.Where(w => w.ExpressionQueryId == expressionQueryId).Select(w => w.Type).Contains(type);
        }

        public IEnumerator<ForeignKeyTable> GetEnumerator()
        {
            return Internal.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
