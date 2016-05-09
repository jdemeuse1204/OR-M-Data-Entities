using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Expressions;

// ReSharper disable once CheckNamespace
namespace OR_M_Data_Entities
{
    public static class MockExtensions
    {
        #region First
        public static TSource Mock_First<TSource>(this IExpressionQuery<TSource> source)
        {
            return _first(source, false);
        }

        public static TSource Mock_First<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            return Mock_First(source);
        }

        public static TSource Mock_First<TSource>(this IOrderedExpressionQuery<TSource> source)
        {
            return Mock_First(source._asExpressionQuery());
        }

        public static TSource Mock_First<TSource>(this IOrderedExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return Mock_First(source._asExpressionQuery(), expression);
        }

        public static TSource Mock_FirstOrDefault<TSource>(this IExpressionQuery<TSource> source)
        {
            return _first(source, true);
        }

        public static TSource Mock_FirstOrDefault<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            // execute sql, and grab data
            return Mock_FirstOrDefault(source);
        }

        public static TSource Mock_FirstOrDefault<TSource>(this IOrderedExpressionQuery<TSource> source)
        {
            return Mock_FirstOrDefault(source._asExpressionQuery());
        }

        public static TSource Mock_FirstOrDefault<TSource>(this IOrderedExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return Mock_FirstOrDefault(source._asExpressionQuery(), expression);
        }

        private static TSource _first<TSource>(this IExpressionQuery<TSource> source, bool isFirstOrDefault)
        {
            TSource result;

            var resolvable = source._asResolvable();

            // get the object
            using (var reader = resolvable.ExecuteReader<TSource>()) result = isFirstOrDefault ? reader.FirstOrDefault() : reader.First();

            // disconnect from the server
            resolvable.Disconnect();

            return result;
        }
        #endregion

        #region Functions
        public static IExpressionQuery<T> Mock_Distinct<T>(this IExpressionQuery<T> source)
        {
            source._asResolvable().Distinct();

            return source;
        }

        public static IOrderedExpressionQuery<T> Mock_Distinct<T>(this IOrderedExpressionQuery<T> source)
        {
            source._asResolvable().Distinct();

            return source;
        }

        public static IExpressionQuery<T> Mock_Take<T>(this IExpressionQuery<T> source, int rows)
        {
            source._asResolvable().Take(rows);

            return source;
        }

        public static IOrderedExpressionQuery<T> Mock_Take<T>(this IOrderedExpressionQuery<T> source, int rows)
        {
            source._asResolvable().Take(rows);

            return source;
        }
        #endregion

        #region Max
        public static decimal? Mock_Max(this IExpressionQuery<decimal?> source)
        {
            return _max(source);
        }

        public static decimal Mock_Max(this IExpressionQuery<decimal> source)
        {
            return _max(source);
        }

        public static double? Mock_Max(this IExpressionQuery<double?> source)
        {
            return _max(source);
        }

        public static double Mock_Max(this IExpressionQuery<double> source)
        {
            return _max(source);
        }

        public static float? Mock_Max(this IExpressionQuery<float?> source)
        {
            return _max(source);
        }

        public static float Mock_Max(this IExpressionQuery<float> source)
        {
            return _max(source);
        }

        public static int? Mock_Max(this IExpressionQuery<int?> source)
        {
            return _max(source);
        }

        public static int Mock_Max(this IExpressionQuery<int> source)
        {
            return _max(source);
        }

        public static long? Mock_Max(this IExpressionQuery<long?> source)
        {
            return _max(source);
        }

        public static long Mock_Max(this IExpressionQuery<long> source)
        {
            return _max(source);
        }


        public static decimal? Mock_Max(this IOrderedExpressionQuery<decimal?> source)
        {
            return _max(source);
        }

        public static decimal Mock_Max(this IOrderedExpressionQuery<decimal> source)
        {
            return _max(source);
        }

        public static double? Mock_Max(this IOrderedExpressionQuery<double?> source)
        {
            return _max(source);
        }

        public static double Mock_Max(this IOrderedExpressionQuery<double> source)
        {
            return _max(source);
        }

        public static float? Mock_Max(this IOrderedExpressionQuery<float?> source)
        {
            return _max(source);
        }

        public static float Mock_Max(this IOrderedExpressionQuery<float> source)
        {
            return _max(source);
        }

        public static int? Mock_Max(this IOrderedExpressionQuery<int?> source)
        {
            return _max(source);
        }

        public static int Mock_Max(this IOrderedExpressionQuery<int> source)
        {
            return _max(source);
        }

        public static long? Mock_Max(this IOrderedExpressionQuery<long?> source)
        {
            return _max(source);
        }

        public static long Mock_Max(this IOrderedExpressionQuery<long> source)
        {
            return _max(source);
        }
        #endregion

        #region Min
        public static decimal? Mock_Min(this IExpressionQuery<decimal?> source)
        {
            return _min(source);
        }

        public static decimal Mock_Min(this IExpressionQuery<decimal> source)
        {
            return _min(source);
        }

        public static double? Mock_Min(this IExpressionQuery<double?> source)
        {
            return _min(source);
        }

        public static double Mock_Min(this IExpressionQuery<double> source)
        {
            return _min(source);
        }

        public static float? Mock_Min(this IExpressionQuery<float?> source)
        {
            return _min(source);
        }

        public static float Mock_Min(this IExpressionQuery<float> source)
        {
            return _min(source);
        }

        public static int? Mock_Min(this IExpressionQuery<int?> source)
        {
            return _min(source);
        }

        public static int Mock_Min(this IExpressionQuery<int> source)
        {
            return _min(source);
        }

        public static long? Mock_Min(this IExpressionQuery<long?> source)
        {
            return _min(source);
        }

        public static long Mock_Min(this IExpressionQuery<long> source)
        {
            return _min(source);
        }


        public static decimal? Mock_Min(this IOrderedExpressionQuery<decimal?> source)
        {
            return _min(source);
        }

        public static decimal Mock_Min(this IOrderedExpressionQuery<decimal> source)
        {
            return _min(source);
        }

        public static double? Mock_Min(this IOrderedExpressionQuery<double?> source)
        {
            return _min(source);
        }

        public static double Mock_Min(this IOrderedExpressionQuery<double> source)
        {
            return _min(source);
        }

        public static float? Mock_Min(this IOrderedExpressionQuery<float?> source)
        {
            return _min(source);
        }

        public static float Mock_Min(this IOrderedExpressionQuery<float> source)
        {
            return _min(source);
        }

        public static int? Mock_Min(this IOrderedExpressionQuery<int?> source)
        {
            return _min(source);
        }

        public static int Mock_Min(this IOrderedExpressionQuery<int> source)
        {
            return _min(source);
        }

        public static long? Mock_Min(this IOrderedExpressionQuery<long?> source)
        {
            return _min(source);
        }

        public static long Mock_Min(this IOrderedExpressionQuery<long> source)
        {
            return _min(source);
        }
        #endregion

        #region Min/Max
        private static T _max<T>(this IExpressionQuery<T> source)
        {
            source._asResolvable().Max();

            return source.FirstOrDefault();
        }

        private static T _max<T>(this IOrderedExpressionQuery<T> source)
        {
            return _max(source._asExpressionQuery());
        }

        private static T _min<T>(this IExpressionQuery<T> source)
        {
            source._asResolvable().Min();

            return source.FirstOrDefault();
        }

        private static T _min<T>(this IOrderedExpressionQuery<T> source)
        {
            return _min(source._asExpressionQuery());
        }
        #endregion

        #region To List
        public static List<TSource> Mock_ToList<TSource>(this IExpressionQuery<TSource> source)
        {
            List<TSource> result;

            var resolvable = source._asResolvable();

            // get the object
            using (var reader = resolvable.ExecuteReader<TSource>()) result = reader.ToList();

            // disconnect from the server
            resolvable.Disconnect();

            return result;
        }

        public static List<TSource> Mock_ToList<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            return Mock_ToList(source);
        }

        public static List<TSource> Mock_ToList<TSource>(this IOrderedExpressionQuery<TSource> source)
        {
            return Mock_ToList(source._asExpressionQuery());
        }

        public static List<TSource> Mock_ToList<TSource>(this IOrderedExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return Mock_ToList(source._asExpressionQuery(), expression);
        }
        #endregion

        #region Any
        public static bool Mock_Any<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            return Mock_Any(source);
        }

        public static bool Mock_Any<TSource>(this IExpressionQuery<TSource> source)
        {
            bool result;

            var resolvable = source._asResolvable();

            // all we need to do is Select Top 1 1
            resolvable.Any();

            // get the object
            using (var reader = resolvable.ExecuteReader<TSource>()) result = reader.HasRows;

            // disconnect from the server
            resolvable.Disconnect();

            return result;
        }

        public static bool Mock_Any<TSource>(this IOrderedExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            return Mock_Any(source._asExpressionQuery(), expression);
        }

        public static bool Mock_Any<TSource>(this IOrderedExpressionQuery<TSource> source)
        {
            return Mock_Any(source._asExpressionQuery());
        }
        #endregion

        #region Where
        public static IExpressionQuery<TSource> Mock_Where<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source._asResolvable().Where(expression);

            return source;
        }
        #endregion

        #region Include
        public static IExpressionQuery<TSource> Mock_Include<TSource>(this IExpressionQuery<TSource> source, string pathOrTableName)
        {
            source._asResolvable().Include(pathOrTableName);

            return source;
        }

        public static IExpressionQuery<TSource> Mock_IncludeAll<TSource>(this IExpressionQuery<TSource> source)
        {
            source._asResolvable().IncludeAll();

            return source;
        }
        #endregion

        #region Count
        public static int Mock_Count<TSource>(this IExpressionQuery<TSource> source)
        {
            int result;

            var resolvable = source._asResolvable();

            // tell resolvable to do a count
            resolvable.Count();

            // get the object
            using (var reader = resolvable.ExecuteReader<int>()) result = reader.FirstOrDefault();

            // disconnect from the server
            resolvable.Disconnect();

            return result;
        }

        public static int Mock_Count<TSource>(this IExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            // execute sql, and grab data
            return Mock_Count(source);
        }
        #endregion

        #region Order By
        public static IOrderedExpressionQuery<TSource> Mock_OrderBy<TSource, TKey>(this IExpressionQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            var resolvable = source._asResolvable();

            // make sure foreign keys are not selected
            _foreignKeysSelectedCheck(resolvable);

            resolvable.OrderBy(keySelector);

            return source._asOrderedExpressionQuery();
        }

        public static IOrderedExpressionQuery<TSource> Mock_OrderByDescending<TSource, TKey>(this IExpressionQuery<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            var resolvable = source._asResolvable();

            // make sure foreign keys are not selected
            _foreignKeysSelectedCheck(resolvable);

            resolvable.OrderByDescending(keySelector);

            return source._asOrderedExpressionQuery();
        }

        public static IOrderedExpressionQuery<TSource> Mock_ThenBy<TSource, TKey>(this IOrderedExpressionQuery<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            var resolvable = source._asResolvable();

            // make sure foreign keys are not selected
            _foreignKeysSelectedCheck(resolvable);

            resolvable.OrderBy(keySelector);

            return source;
        }

        public static IOrderedExpressionQuery<TSource> Mock_ThenByDescending<TSource, TKey>(this IOrderedExpressionQuery<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            var resolvable = source._asResolvable();

            // make sure foreign keys are not selected
            _foreignKeysSelectedCheck(resolvable);

            resolvable.OrderByDescending(keySelector);

            return source;
        }
        #endregion

        #region Joins
        public static IExpressionQuery<TResult> Mock_InnerJoin<TOuter, TInner, TKey, TResult>(this IExpressionQuery<TOuter> outer,
            IExpressionQuery<TInner> inner,
            Expression<Func<TOuter, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            _foreignKeysJoinCheck(outer._asResolvable());

            return outer._asResolvable().Join(outer, inner, outerKeySelector, innerKeySelector, resultSelector, JoinType.Inner);
        }

        public static IExpressionQuery<TResult> Mock_LeftJoin<TOuter, TInner, TKey, TResult>(this IExpressionQuery<TOuter> outer,
            IExpressionQuery<TInner> inner,
            Expression<Func<TOuter, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            _foreignKeysJoinCheck(outer._asResolvable());

            return outer._asResolvable().Join(outer, inner, outerKeySelector, innerKeySelector, resultSelector, JoinType.Left);
        }
        #endregion

        private static IExpressionQueryResolvable<T> _asResolvable<T>(this IExpressionQuery<T> source)
        {
            return ((IExpressionQueryResolvable<T>)source);
        }

        private static IExpressionQueryResolvable<T> _asResolvable<T>(this IOrderedExpressionQuery<T> source)
        {
            return ((IExpressionQueryResolvable<T>)source);
        }

        private static IExpressionQuery<T> _asExpressionQuery<T>(this IOrderedExpressionQuery<T> source)
        {
            return ((IExpressionQuery<T>)source);
        }

        private static IOrderedExpressionQuery<T> _asOrderedExpressionQuery<T>(this IExpressionQuery<T> source)
        {
            return ((IOrderedExpressionQuery<T>)source);
        }

        private static void _foreignKeysSelectedCheck<T>(IExpressionQueryResolvable<T> source)
        {
            if (source.AreForeignKeysSelected()) throw new OrderByException("Cannot order Expression Query that has foreign keys.  Consider returning the results then ordering.  If Lazy loading do not inculde any foreign keys.");
        }

        private static void _foreignKeysJoinCheck<T>(IExpressionQueryResolvable<T> source)
        {
            if (source.HasForeignKeys()) throw new OrderByException("Cannot join Expression Querys that have foreign keys.");
        }

        #region Select
        public static IExpressionQuery<TResult> Mock_Select<TSource, TResult>(this IExpressionQuery<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            var resolvable = ((IExpressionQueryResolvable<TSource>)source);

            return resolvable.Select(source, selector);
        }

        public static IOrderedExpressionQuery<TResult> Mock_Select<TSource, TResult>(this IOrderedExpressionQuery<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            var resolvable = ((IExpressionQueryResolvable<TSource>)source);

            return resolvable.Select(source, selector);
        }
        #endregion
    }
}
