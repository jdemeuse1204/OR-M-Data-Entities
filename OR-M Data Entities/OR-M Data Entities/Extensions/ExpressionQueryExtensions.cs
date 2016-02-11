/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using OR_M_Data_Entities.Expressions;

// ReSharper disable once CheckNamespace
namespace OR_M_Data_Entities
{
    public static class ExpressionQueryExtensions
    {
        #region First
        public static TSource First<TSource>(this IExpressionQuery<TSource> source)
        {
            TSource result;
            var resolvable = ((IExpressionQueryResolvable<TSource>) source);

            // select all
            resolvable.SelectAll();

            // order by primary keys
            resolvable.OrderByPrimaryKeys();

            // resolve the foreign key joins
            resolvable.ResolveForeignKeyJoins();

            // get the object
            using (var reader = resolvable.ExecuteReader()) result = reader.First();

            // clean disconnect from the server
            resolvable.Disconnect();

            return result;
        }

        public static TSource First<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            return First(source);
        }

        public static TSource FirstOrDefault<TSource>(this IExpressionQuery<TSource> source)
        {
            //var resolvable = ((IExpressionQueryResolvable)source);

            //TSource result;

            //using (var reader = resolvable.DbContext.ExecuteQuery(source))
            //{
            //    result = reader.FirstOrDefault();
            //}

            //resolvable.DbContext.Dispose();

            return default(TSource);
        }

        public static TSource FirstOrDefault<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            // execute sql, and grab data
            return FirstOrDefault(source);
        }
        #endregion

        #region Max
        public static decimal? Max(this IExpressionQuery<decimal?> source)
        {
            return _max(source);
        }

        public static decimal Max(this IExpressionQuery<decimal> source)
        {
            return _max(source);
        }

        public static double? Max(this IExpressionQuery<double?> source)
        {
            return _max(source);
        }

        public static double Max(this IExpressionQuery<double> source)
        {
            return _max(source);
        }

        public static float? Max(this IExpressionQuery<float?> source)
        {
            return _max(source);
        }

        public static float Max(this IExpressionQuery<float> source)
        {
            return _max(source);
        }

        public static int? Max(this IExpressionQuery<int?> source)
        {
            return _max(source);
        }

        public static int Max(this IExpressionQuery<int> source)
        {
            return _max(source);
        }

        public static long? Max(this IExpressionQuery<long?> source)
        {
            return _max(source);
        }

        public static long Max(this IExpressionQuery<long> source)
        {
            return _max(source);
        }
        #endregion

        #region Functions
        public static IExpressionQuery<T> Distinct<T>(this IExpressionQuery<T> source)
        {
            //((ExpressionQueryResolvable<T>)source).ResolveDistinct();

            return source;
        }

        public static IExpressionQuery<T> Take<T>(this IExpressionQuery<T> source, int rows)
        {
            //((ExpressionQueryResolvable<T>)source).ResolveTakeRows(rows);

            return source;
        }
        #endregion

        #region Min
        public static decimal? Min(this IExpressionQuery<decimal?> source)
        {
            return _min(source);
        }

        public static decimal Min(this IExpressionQuery<decimal> source)
        {
            return _min(source);
        }

        public static double? Min(this IExpressionQuery<double?> source)
        {
            return _min(source);
        }

        public static double Min(this IExpressionQuery<double> source)
        {
            return _min(source);
        }

        public static float? Min(this IExpressionQuery<float?> source)
        {
            return _min(source);
        }

        public static float Min(this IExpressionQuery<float> source)
        {
            return _min(source);
        }

        public static int? Min(this IExpressionQuery<int?> source)
        {
            return _min(source);
        }

        public static int Min(this IExpressionQuery<int> source)
        {
            return _min(source);
        }

        public static long? Min(this IExpressionQuery<long?> source)
        {
            return _min(source);
        }

        public static long Min(this IExpressionQuery<long> source)
        {
            return _min(source);
        }

        public static IExpressionQuery<TSource> Include<TSource>(this IExpressionQuery<TSource> source, string tableName)
        {
            //((ExpressionQueryResolvable<TSource>)source).ResolveInclude(tableName);

            return source;
        }

        public static IExpressionQuery<TSource> IncludeAll<TSource>(this IExpressionQuery<TSource> source, string tableName)
        {
            //((ExpressionQueryResolvable<TSource>)source).ResolveInclude(tableName);

            return source;
        }

        public static IExpressionQuery<TSource> IncludeUpTo<TSource>(this IExpressionQuery<TSource> source, string tableName)
        {
            //((ExpressionQueryResolvable<TSource>)source).ResolveInclude(tableName);

            return source;
        }
        #endregion

        #region Count
        public static int Count<TSource>(this IExpressionQuery<TSource> source)
        {
            //public static int Count<TSource>(this IEnumerable<TSource> source);
            //((ExpressionQueryResolvable<TSource>)source).ResolveCount();

            //var resolvable = ((IExpressionQueryResolvable)source);
            //int result;

            //using (var reader = resolvable.DbContext.ExecuteQuery(source))
            //{
            //    var peekDataReaderField = reader.GetType()
            //        .GetField("_reader", BindingFlags.Instance | BindingFlags.NonPublic);
            //    var peekDataReader = peekDataReaderField.GetValue(reader);

            //    using (var dbReader = new DataReader<int>(peekDataReader as PeekDataReader))
            //    {
            //        result = dbReader.FirstOrDefault();
            //    }
            //}

            //resolvable.DbContext.Dispose();

            return 1;
        }

        public static int Count<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            // execute sql, and grab data
            return Count(source);
        }
        #endregion

        #region To List
        public static List<TSource> ToList<TSource>(this IExpressionQuery<TSource> source)
        {
            //var resolvable = ((IExpressionQueryResolvable)source);

            //List<TSource> result;

            //using (var reader = resolvable.DbContext.ExecuteQuery(source))
            //{
            //    result = reader.ToList();
            //}

            //resolvable.DbContext.Dispose();

            return new List<TSource>();
        }

        public static List<TSource> ToList<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            return ToList(source);
        }
        #endregion

        #region Order By
        //public static OrderedExpressionQuery<TSource> OrderBy<TSource, TKey>(this IExpressionQuery<TSource> source,
        //    Expression<Func<TSource, TKey>> keySelector)
        //{
        //    //if (source.HasForeignKeys) throw new OrderByException("Cannot Order Expression Query that has foreign keys.  Consider returning the results then ordering.");

        //    //return ((ExpressionQueryResolvable<TSource>)source).ResolveOrderBy(keySelector);

        //    return null;
        //}

        //public static OrderedExpressionQuery<TSource> OrderByDescending<TSource, TKey>(this IExpressionQuery<TSource> source,
        //    Expression<Func<TSource, TKey>> keySelector)
        //{
        //    //if (source.HasForeignKeys) throw new OrderByException("Cannot Order Expression Query that has foreign keys.  Consider returning the results then ordering.");

        //    //return ((ExpressionQueryResolvable<TSource>)source).ResolveOrderByDescending(keySelector);

        //    return null;
        //}

        //public static OrderedExpressionQuery<TSource> ThenBy<TSource, TKey>(this OrderedExpressionQuery<TSource> source,
        //    Expression<Func<TSource, TKey>> keySelector)
        //{
        //    //if (source.HasForeignKeys) throw new OrderByException("Cannot Order Expression Query that has foreign keys.  Consider returning the results then ordering.");

        //    //return ((ExpressionQueryResolvable<TSource>)source).ResolveOrderBy(keySelector);

        //    return null;
        //}

        //public static OrderedExpressionQuery<TSource> ThenByDescending<TSource, TKey>(this OrderedExpressionQuery<TSource> source,
        //    Expression<Func<TSource, TKey>> keySelector)
        //{
        //    //if (source.HasForeignKeys) throw new OrderByException("Cannot Order Expression Query that has foreign keys.  Consider returning the results then ordering.");

        //    //return ((ExpressionQueryResolvable<TSource>)source).ResolveOrderByDescending(keySelector);

        //    return null;
        //}
        #endregion

        #region Any
        public static bool Any<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            return Any(source);
        }

        public static bool Any<TSource>(this IExpressionQuery<TSource> source)
        {
            // only take one, we only care if it exists or not
            //source.Take(1);

            //var resolvable = ((IExpressionQueryResolvable)source);

            //bool result;

            //using (var reader = resolvable.DbContext.ExecuteQuery(source))
            //{
            //    result = reader.HasRows;
            //}

            //resolvable.DbContext.Dispose();

            return false;
        }
        #endregion

        #region Where
        public static IExpressionQuery<TSource> Where<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            ((IExpressionQueryResolvable<TSource>)source).ResolveWhere(expression);

            return source;
        }
        #endregion

        #region Joins
        public static IExpressionQuery<TResult> InnerJoin<TOuter, TInner, TKey, TResult>(this IExpressionQuery<TOuter> outer,
            IExpressionQuery<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            //return ((ExpressionQueryResolvable<TOuter>)outer).ResolveJoin(inner, outerKeySelector,
            //    innerKeySelector, resultSelector, JoinType.Inner);

            return null;
        }

        public static IExpressionQuery<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(this IExpressionQuery<TOuter> outer,
            IExpressionQuery<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            //return ((ExpressionQueryResolvable<TOuter>)outer).ResolveJoin(inner, outerKeySelector,
            //    innerKeySelector, resultSelector, JoinType.Left);

            return null;
        }
        #endregion

        #region Select
        public static IExpressionQuery<TResult> Select<TSource, TResult>(this IExpressionQuery<TSource> source,
            Expression<Func<TSource, TResult>> selector)
        {
            //var item = ExpressionQuerySelectResolver.Resolve(source, selector);
            //if (item != null)
            //{
            //}


            //return ((ExpressionQueryResolvable<TSource>)source).ResolveSelect(selector, source);

            return null;
        }
        #endregion

        private static T _max<T>(this IExpressionQuery<T> source)
        {
            //((ExpressionQueryResolvable<T>)source).ResoveMax();

            return source.FirstOrDefault();
        }

        private static T _min<T>(this IExpressionQuery<T> source)
        {
            //((ExpressionQueryResolvable<T>)source).ResoveMin();

            return source.FirstOrDefault();
        }

        public static bool IsExpressionQuery(this MethodCallExpression expression)
        {
            return expression != null && (expression.Method.ReturnType.IsGenericType &&
                                          expression.Method.ReturnType.GetGenericTypeDefinition()
                                              .IsAssignableFrom(typeof(IExpressionQuery<>)));
        }

        public static bool IsExpressionQuery(this Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition()
                       .IsAssignableFrom(typeof(IExpressionQuery<>));
        }

        public static bool IsExpressionQuery(this object o)
        {
            return IsExpressionQuery(o.GetType());
        }
    }
}
