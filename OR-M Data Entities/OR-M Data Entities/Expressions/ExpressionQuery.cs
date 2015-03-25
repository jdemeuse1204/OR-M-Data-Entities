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
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Expressions
{
    public sealed class ExpressionQuery : ExpressionResolver, IEnumerable
    {
        #region Properties
        private string _from { get; set; }
        private List<object> _where { get; set; }
        private List<object> _innerJoins { get; set; }
        private List<object> _leftJoins { get; set; }
        private List<object> _select { get; set; }
        private string _sql { get; set; }
        private Dictionary<string, object> _parameters { get; set; }
        private int _take { get; set; }
        private readonly DataFetching _context;
        private Type _returnDataType { get; set; }
        private bool _distinct { get; set; }
        #endregion

        #region Constructor
        public ExpressionQuery(string fromTable, DataFetching context)
        {
            _from = fromTable;
            _where = new List<object>();
            _innerJoins = new List<object>();
            _leftJoins = new List<object>();
            _select = new List<object>();
            _sql = string.Empty;
            _parameters = new Dictionary<string, object>();
            _take = -1;  // -1 is select *
            _context = context;
            _distinct = false;
        }
        #endregion

        #region Add To Expression List
        private void AddWhereExpression<T>(Expression<Func<T, bool>> whereExpression)
        {
            _where.Add(whereExpression);
        }

        private void AddInnerJoinExpression<TParent, TChild>(Expression<Func<TParent, TChild, bool>> joinExpression)
        {
            _innerJoins.Add(joinExpression);
        }

        private void AddLeftJoinExpression<TParent, TChild>(Expression<Func<TParent, TChild, bool>> joinExpression)
        {
            _leftJoins.Add(joinExpression);
        }

        private void AddSelectExpression<T>(Expression<Func<T, object>> selectExpression)
        {
            _select.Add(selectExpression);
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

            _innerJoins.AddRange(joins);
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

            _leftJoins.Add(join);
            return this;
        }

        public ExpressionQuery Select<T>(Expression<Func<T, object>> result)
        {
            AddSelectExpression(result);

            if (_returnDataType == null)
            {
                _returnDataType = typeof(T);
            }

            return this;
        }

        public ExpressionQuery Select<T>()
        {
            return Select<T>(w => w);
        }

        public ExpressionQuery Distinct()
        {
            _distinct = true;
            return this;
        }

        public ExpressionQuery Take(int rows)
        {
            _take = rows;
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

        private void ResolveExpression()
        {
            if (_select.Count == 0) throw new ArgumentException("No columns selected, please use .Select<T>() to select columns.");

            // check for auto load attributes

            _sql = string.Empty;

            // Turn the Where Lambda Statements into Sql
            var where = new List<SqlWhere>();

            foreach (var resolution in _where.Select(item => ResolveWhere(item as dynamic)))
            {
                @where.AddRange(resolution);
            }

            // Turn the Inner Join Lambda Statements into Sql
            var innerJoin = new List<SqlJoin>();

            foreach (var resolution in _innerJoins.Select(item => item is SqlJoin ? item : ResolveJoin(item as dynamic, JoinType.Inner)))
            {
                if (resolution is SqlJoin)
                {
                    @innerJoin.Add(resolution);
                }
                else
                {
                    @innerJoin.AddRange(resolution);
                }
            }

            // Turn the Left Join Lambda Statements into Sql
            var leftJoin = new List<SqlJoin>();

            foreach (var resolution in _leftJoins.Select(item => item is SqlJoin ? item : ResolveJoin(item as dynamic, JoinType.Left)))
            {
                if (resolution is SqlJoin)
                {
                    @leftJoin.Add(resolution);
                }
                else
                {
                    @leftJoin.AddRange(resolution);
                }
            }

            // Turn the Select Lambda Statements into Sql
            var select = new List<SqlTableColumnPair>();

            foreach (var resolution in _select.Select(item => ResolveSelect(item as dynamic)))
            {
                @select.AddRange(resolution);
            }

            var selectTopModifier = _take == -1 ? string.Empty : string.Format(" TOP {0} ", _take);
            var selectDistinctModifier = _distinct ? "DISTINCT" : string.Empty;

            // add the select modifier
            _sql += string.Format(" SELECT {0}{1} ", selectDistinctModifier, selectTopModifier);

            var selectText = @select.Aggregate(string.Empty, (current, item) => current + string.Format("{0},", item.GetSelectColumnTextWithAlias()));

            // trim the ending comma
            _sql += selectText.TrimEnd(',');

            _sql += string.Format(" FROM [{0}] ", _from);

            var innerJoinText = @innerJoin.Aggregate(string.Empty, (current, item) => current + item.GetJoinText());

            if (!string.IsNullOrWhiteSpace(innerJoinText))
            {
                _sql += innerJoinText;
            }

            var leftJoinText = @leftJoin.Aggregate(string.Empty, (current, item) => current + item.GetJoinText());

            if (!string.IsNullOrWhiteSpace(leftJoinText))
            {
                _sql += leftJoinText;
            }

            var validationStatements = @where.Select(item => item.GetWhereText(_parameters)).ToList();

            var finalValidationStatement = validationStatements.Aggregate(string.Empty, (current, validationStatement) => current + string.Format(string.IsNullOrWhiteSpace(current) ? " WHERE {0}" : " AND {0}", validationStatement));

            _sql += finalValidationStatement;
        }

        #region Data Retrieval

        public object First()
        {
            // inject the generic type here
            var method = typeof(ExpressionQuery).GetMethods().FirstOrDefault(w => w.Name == "First" && w.ReturnParameter != null && w.ReturnParameter.ParameterType.Name == "T");
            var genericMethod = method.MakeGenericMethod(new[] { _returnDataType });

            return genericMethod.Invoke(this, null);
        }

        public T First<T>()
        {
            ResolveExpression();

            using (var reader = _context.ExecuteQuery<T>(_sql, _parameters))
            {
                return reader.Select();
            }
        }

        public ICollection All()
        {
            // inject the generic type here
            var method = typeof(ExpressionQuery).GetMethods().FirstOrDefault(w => w.Name == "All" && w.ReturnType != typeof(ICollection));
            var genericMethod = method.MakeGenericMethod(new[] { _returnDataType });
            var result = genericMethod.Invoke(this, null);

            return result as dynamic;
        }

        public List<T> All<T>()
        {
            ResolveExpression();

            using (var reader = _context.ExecuteQuery<T>(_sql, _parameters))
            {
                return reader.Cast<T>().ToList();
            }
        }

        public IEnumerator GetEnumerator<T>()
        {
            ResolveExpression();

            var reader = _context.ExecuteQuery<T>(_sql, _parameters);

            return reader.Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            // inject the generic type here
            var method = typeof(ExpressionQuery).GetMethod("GetEnumerator");
            var genericMethod = method.MakeGenericMethod(new[] { _returnDataType });

            return (IEnumerator)genericMethod.Invoke(this, null);
        }
        #endregion
    }
}
