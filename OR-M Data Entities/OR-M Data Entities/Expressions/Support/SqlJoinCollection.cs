/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.Secure.StatementParts;

namespace OR_M_Data_Entities.Expressions.Support
{
    public sealed class SqlJoinCollection : IEnumerable<SqlJoin>
    {
        #region Properties
        private List<SqlJoin> _collection { get; set; }

        private List<KeyValuePair<Type,Type>> _keys { get; set; }
        public IEnumerable<KeyValuePair<Type, Type>> Keys 
        {
            get { return _keys; }
        }

        // JoinEntityTableName, Type
        public IEnumerable<KeyValuePair<string, Type>> SelectedTypes 
        {
            get { return _distinctTypes; }
        }
        private Dictionary<string , Type> _distinctTypes { get; set; } 
        #endregion

        #region Constructor

        public SqlJoinCollection()
        {
            _collection = new List<SqlJoin>();
            _keys = new List<KeyValuePair<Type, Type>>();
            _distinctTypes = new Dictionary<string , Type>();
        }
        #endregion

        #region Methods

        public IEnumerable<SqlJoin> All()
        {
            return _collection;
        }

        public SqlJoinCollectionKeyMatchType ContainsKey(KeyValuePair<Type, Type> key)
        {
            if (_keys.Contains(key))
            {
                return SqlJoinCollectionKeyMatchType.AsIs;
            }

            return _keys.Contains(new KeyValuePair<Type, Type>(key.Value, key.Key)) ? 
                SqlJoinCollectionKeyMatchType.Inverse : 
                SqlJoinCollectionKeyMatchType.NoMatch;
        }

        public void Add(SqlJoin join)
        {
            var key = new KeyValuePair<Type, Type>(
                join.ParentEntity.Table,
                join.JoinEntity.Table);

            if (_keys.Contains(key))
            {
                if (join.Type == JoinType.Left) return;

                var index = _keys.IndexOf(key);

                // inner joins should always overwrite left joins, except if the FK can be null
                if (_collection[index].Type == JoinType.Left)
                {
                    _collection[index].Type = join.Type;
                }
            }

            if (join.JoinEntityTableName != null && !_distinctTypes.ContainsKey(join.JoinEntityTableName))
            {
                _distinctTypes.Add(join.JoinEntityTableName, join.JoinEntity.Table);
            }

            _collection.Add(join);
            _keys.Add(key);
        }

        public string GetSql()
        {
            return _collection.Aggregate(string.Empty, (current, @join) => current + @join.GetJoinText());
        }

        public KeyValuePair<Type, Type> GetKey(int index)
        {
            return _keys[index];
        }

        public SqlJoin GetJoin(int index)
        {
            return _collection[index];
        }
        #endregion

        #region Enumeration
        public IEnumerator<SqlJoin> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
