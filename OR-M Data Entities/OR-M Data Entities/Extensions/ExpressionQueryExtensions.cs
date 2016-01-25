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
using System.Reflection;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Expressions;
using OR_M_Data_Entities.Expressions.Resolution;

// ReSharper disable once CheckNamespace
namespace OR_M_Data_Entities
{
    public static class ExpressionQueryExtensions
    {
        #region First
        public static TSource First<TSource>(this ExpressionQuery<TSource> source)
        {
            var resolvable = ((IExpressionQueryResolvable)source);

            TSource result;

            using (var reader = resolvable.DbContext.ExecuteQuery(source))
            {
                result = reader.First();
            }

            resolvable.DbContext.Dispose();

            return result;
        }

        public static TSource First<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            return First(source);
        }

        public static TSource FirstOrDefault<TSource>(this ExpressionQuery<TSource> source)
        {
            var resolvable = ((IExpressionQueryResolvable)source);

            TSource result;

            using (var reader = resolvable.DbContext.ExecuteQuery(source))
            {
                result = reader.FirstOrDefault();
            }

            resolvable.DbContext.Dispose();

            return result;
        }

        public static TSource FirstOrDefault<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            // execute sql, and grab data
            return FirstOrDefault(source);
        }
        #endregion

        #region Max
        public static decimal? Max(this ExpressionQuery<decimal?> source)
        {
            return _max(source);
        }

        public static decimal Max(this ExpressionQuery<decimal> source)
        {
            return _max(source);
        }

        public static double? Max(this ExpressionQuery<double?> source)
        {
            return _max(source);
        }

        public static double Max(this ExpressionQuery<double> source)
        {
            return _max(source);
        }

        public static float? Max(this ExpressionQuery<float?> source)
        {
            return _max(source);
        }

        public static float Max(this ExpressionQuery<float> source)
        {
            return _max(source);
        }

        public static int? Max(this ExpressionQuery<int?> source)
        {
            return _max(source);
        }

        public static int Max(this ExpressionQuery<int> source)
        {
            return _max(source);
        }

        public static long? Max(this ExpressionQuery<long?> source)
        {
            return _max(source);
        }

        public static long Max(this ExpressionQuery<long> source)
        {
            return _max(source);
        }
        #endregion

        #region Functions
        public static ExpressionQuery<T> Distinct<T>(this ExpressionQuery<T> source)
        {
            ((ExpressionQueryResolvable<T>)source).ResolveDistinct();

            return source;
        }

        public static ExpressionQuery<T> Take<T>(this ExpressionQuery<T> source, int rows)
        {
            ((ExpressionQueryResolvable<T>)source).ResolveTakeRows(rows);

            return source;
        }
        #endregion

        #region Min
        public static decimal? Min(this ExpressionQuery<decimal?> source)
        {
            return _min(source);
        }

        public static decimal Min(this ExpressionQuery<decimal> source)
        {
            return _min(source);
        }

        public static double? Min(this ExpressionQuery<double?> source)
        {
            return _min(source);
        }

        public static double Min(this ExpressionQuery<double> source)
        {
            return _min(source);
        }

        public static float? Min(this ExpressionQuery<float?> source)
        {
            return _min(source);
        }

        public static float Min(this ExpressionQuery<float> source)
        {
            return _min(source);
        }

        public static int? Min(this ExpressionQuery<int?> source)
        {
            return _min(source);
        }

        public static int Min(this ExpressionQuery<int> source)
        {
            return _min(source);
        }

        public static long? Min(this ExpressionQuery<long?> source)
        {
            return _min(source);
        }

        public static long Min(this ExpressionQuery<long> source)
        {
            return _min(source);
        }

        public static ExpressionQuery<TSource> Include<TSource>(this ExpressionQuery<TSource> source, string tableName)
        {
            ((ExpressionQueryResolvable<TSource>)source).ResolveInclude(tableName);

            return source;
        }
        #endregion

        #region Count
        public static int Count<TSource>(this ExpressionQuery<TSource> source)
        {
            //public static int Count<TSource>(this IEnumerable<TSource> source);
            ((ExpressionQueryResolvable<TSource>)source).ResolveCount();

            var resolvable = ((IExpressionQueryResolvable)source);
            int result;

            using (var reader = resolvable.DbContext.ExecuteQuery(source))
            {
                var peekDataReaderField = reader.GetType()
                    .GetField("_reader", BindingFlags.Instance | BindingFlags.NonPublic);
                var peekDataReader = peekDataReaderField.GetValue(reader);

                using (var dbReader = new DataReader<int>(peekDataReader as PeekDataReader))
                {
                    result = dbReader.FirstOrDefault();
                }
            }

            resolvable.DbContext.Dispose();

            return result;
        }

        public static int Count<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            // execute sql, and grab data
            return Count(source);
        }
        #endregion

        #region To List
        public static List<TSource> ToList<TSource>(this ExpressionQuery<TSource> source)
        {
            var resolvable = ((IExpressionQueryResolvable)source);

            List<TSource> result;

            using (var reader = resolvable.DbContext.ExecuteQuery(source))
            {
                result = reader.ToList();
            }

            resolvable.DbContext.Dispose();

            return result;
        }

        public static List<TSource> ToList<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            return ToList(source);
        }
        #endregion

        #region Order By
        public static OrderedExpressionQuery<TSource> OrderBy<TSource, TKey>(this ExpressionQuery<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            if (source.HasForeignKeys) throw new OrderByException("Cannot Order Expression Query that has foreign keys.  Consider returning the results then ordering.");

            return ((ExpressionQueryResolvable<TSource>)source).ResolveOrderBy(keySelector);
        }

        public static OrderedExpressionQuery<TSource> OrderByDescending<TSource, TKey>(this ExpressionQuery<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            if (source.HasForeignKeys) throw new OrderByException("Cannot Order Expression Query that has foreign keys.  Consider returning the results then ordering.");

            return ((ExpressionQueryResolvable<TSource>)source).ResolveOrderByDescending(keySelector);
        }

        public static OrderedExpressionQuery<TSource> ThenBy<TSource, TKey>(this OrderedExpressionQuery<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            if (source.HasForeignKeys) throw new OrderByException("Cannot Order Expression Query that has foreign keys.  Consider returning the results then ordering.");

            return ((ExpressionQueryResolvable<TSource>)source).ResolveOrderBy(keySelector);
        }

        public static OrderedExpressionQuery<TSource> ThenByDescending<TSource, TKey>(this OrderedExpressionQuery<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            if (source.HasForeignKeys) throw new OrderByException("Cannot Order Expression Query that has foreign keys.  Consider returning the results then ordering.");

            return ((ExpressionQueryResolvable<TSource>)source).ResolveOrderByDescending(keySelector);
        }
        #endregion

        #region Any
        public static bool Any<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            return Any(source);
        }

        public static bool Any<TSource>(this ExpressionQuery<TSource> source)
        {
            // only take one, we only care if it exists or not
            source.Take(1);

            var resolvable = ((IExpressionQueryResolvable)source);

            bool result;

            using (var reader = resolvable.DbContext.ExecuteQuery(source))
            {
                result = reader.HasRows;
            }

            resolvable.DbContext.Dispose();

            return result;
        }
        #endregion

        #region Where
        public static ExpressionQuery<TSource> Where<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            //var s = DateTime.Now;
            //((ExpressionQueryResolvable<TSource>)source).ResolveWhere(expression);
            //var e = DateTime.Now;

            //Console.WriteLine((e - s).TotalMilliseconds);


            var item = ExpressionQueryResolver.Resolve(source, expression);
            if (item != null)
            {
                foreach (var sqlDbParameter in item.Parameters)
                {

                }
            }

            ((ExpressionQueryResolvable<TSource>)source).ResolveWhere(expression);

            return source;
        }
        #endregion

        #region Joins
        public static ExpressionQuery<TResult> InnerJoin<TOuter, TInner, TKey, TResult>(this ExpressionQuery<TOuter> outer,
            ExpressionQuery<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            return ((ExpressionQueryResolvable<TOuter>)outer).ResolveJoin(inner, outerKeySelector,
                innerKeySelector, resultSelector, JoinType.Inner);
        }

        public static ExpressionQuery<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(this ExpressionQuery<TOuter> outer,
            ExpressionQuery<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            return ((ExpressionQueryResolvable<TOuter>)outer).ResolveJoin(inner, outerKeySelector,
                innerKeySelector, resultSelector, JoinType.Left);
        }
        #endregion

        #region Select
        public static ExpressionQuery<TResult> Select<TSource, TResult>(this ExpressionQuery<TSource> source,
            Expression<Func<TSource, TResult>> selector)
        {
            var item = ExpressionQuerySelectResolver.Resolve(source, selector);
            if (item != null)
            {
            }


            return ((ExpressionQueryResolvable<TSource>)source).ResolveSelect(selector, source);
        }
        #endregion

        private static T _max<T>(this ExpressionQuery<T> source)
        {
            ((ExpressionQueryResolvable<T>)source).ResoveMax();

            return source.FirstOrDefault();
        }

        private static T _min<T>(this ExpressionQuery<T> source)
        {
            ((ExpressionQueryResolvable<T>)source).ResoveMin();

            return source.FirstOrDefault();
        }

        public static bool IsExpressionQuery(this MethodCallExpression expression)
        {
            return expression != null && (expression.Method.ReturnType.IsGenericType &&
                                          expression.Method.ReturnType.GetGenericTypeDefinition()
                                              .IsAssignableFrom(typeof(ExpressionQuery<>)));
        }

        public static bool IsExpressionQuery(this Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition()
                       .IsAssignableFrom(typeof(ExpressionQuery<>)) || type.IsGenericType &&
                   type.GetGenericTypeDefinition()
                       .IsAssignableFrom(typeof(ExpressionQueryResolvable<>));
        }

        public static bool IsExpressionQuery(this object o)
        {
            return IsExpressionQuery(o.GetType());
        }
    }
}
