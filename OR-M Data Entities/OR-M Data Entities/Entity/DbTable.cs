/*
 * OR-M Data Entities v1.0.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Entity
{
    public sealed class DbTable<T> : IDbTable<T> where T : class
    {
        public string TableName { get { return _tableName; } }
        private string _tableName { get; set; }
        public bool HasChanges
        {
            get { return _collection != null && _collection.Count > 0; }
        }
        private readonly DbSqlContext _context;
        private readonly Dictionary<T, SaveAction> _collection;

        // Expose for user, not editable
        public IEnumerable<KeyValuePair<T,SaveAction>> Local { get { return _collection; } } 

        public DbTable(DbSqlContext context)
        {
            _context = context;
            _collection = new Dictionary<T, SaveAction>();
            _tableName = DatabaseSchemata.GetTableName(Activator.CreateInstance<T>());
        }

        public void Add(T entity)
        {
            _collection.Add(entity, SaveAction.Save);
        }


        public void Remove(T entity)
        {
            _collection.Add(entity, SaveAction.Remove);
        }

        public bool RemoveLocal(T entity)
        {
            return _collection.Remove(entity);
        }

        public void Clear()
        {
            _collection.Clear();
        }

        public List<T> Where(Expression<Func<T, bool>> propertyLambda)
        {
            //using (var reader = _context.ExecuteQuery(propertyLambda, _context))
            //{
            //    return reader.All();
            //}

            // TODO FIX ME
            return null;
        }

        public T FirstOrDefault(Expression<Func<T, bool>> propertyLambda)
        {
            //using (var reader = _context.ExecuteQuery(propertyLambda))
            //{
            //    return reader.Select();;
            //}
            // TODO FIX ME
            return null;
        }

        public T Find(params object[] pks)
        {
            return _context.Find<T>(pks);
        }

        public List<T> All()
        {
            return _context.All<T>();
        }
    }
}
