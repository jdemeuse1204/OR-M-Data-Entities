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

        private void ResolveExpression()
        {
            if (SelectsList.Count == 0) throw new ArgumentException("No columns selected, please use .Select<T>() to select columns.");

            // get the expression type            
            var expression = _getExpressionType();

            // resolve the expression
            expression.Resolve();

            // END OF CODE



            // Turn the Where Lambda Statements into Sql
            var where = new List<SqlWhere>();

            foreach (var resolution in WheresList.Select(item => GetWheres(item as dynamic)))
            {
                @where.AddRange(resolution);
            }

            // Turn the Inner Join Lambda Statements into Sql
            var innerJoin = new Dictionary<KeyValuePair<Type, Type>, SqlJoin>();

            foreach (var resolution in InnerJoinsList.Select(item => item is SqlJoin ? item : GetJoins(item as dynamic, JoinType.Inner)))
            {
                if (resolution is SqlJoin)
                {
                    innerJoin.Add(
                        new KeyValuePair<Type, Type>(
                            ((SqlJoin)resolution).ParentEntity.Table,
                            ((SqlJoin)resolution).JoinEntity.Table),
                        resolution);
                }
                else
                {
                    foreach (var item in resolution)
                    {
                        innerJoin.Add(item.Key, item.Value);
                    }
                }
            }

            // Turn the Left Join Lambda Statements into Sql
            var leftJoin = new Dictionary<KeyValuePair<Type, Type>, SqlJoin>();

            foreach (var resolution in LeftJoinsList.Select(item => item is SqlJoin ? item : GetJoins(item as dynamic, JoinType.Left)))
            {
                if (resolution is SqlJoin)
                {
                    leftJoin.Add(
                        new KeyValuePair<Type, Type>(
                            ((SqlJoin)resolution).ParentEntity.Table,
                            ((SqlJoin)resolution).JoinEntity.Table),
                        resolution);
                }
                else
                {
                    // REMOVE ME DAMMIT!
                    foreach (var item in resolution)
                    {
                        leftJoin.Add(item.Key, item.Value);
                    }
                }
            }

            // Turn the Select Lambda Statements into Sql
            var select = new List<SqlTableColumnPair>();

            foreach (var resolution in SelectsList.Select(item => GetSelects(item as dynamic)))
            {
                @select.AddRange(resolution);
            }


            var distinctTypes = select.Select(w => w.Table).Distinct().ToList();

            var typeHasForeignKeys = DatabaseSchemata.HasForeignKeys(distinctTypes.First());

            // Resolve Joins And Selects for FKs
            // make sure all joins and fields are incorporated into sql
            if (distinctTypes.Count == 1 && typeHasForeignKeys)
            {
                List<Type> distinctSelectTypes;
                var foreignKeyJoins = DatabaseSchemata.GetForeignKeyJoinsRecursive(distinctTypes.First(), out distinctSelectTypes);

                foreach (var keyJoin in foreignKeyJoins.Where(keyJoin => !innerJoin.ContainsKey(keyJoin.Key)))
                {
                    innerJoin.Add(keyJoin.Key, keyJoin.Value);
                }

                foreach (var distinctSelectType in distinctSelectTypes)
                {
                    select.AddRange(DatabaseSchemata.GetTableColumnPairsFromTable(distinctSelectType).Where(w => !select.Contains(w)).ToList());
                }
            }

            var selectTopModifier = TakeTopRows == -1 ? string.Empty : string.Format(" TOP {0} ", TakeTopRows);
            var selectDistinctModifier = IsDistinct ? "DISTINCT" : string.Empty;

            // add the select modifier
            Sql += string.Format(" SELECT {0}{1} ", selectDistinctModifier, selectTopModifier);

            var selectText = @select.Aggregate(string.Empty, (current, item) => current + string.Format("{0},", item.GetSelectColumnTextWithAlias()));

            // trim the ending comma
            Sql += selectText.TrimEnd(',');

            Sql += string.Format(" FROM [{0}] ", FromTableName);

            var innerJoinText = @innerJoin.Aggregate(string.Empty, (current, item) => current + item.Value.GetJoinText());

            if (!string.IsNullOrWhiteSpace(innerJoinText))
            {
                Sql += innerJoinText;
            }

            var leftJoinText = @leftJoin.Aggregate(string.Empty, (current, item) => current + item.Value.GetJoinText());

            if (!string.IsNullOrWhiteSpace(leftJoinText))
            {
                Sql += leftJoinText;
            }

            var validationStatements = @where.Select(item => item.GetWhereText(Parameters)).ToList();

            var finalValidationStatement = validationStatements.Aggregate(string.Empty, (current, validationStatement) => current + string.Format(string.IsNullOrWhiteSpace(current) ? " WHERE {0}" : " AND {0}", validationStatement));

            Sql += finalValidationStatement;
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
            ResolveExpression();

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
            ResolveExpression();

            using (var reader = _context.ExecuteQuery<T>(Sql, Parameters))
            {
                return reader.Cast<T>().ToList();
            }
        }

        public IEnumerator GetEnumerator<T>()
        {
            ResolveExpression();

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
