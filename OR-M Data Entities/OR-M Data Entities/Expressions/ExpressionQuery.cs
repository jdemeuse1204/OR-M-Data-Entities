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
        private dynamic _results { get; set; }
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
            var joins = _getJoins<TParent, TChild>();

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
            var join = _getJoins<TParent, TChild>();

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

            ResolveExpression();

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

        private List<SqlJoin> _getJoins<TParent, TChild>()
            where TChild : class
            where TParent : class
        {
            var joins = new List<SqlJoin>();
            var foreignKeys = DatabaseSchemata.GetForeignKeys<TChild>();

            if (foreignKeys.Count == 0) throw new ArgumentException("Child Table does not contain any Foreign Key(s)");

            foreach (var foreignKey in foreignKeys)
            {
                var foreignKeyAttribute = foreignKey.GetCustomAttribute<ForeignKeyAttribute>();
                var join = new SqlJoin();

                join.ParentEntity = new SqlTableColumnPair
                {
                    Table = typeof(TParent),
                    Column = DatabaseSchemata.GetPrimaryKeyByName(foreignKeyAttribute.PrimaryKeyColumnName, typeof(TParent))
                };

                join.JoinEntity = new SqlTableColumnPair
                {
                    Table = typeof(TChild),
                    Column = DatabaseSchemata.GetTableFieldByName(foreignKeyAttribute.ForeignKeyColumnName, typeof(TChild))
                };

                joins.Add(join);
            }

            return joins;
        }

        #region Helpers
        private string _enumerateList(ICollection list)
        {
            var result = "";

            foreach (var item in list)
            {
                var parameter = _parameters.GetNextParameter();
                _parameters.Add(parameter, item);

                result += parameter + ",";
            }

            return result.TrimEnd(',');
        }

        private string _getComparisonString(SqlWhere where)
        {
            if (where.ComparisonType == ComparisonType.Contains)
            {
                return where.ObjectCompareValue.IsList() ? " {0} IN ({1}) " : " {0} LIKE {1}";
            }

            switch (where.ComparisonType)
            {
                case ComparisonType.BeginsWith:
                case ComparisonType.EndsWith:
                    return " {0} LIKE {1}";
                case ComparisonType.Equals:
                    return "{0} = {1}";
                case ComparisonType.EqualsIgnoreCase:
                    return "";
                case ComparisonType.EqualsTruncateTime:
                    return "";
                case ComparisonType.GreaterThan:
                    return "{0} > {1}";
                case ComparisonType.GreaterThanEquals:
                    return "{0} >= {1}";
                case ComparisonType.LessThan:
                    return "{0} < {1}";
                case ComparisonType.LessThanEquals:
                    return "{0} <= {1}";
                case ComparisonType.NotEqual:
                    return "{0} != {1}";
                default:
                    throw new ArgumentOutOfRangeException("comparison");
            }
        }
        #endregion

        #endregion

        private object _resolveCompareValue(SqlWhere where)
        {
            if (where.ComparisonType == ComparisonType.Contains
                && where.ObjectCompareValue.IsList())
            {
                return where.ObjectCompareValue;
            }

            switch (where.ComparisonType)
            {
                case ComparisonType.Contains:
                    return "%" + Convert.ToString(where.ObjectCompareValue) + "%";
                case ComparisonType.BeginsWith:
                    return Convert.ToString(where.ObjectCompareValue) + "%";
                case ComparisonType.EndsWith:
                    return "%" + Convert.ToString(where.ObjectCompareValue);
                default:
                    return where.ObjectCompareValue;
            }
        }

        private void ResolveExpression()
        {
            if (_select.Count == 0) throw new ArgumentException("No columns selected, please use .Select<T>() to select columns.");

            // check for auto load attributes

            _sql = string.Empty;

            var where = new List<SqlWhere>();

            foreach (var resolution in _where.Select(item => ResolveWhere(item as dynamic)))
            {
                @where.AddRange(resolution);
            }

            var innerJoin = new List<SqlJoin>();

            foreach (var item in _innerJoins)
            {
                if (item is SqlJoin)
                {
                    @innerJoin.Add(item as SqlJoin);
                }
                else
                {
                    @innerJoin.AddRange(ResolveJoin(item as dynamic));
                }
            }

            var leftJoin = new List<SqlJoin>();

            foreach (var item in _leftJoins)
            {
                if (item is SqlJoin)
                {
                    @leftJoin.Add(item as SqlJoin);
                }
                else
                {
                    @leftJoin.AddRange(ResolveJoin(item as dynamic));
                }
            }

            var select = new List<SqlTableColumnPair>();

            foreach (var resolution in _select.Select(item => ResolveSelect(item as dynamic)))
            {
                @select.AddRange(resolution);
            }

            var hasLeftJoins = leftJoin.Count > 0;
            var hasInnerJoins = innerJoin.Count > 0;
            var hasJoins = hasInnerJoins || hasLeftJoins;
            var selectTopModifier = _take == -1 ? string.Empty : string.Format(" TOP {0} ", _take);
            var selectDistinctModifier = _distinct ? "DISTINCT" : string.Empty;
            var validationStatements = new List<string>();

            // add the select modifier
            _sql += string.Format(" SELECT {0}{1} ", selectDistinctModifier, selectTopModifier);

            foreach (var item in @select)
            {
                _sql += string.Format("{0},", item.GetSelectColumnTextWithAlias());
            }

            // trim the ending comma
            _sql = _sql.TrimEnd(',');

            _sql += string.Format("FROM [{0}] ", _from);

            foreach (var item in @innerJoin)
            {
                _sql += item.GetJoinText();
            }

            foreach (var item in @leftJoin)
            {
                _sql += item.GetJoinText();
            }

            foreach (var item in @where)
            {
                //var partOfValidation = _getComparisonString(item);
                //var leftSide = !item.ShouldCast
                //    ? string.Format(" [{0}].[{1}] ",
                //        item.TableName,
                //        item.ColumnName)
                //    : Cast(item);

                //var rightSide = "";

                //if (item.CompareValue is ExpressionSelectResult)
                //{
                //    var compareValue = item.CompareValue as ExpressionSelectResult;
                //    rightSide = !compareValue.ShouldCast
                //        ? string.Format("[{0}].[{1}]", compareValue.TableName, compareValue.ColumnName)
                //        : Cast(compareValue, false);
                //}
                //else if (ExpressionTypeTransform.IsList(item.CompareValue))
                //{
                //    rightSide = _enumerateList(item.CompareValue as ICollection);
                //}
                //else
                //{
                //    var parameter = _getNextParameter();
                //    var compareValue = _resolveCompareValue(item);
                //    rightSide = parameter;

                //    if (item.CompareValue is DataTransformContainer)
                //    {
                //        rightSide = Cast(parameter, ((DataTransformContainer)item.CompareValue).Transform);
                //        compareValue = ((DataTransformContainer)item.CompareValue).Value;
                //    }

                //    _parameters.Add(parameter, compareValue);
                //}

                validationStatements.Add(item.GetWhereText(_parameters));
            }

            for (var i = 0; i < validationStatements.Count; i++)
            {
                _sql += string.Format(i == 0 ? " WHERE {0}" : " AND {0}",
                    validationStatements[i]);
            }
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
            _results = new List<T>();

            using (var reader = _context.ExecuteQuery<T>(_sql, _parameters))
            {
                var executionResult = reader.Select();

                _results.Add(executionResult);

                return executionResult;
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
            using (var reader = _context.ExecuteQuery<T>(_sql, _parameters))
            {
                _results = new List<T>();

                foreach (var item in reader)
                {
                    _results.Add(item);
                }

                return _results;
            }
        }

        public IEnumerator GetEnumerator<T>()
        {
            var reader = _context.ExecuteQuery<T>(_sql, _parameters);

            foreach (var item in reader)
            {
                _results.Add(item);

                yield return item;
            }
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
