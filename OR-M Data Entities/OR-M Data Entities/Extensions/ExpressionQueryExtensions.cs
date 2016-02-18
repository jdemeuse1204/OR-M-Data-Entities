/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Expressions;

// ReSharper disable once CheckNamespace
namespace OR_M_Data_Entities
{
    /// <summary>
    /// All methods that return IExpressionQuery<T> must to so, this way
    /// the methods can be chained together
    /// </summary>
    public static class ExpressionQueryExtensions
    {
        #region First
        public static TSource First<TSource>(this IExpressionQuery<TSource> source)
        {
            return _first(source, false);
        }

        public static TSource First<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            return First(source);
        }

        public static TSource First<TSource>(this IOrderedExpressionQuery<TSource> source)
        {
            return First((IExpressionQuery<TSource>)source);
        }

        public static TSource First<TSource>(this IOrderedExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return First((IExpressionQuery<TSource>)source, expression);
        }

        public static TSource FirstOrDefault<TSource>(this IExpressionQuery<TSource> source)
        {
            return _first(source, true);
        }

        public static TSource FirstOrDefault<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            // execute sql, and grab data
            return FirstOrDefault(source);
        }

        public static TSource FirstOrDefault<TSource>(this IOrderedExpressionQuery<TSource> source)
        {
            return FirstOrDefault((IExpressionQuery<TSource>)source);
        }

        public static TSource FirstOrDefault<TSource>(this IOrderedExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return FirstOrDefault((IExpressionQuery<TSource>)source, expression);
        }

        private static TSource _first<TSource>(this IExpressionQuery<TSource> source, bool isFirstOrDefault)
        {
            TSource result;

            var resolvable = ((IExpressionQueryResolvable<TSource>)source);

            // get the object
            using (var reader = resolvable.ExecuteReader<TSource>()) result = isFirstOrDefault ? reader.FirstOrDefault() : reader.First();

            // disconnect from the server
            resolvable.Disconnect();

            return result;
        }
        #endregion

        #region Functions
        public static IExpressionQuery<T> Distinct<T>(this IExpressionQuery<T> source)
        {
            ((IExpressionQueryResolvable<T>)source).MakeDistinct();

            return source;
        }

        public static IOrderedExpressionQuery<T> Distinct<T>(this IOrderedExpressionQuery<T> source)
        {
            ((IExpressionQueryResolvable<T>)source).MakeDistinct();

            return source;
        }

        public static IExpressionQuery<T> Take<T>(this IExpressionQuery<T> source, int rows)
        {
            ((IExpressionQueryResolvable<T>)source).Take(rows);

            return source;
        }

        public static IOrderedExpressionQuery<T> Take<T>(this IOrderedExpressionQuery<T> source, int rows)
        {
            ((IExpressionQueryResolvable<T>)source).Take(rows);

            return source;
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


        public static decimal? Max(this IOrderedExpressionQuery<decimal?> source)
        {
            return _max(source);
        }

        public static decimal Max(this IOrderedExpressionQuery<decimal> source)
        {
            return _max(source);
        }

        public static double? Max(this IOrderedExpressionQuery<double?> source)
        {
            return _max(source);
        }

        public static double Max(this IOrderedExpressionQuery<double> source)
        {
            return _max(source);
        }

        public static float? Max(this IOrderedExpressionQuery<float?> source)
        {
            return _max(source);
        }

        public static float Max(this IOrderedExpressionQuery<float> source)
        {
            return _max(source);
        }

        public static int? Max(this IOrderedExpressionQuery<int?> source)
        {
            return _max(source);
        }

        public static int Max(this IOrderedExpressionQuery<int> source)
        {
            return _max(source);
        }

        public static long? Max(this IOrderedExpressionQuery<long?> source)
        {
            return _max(source);
        }

        public static long Max(this IOrderedExpressionQuery<long> source)
        {
            return _max(source);
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


        public static decimal? Min(this IOrderedExpressionQuery<decimal?> source)
        {
            return _min(source);
        }

        public static decimal Min(this IOrderedExpressionQuery<decimal> source)
        {
            return _min(source);
        }

        public static double? Min(this IOrderedExpressionQuery<double?> source)
        {
            return _min(source);
        }

        public static double Min(this IOrderedExpressionQuery<double> source)
        {
            return _min(source);
        }

        public static float? Min(this IOrderedExpressionQuery<float?> source)
        {
            return _min(source);
        }

        public static float Min(this IOrderedExpressionQuery<float> source)
        {
            return _min(source);
        }

        public static int? Min(this IOrderedExpressionQuery<int?> source)
        {
            return _min(source);
        }

        public static int Min(this IOrderedExpressionQuery<int> source)
        {
            return _min(source);
        }

        public static long? Min(this IOrderedExpressionQuery<long?> source)
        {
            return _min(source);
        }

        public static long Min(this IOrderedExpressionQuery<long> source)
        {
            return _min(source);
        }
        #endregion

        #region Min/Max
        private static T _max<T>(this IExpressionQuery<T> source)
        {
            ((IExpressionQueryResolvable<T>)source).ResolveMax();

            return source.FirstOrDefault();
        }

        private static T _max<T>(this IOrderedExpressionQuery<T> source)
        {
            return _max((IExpressionQuery<T>) source);
        }

        private static T _min<T>(this IExpressionQuery<T> source)
        {
            ((IExpressionQueryResolvable<T>)source).ResolveMin();

            return source.FirstOrDefault();
        }

        private static T _min<T>(this IOrderedExpressionQuery<T> source)
        {
            return _min((IExpressionQuery<T>)source);
        }
        #endregion

        #region To List
        public static List<TSource> ToList<TSource>(this IExpressionQuery<TSource> source)
        {
            List<TSource> result;

            var resolvable = ((IExpressionQueryResolvable<TSource>)source);

            // get the object
            using (var reader = resolvable.ExecuteReader<TSource>()) result = reader.ToList();

            // disconnect from the server
            resolvable.Disconnect();

            return result;
        }

        public static List<TSource> ToList<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            return ToList(source);
        }

        public static List<TSource> ToList<TSource>(this IOrderedExpressionQuery<TSource> source)
        {
            return ToList((IExpressionQuery<TSource>) source);
        }

        public static List<TSource> ToList<TSource>(this IOrderedExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return ToList((IExpressionQuery<TSource>)source, expression);
        }
        #endregion

        #region Any
        public static bool Any<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            return Any(source);
        }

        public static bool Any<TSource>(this IExpressionQuery<TSource> source)
        {
            bool result;

            var resolvable = ((IExpressionQueryResolvable<TSource>)source);

            // all we need to do is Select Top 1 1
            resolvable.ResolveAny();

            // get the object
            using (var reader = resolvable.ExecuteReader<TSource>()) result = reader.HasRows;

            // disconnect from the server
            resolvable.Disconnect();

            return result;
        }

        public static bool Any<TSource>(this IOrderedExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return Any((IExpressionQuery<TSource>)source, expression);
        }

        public static bool Any<TSource>(this IOrderedExpressionQuery<TSource> source)
        {
            return Any((IExpressionQuery<TSource>)source);
        }
        #endregion

        #region Where
        public static IExpressionQuery<TSource> Where<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            ((IExpressionQueryResolvable<TSource>)source).ResolveWhere(expression);

            return source;
        }
        #endregion

        #region Include
        public static IExpressionQuery<TSource> Include<TSource>(this IExpressionQuery<TSource> source, string tableName)
        {
            ((IExpressionQueryResolvable<TSource>)source).Include(tableName);

            return source;
        }

        public static IExpressionQuery<TSource> IncludeAll<TSource>(this IExpressionQuery<TSource> source)
        {
            ((IExpressionQueryResolvable<TSource>)source).IncludeAll();

            return source;
        }

        public static IExpressionQuery<TSource> IncludeTo<TSource>(this IExpressionQuery<TSource> source, string tableName)
        {
            source.AsResolvable().IncludeTo(tableName);

            return source;
        }

        private static IExpressionQueryResolvable<T> AsResolvable<T>(this IExpressionQuery<T> source)
        {
            return ((IExpressionQueryResolvable<T>) source);
        }
        #endregion

        #region Count
        public static int Count<TSource>(this IExpressionQuery<TSource> source)
        {
            int result;

            var resolvable = ((IExpressionQueryResolvable<TSource>)source);

            // tell resolvable to do a count
            resolvable.ResolveCount();

            // get the object
            using (var reader = resolvable.ExecuteReader<int>()) result = reader.FirstOrDefault();

            // disconnect from the server
            resolvable.Disconnect();

            return result;
        }

        public static int Count<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            // execute sql, and grab data
            return Count(source);
        }
        #endregion

        #region Order By
        public static IOrderedExpressionQuery<TSource> OrderBy<TSource, TKey>(this IExpressionQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            var resolvable = ((IExpressionQueryResolvable<TSource>)source);

            // make sure foreign keys are not selected
            _foreignKeyCheck(resolvable);

            resolvable.ResolveOrderBy(keySelector);

            return (IOrderedExpressionQuery<TSource>)source;
        }

        public static IOrderedExpressionQuery<TSource> OrderByDescending<TSource, TKey>(this IExpressionQuery<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            var resolvable = ((IExpressionQueryResolvable<TSource>)source);

            // make sure foreign keys are not selected
            _foreignKeyCheck(resolvable);

            resolvable.ResolveOrderByDescending(keySelector);

            return (IOrderedExpressionQuery<TSource>)source;
        }

        public static IOrderedExpressionQuery<TSource> ThenBy<TSource, TKey>(this IOrderedExpressionQuery<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            var resolvable = ((IExpressionQueryResolvable<TSource>)source);

            // make sure foreign keys are not selected
            _foreignKeyCheck(resolvable);

            resolvable.ResolveOrderBy(keySelector);

            return source;
        }

        public static IOrderedExpressionQuery<TSource> ThenByDescending<TSource, TKey>(this IOrderedExpressionQuery<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            var resolvable = ((IExpressionQueryResolvable<TSource>)source);

            // make sure foreign keys are not selected
            _foreignKeyCheck(resolvable);

            resolvable.ResolveOrderByDescending(keySelector);

            return source;
        }

        private static void _foreignKeyCheck<T>(IExpressionQueryResolvable<T> source)
        {
            if (source.AreForeignKeysSelected()) throw new OrderByException("Cannot Order Expression Query that has foreign keys.  Consider returning the results then ordering.  If Lazy loading do not inculde any foreign keys.");
        }
        #endregion


        // not done

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
        public static IExpressionQuery<TResult> Select<TSource, TResult>(this IExpressionQuery<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            var resolvable = ((IExpressionQueryResolvable<TSource>)source);

            return resolvable.ResolveSelect(source, selector);
        }

        public static IOrderedExpressionQuery<TResult> Select<TSource, TResult>(this IOrderedExpressionQuery<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            var resolvable = ((IExpressionQueryResolvable<TSource>)source);

            return resolvable.ResolveSelect(source, selector);
        }
        #endregion



    }
}
