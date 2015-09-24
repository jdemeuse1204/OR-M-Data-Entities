/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */
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
            var table = Internal.FirstOrDefault(w => w.ExpressionQueryId == expressionQueryId && w.Type == type) ??
                        Internal.FirstOrDefault(w => w.TypeChanges.Contains(type));

            return table;
        }

        public ForeignKeyTable Find(Type type, string foreignKeyTableName, string parentTableAlias, Guid expressionQueryId)
        {
            var table =
                Internal.FirstOrDefault(
                    w =>
                        w.ExpressionQueryId == expressionQueryId && w.ForeignKeyPropertyName == foreignKeyTableName &&
                        w.Type == type && w.ParentTableAlias == parentTableAlias) ??
                Internal.FirstOrDefault(w => w.TypeChanges.Contains(type));

            return table;
        }

        public ForeignKeyTable Find(string alias, Guid expressionQueryId)
        {
            return Internal.FirstOrDefault(w => w.ExpressionQueryId == expressionQueryId && w.Alias == alias);
        }

        public ForeignKeyTable FindByPropertyName(string propertyName, Guid expressionQueryId)
        {
            return Internal.First(w => w.ExpressionQueryId == expressionQueryId && w.ForeignKeyPropertyName == propertyName);
        }

        /// <summary>
        /// Only to be used in the resolve method
        /// </summary>
        /// <param name="type"></param>
        /// <param name="expressionQueryId"></param>
        /// <returns></returns>
        public string FindAlias(Type type, Guid expressionQueryId)
        {
            return Find(type, expressionQueryId).Alias;
        }

        /// <summary>
        /// Need to match on parent table alias because parent property name can be the same if nested inside other properties
        /// </summary>
        /// <param name="type"></param>
        /// <param name="expressionQueryId"></param>
        /// <param name="parentPropertyName"></param>
        /// <param name="parentTableAlias"></param>
        /// <returns></returns>
        public string FindAlias(Type type, Guid expressionQueryId, string parentPropertyName, string parentTableAlias)
        {
            return string.IsNullOrWhiteSpace(parentPropertyName)
                ? Find(type, expressionQueryId).Alias
                : Find(type, parentPropertyName, parentTableAlias, expressionQueryId).Alias;
        }

        public Type GetTableType(Type type, Guid expressionQueryId)
        {
            var table = Find(type, expressionQueryId);

            return table.Type;
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
