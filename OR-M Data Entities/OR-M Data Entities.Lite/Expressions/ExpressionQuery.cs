﻿using OR_M_Data_Entities.Lite.Context;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using OR_M_Data_Entities.Lite.Mapping.Schema;
using OR_M_Data_Entities.Lite.Expressions.Query;
using OR_M_Data_Entities.Lite.Data;

namespace OR_M_Data_Entities.Lite.Expressions
{
    internal class ExpressionQuery<T> : IExpressionQuery<T> where T : class, new()
    {
        private readonly ExecutionContext executionContext;
        private readonly IReadOnlyDictionary<Type, TableSchema> objectSchemas;

        public ExpressionQuery(ExecutionContext executionContext, Dictionary<Type, TableSchema> objectSchemas)
        {
            this.executionContext = executionContext;
            this.objectSchemas = objectSchemas;
        }

        public T First()
        {
            throw new NotImplementedException();
        }

        public T First(Expression<Func<T, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public T FirstOrDefault()
        {
            // create the query
            var sqlQuery = SqlCreator.Create<T>(objectSchemas);

            return executionContext.LoadOneDefault<T>(sqlQuery, objectSchemas);
        }

        public T FirstOrDefault(Expression<Func<T, bool>> expression)
        {
            // create the query
            var sqlQuery = SqlCreator.Create(objectSchemas, expression);

            return executionContext.LoadOneDefault<T>(sqlQuery, objectSchemas);
        }

        public IExpressionQuery<T> Include(string pathOrTableName)
        {
            throw new NotImplementedException();
        }

        public IExpressionQuery<T> IncludeAll()
        {
            throw new NotImplementedException();
        }

        public IExpressionQuery<T> Select<TResult>(Expression<Func<T, TResult>> selector)
        {
            throw new NotImplementedException();
        }

        public List<T> ToList()
        {
            throw new NotImplementedException();
        }

        public List<T> ToList(Expression<Func<T, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public IExpressionQuery<T> Where(Expression<Func<T, bool>> expression)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}