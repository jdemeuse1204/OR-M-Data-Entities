/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.StatementParts;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Support;
using OR_M_Data_Entities.Expressions.Types;
using OR_M_Data_Entities.Expressions.Types.Base;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Expressions
{
    public sealed class ExpressionQuery : ExpressionResolver, IEnumerable
    {
        #region Properties
        public string FromTableName { get; private set; }
        public List<object> WheresList { get; private set; }
        public List<object> InnerJoinsList { get; private set; }
        public List<object> LeftJoinsList { get; private set; }
        public List<object> SelectsList { get; private set; }
        public int TakeTopRows { get; private set; }
        public Type ReturnDataType { get; private set; }
        public bool IsDistinct { get; private set; }

        public string Sql { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        #endregion

        #region Fields
        private readonly DataFetching _context;
        #endregion

        #region Constructor
        public ExpressionQuery(string fromTable, DataFetching context)
        {
            FromTableName = fromTable;
            WheresList = new List<object>();
            InnerJoinsList = new List<object>();
            LeftJoinsList = new List<object>();
            SelectsList = new List<object>();
            Sql = string.Empty;
            Parameters = new Dictionary<string, object>();
            TakeTopRows = -1;  // -1 is select *
            _context = context;
            IsDistinct = false;
        }
        #endregion

        #region Add To Expression List
        private void AddWhereExpression<T>(Expression<Func<T, bool>> whereExpression)
        {
            WheresList.Add(whereExpression);
        }

        private void AddInnerJoinExpression<TParent, TChild>(Expression<Func<TParent, TChild, bool>> joinExpression)
        {
            InnerJoinsList.Add(joinExpression);
        }

        private void AddLeftJoinExpression<TParent, TChild>(Expression<Func<TParent, TChild, bool>> joinExpression)
        {
            LeftJoinsList.Add(joinExpression);
        }

        private void AddSelectExpression<T>(Expression<Func<T, object>> selectExpression)
        {
            SelectsList.Add(selectExpression);
        }
        #endregion

        #region Methods
        public ExpressionQuery Where<T>(Expression<Func<T, bool>> whereExpression)
        {
            AddWhereExpression(whereExpression);
            return this;
        }

        public ExpressionQuery Join<TParent, TChild>(Expression<Func<TParent, TChild, bool>> joinExpression)
            where TParent : class
            where TChild : class
        {
            AddInnerJoinExpression(joinExpression);
            return this;
        }

        public ExpressionQuery Join<TParent, TChild>()
            where TParent : class
            where TChild : class
        {
            var joins = _getJoins<TParent, TChild>(JoinType.Inner);

            InnerJoinsList.AddRange(joins);
            return this;
        }

        public ExpressionQuery LeftJoin<TParent, TChild>(Expression<Func<TParent, TChild, bool>> joinExpression)
            where TParent : class
            where TChild : class
        {
            AddLeftJoinExpression(joinExpression);
            return this;
        }

        public ExpressionQuery LeftJoin<TParent, TChild>()
            where TParent : class
            where TChild : class
        {
            var join = _getJoins<TParent, TChild>(JoinType.Left);

            LeftJoinsList.Add(join);
            return this;
        }

        public ExpressionQuery Select<T>(Expression<Func<T, object>> result)
        {
            AddSelectExpression(result);

            if (ReturnDataType != null)
            {
                throw new Exception("Cannot return multiple types");
            }

            ReturnDataType = typeof(T);

            return this;
        }

        public ExpressionQuery Select<T>()
        {
            return Select<T>(w => w);
        }

        public ExpressionQuery Distinct()
        {
            IsDistinct = true;
            return this;
        }

        public ExpressionQuery Take(int rows)
        {
            TakeTopRows = rows;
            return this;
        }

        private List<SqlJoin> _getJoins<TParent, TChild>(JoinType joinType)
            where TChild : class
            where TParent : class
        {
            var joins = new List<SqlJoin>();
            var foreignKeys = DatabaseSchemata.GetForeignKeys<TParent>();

            if (foreignKeys.Count == 0) throw new ArgumentException("Child Table does not contain any Foreign Key(s)");

            foreach (var foreignKey in foreignKeys)
            {
                var foreignKeyAttribute = foreignKey.GetCustomAttribute<ForeignKeyAttribute>();
                var join = new SqlJoin();

                join.ParentEntity = new SqlTableColumnPair
                {
                    Table = typeof(TParent),
                    Column = DatabaseSchemata.GetTableFieldByName(foreignKeyAttribute.PrimaryKeyColumnName, typeof(TParent))
                };

                join.JoinEntity = new SqlTableColumnPair
                {
                    Table = typeof(TChild),
                    Column = DatabaseSchemata.GetTableFieldByName(foreignKeyAttribute.ForeignKeyColumnName, typeof(TChild))
                };

                join.Type = joinType;

                joins.Add(join);
            }

            return joins;
        }

        #endregion

        private SqlExpressionResolvableBase _getExpressionType()
        {
            // Check for foreign keys
            if (DatabaseSchemata.HasForeignKeys(ReturnDataType))
            {
                if (_hasJoins())
                {
                    if (WheresList.Count > 0)
                    {
                        return _createExpressionType<ForeignKeySelectWhereJoinExpression>();
                    }

                    return _createExpressionType<ForeignKeySelectJoinExpression>();
                }

                if (WheresList.Count > 0)
                {
                    return _createExpressionType<ForeignKeySelectWhereExpression>();
                }

                return _createExpressionType<ForeignKeySelectExpression>();
            }

            // no foreign keys exist
            if (_hasJoins())
            {
                if (WheresList.Count > 0)
                {
                    return _createExpressionType<SelectWhereJoinExpression>();
                }

                return _createExpressionType<SelectJoinExpression>();
            }

            if (WheresList.Count > 0)
            {
                return _createExpressionType<SelectWhereExpression>();
            }

            return _createExpressionType<SelectExpression>();
        }

        private bool _hasJoins()
        {
            return InnerJoinsList.Count != 0 || LeftJoinsList.Count != 0;
        }

        private T _createExpressionType<T>() where T : SqlExpressionResolvableBase
        {
            return Activator.CreateInstance(typeof(T), new object[] { this }) as T;
        }

        private SqlExpressionType _resolveExpression()
        {
            if (SelectsList.Count == 0) throw new ArgumentException("No columns selected, please use .Select<T>() to select columns.");

            // get the expression type            
            var expression = _getExpressionType();

            // resolve the expression
            return expression.Resolve();
        }

        #region Data Retrieval

        public object First()
        {
            // inject the generic type here
            var method = typeof(ExpressionQuery).GetMethods().FirstOrDefault(w => w.Name == "First" && w.ReturnParameter != null && w.ReturnParameter.ParameterType.Name == "T");
            var genericMethod = method.MakeGenericMethod(new[] { ReturnDataType });

            return genericMethod.Invoke(this, null);
        }

        public T First<T>()
        {
            _resolveExpression();

            using (var reader = _context.ExecuteQuery<T>(Sql, Parameters))
            {
                return reader.Select();
            }
        }

        public ICollection All()
        {
            // inject the generic type here
            var method = typeof(ExpressionQuery).GetMethods().FirstOrDefault(w => w.Name == "All" && w.ReturnType != typeof(ICollection));
            var genericMethod = method.MakeGenericMethod(new[] { ReturnDataType });
            var result = genericMethod.Invoke(this, null);

            return result as dynamic;
        }

        public List<T> All<T>()
        {
             _resolveExpression();

            using (var reader = _context.ExecuteQuery<T>(Sql, Parameters))
            {
                return reader.Cast<T>().ToList();
            }
        }

        public IEnumerator GetEnumerator<T>()
        {
            _resolveExpression();

            var reader = _context.ExecuteQuery<T>(Sql, Parameters);

            return reader.Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            // inject the generic type here
            var method = typeof(ExpressionQuery).GetMethod("GetEnumerator");
            var genericMethod = method.MakeGenericMethod(new[] { ReturnDataType });

            return (IEnumerator)genericMethod.Invoke(this, null);
        }
        #endregion
    }
}
