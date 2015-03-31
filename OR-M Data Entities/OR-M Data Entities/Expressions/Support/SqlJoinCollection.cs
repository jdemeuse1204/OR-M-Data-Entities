using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.StatementParts;

namespace OR_M_Data_Entities.Expressions.Support
{
    public class SqlJoinCollection : IEnumerable<SqlJoin>
    {
        #region Properties
        private List<SqlJoin> _collection { get; set; }

        private List<KeyValuePair<Type,Type>> _keys { get; set; }
        public IEnumerable<KeyValuePair<Type, Type>> Keys 
        {
            get { return _keys; }
        } 

        public IEnumerable<Type> SelectedTypes 
        {
            get { return _distinctTypes; }
        }
        private List<Type> _distinctTypes { get; set; } 
        #endregion

        #region Constructor

        public SqlJoinCollection()
        {
            _collection = new List<SqlJoin>();
            _keys = new List<KeyValuePair<Type, Type>>();
            _distinctTypes = new List<Type>();
        }
        #endregion

        #region Methods

        public IEnumerable<SqlJoin> All()
        {
            return _collection;
        }

        public bool ContainsKey(KeyValuePair<Type, Type> key)
        {
            return _keys.Contains(key) || _keys.Contains(new KeyValuePair<Type, Type>(key.Value, key.Key));
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

                // inner joins should always overwrite left joins
                if (_collection[index].Type == JoinType.Left)
                {
                    _collection[index].Type = join.Type;
                }
            }

            if (!_distinctTypes.Contains(join.ParentEntity.Table))
            {
                _distinctTypes.Add(join.ParentEntity.Table);
            }

            if (!_distinctTypes.Contains(join.JoinEntity.Table))
            {
                _distinctTypes.Add(join.JoinEntity.Table);
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
