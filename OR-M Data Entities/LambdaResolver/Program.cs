using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions;
using OR_M_Data_Entities.Tests.Tables;

namespace LambdaResolver
{
    class Program
    {
        static void Main(string[] args)
        {
            var lst = new List<int> { 1, 2, 3, 4, 5 };
            var item =
                new ExpressionQuery<Contact>().Where(
                    w => w.ID == 1 && w.FirstName == "James" || w.FirstName == "Megan" && w.FirstName == "WIN" && w.FirstName == "AHHHH" || w.FirstName == "");
        }
    }

    public class ExpressionQuery<T> : IEnumerable
    {
        public ExpressionQuery()
        {
        }

        public IEnumerator GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }

    public enum CompareType
    {
        None,
        Like,
        BeginsWith,
        EndsWith,
        NotLike,
        NotBeginsWith,
        NotEndsWith,
        Equals,
        EqualsIgnoreCase,
        EqualsTruncateTime,
        GreaterThan,
        GreaterThanEquals,
        LessThan,
        LessThanEquals,
        NotEqual,
        Between,
        In,
        NotIn
    }

    public class SqlDbParameter
    {
        public string Name { get; set; }

        public object Value { get; set; }
    }

    public class SqlResolutionContainer
    {
        public SqlResolutionContainer()
        {
            _resolutions = new List<LambdaResolution>();
            _parameters = new List<SqlDbParameter>();
        }

        private readonly List<LambdaResolution> _resolutions;
        public IEnumerable<LambdaResolution> Resolutions { get { return _resolutions; } }

        private readonly List<SqlDbParameter> _parameters;
        public IEnumerable<SqlDbParameter> Parameters { get { return _parameters; } }

        public string AddParameter(object value)
        {
            var parameter = string.Format("@Param{0}", _parameters.Count);

            _parameters.Add(new SqlDbParameter
            {
                Name = parameter,
                Value = value
            });

            return parameter;
        }

        public void AddResolution(LambdaResolution resolution)
        {
            _resolutions.Add(resolution);
        }

        public int NextGroupNumber()
        {
            return _resolutions.Count == 0 ? 0 : _resolutions.Select(w => w.Group).Max() + 1;
        }
    }

    public static class ListExtensions
    {
        public static bool IsList(this object o)
        {
            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public static bool IsList(this Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }
    }

    public class LambdaResolution
    {
        public LambdaResolution()
        {
            TableName = string.Empty;
            ColumnName = string.Empty;
            Comparison = CompareType.None;
            Group = -1;
        }

        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public CompareType Comparison { get; set; }

        public SqlConnector Connector { get; set; }

        public object CompareValue { get; set; }

        public int Group { get; set; }
    }

    public enum SqlConnector
    {
        And,
        Or
    }

    public class WhereExpressionResolver
    {
        public static string Resolve<T>(Expression<Func<T, bool>> expression)
        {
            var resolution = new SqlResolutionContainer();

            _evaluateTree(expression.Body as BinaryExpression, resolution);

            return "";
        }

        // need to evaluate groupings
        public static void _evaluateTree(BinaryExpression expression, SqlResolutionContainer resolution)
        {
            while (true)
            {
                if (HasComparison(expression))
                {
                    // if the right has a left then its a group expression

                    if (HasComparison(expression.Right))
                    {

                        continue;
                    }

                    expression = expression.Left as BinaryExpression;
                    continue;
                }

                var result = _evaluate(expression as dynamic, resolution);
                break;
            }
        }

        public static SqlConnector GetConnector(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return SqlConnector.Or;

                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return SqlConnector.And;

                default:
                    return SqlConnector.And;
            }
        }

        #region Evaluate
        /// <summary>
        /// Happens when the method uses an operator
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="resolution"></param>
        protected static LambdaResolution _evaluate(BinaryExpression expression, SqlResolutionContainer resolution, int group = -1)
        {
            var result = new LambdaResolution
            {
                Group = group,
            };
            var isParameterOnLeftSide = HasParameter(expression.Left as dynamic);

            // load the table and column name into the result
            LoadColumnAndTableName((isParameterOnLeftSide ? expression.Left : expression.Right) as dynamic, result);

            // get the value from the expression
            LoadValue((isParameterOnLeftSide ? expression.Right : expression.Left) as dynamic, result);

            // get the comparison tyoe
            LoadComparisonType(expression, result);

            // add the result to the list
            return result;
        }

        /// <summary>
        /// Happens when the method uses a method to compare values, IE: Contains, Equals
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="resolution"></param>
        protected static LambdaResolution _evaluate(MethodCallExpression expression, SqlResolutionContainer resolution, int group = -1)
        {
            var result = new LambdaResolution
            {
                Group = group
            };
            var isParameterOnLeftSide = HasParameter(expression.Object as dynamic);

            if (isParameterOnLeftSide)
            {
                LoadColumnAndTableName(expression.Object as dynamic, result);

                LoadValue(expression.Arguments[0] as dynamic, result);
            }
            else
            {
                LoadColumnAndTableName(expression.Arguments[0] as dynamic, result);

                LoadValue(expression.Object as dynamic, result);
            }

            //stringifiedList = list.Cast<object>().Aggregate(stringifiedList, (current, item) => current + (resolution.AddParameter(item) + ",")).TrimEnd(',');

            switch (expression.Method.Name.ToUpper())
            {
                case "EQUALS":
                case "OP_EQUALITY":
                    result.Comparison = CompareType.Equals;
                    break;
                case "CONTAINS":
                    result.Comparison = result.CompareValue.IsList() ? CompareType.In : CompareType.Like;
                    break;
            }

            return result;
        }
        #endregion

        #region Load Comparison Type
        protected static void LoadComparisonType(BinaryExpression expression, LambdaResolution resolution)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    resolution.Comparison = CompareType.Equals;
                    break;
                case ExpressionType.GreaterThan:
                    resolution.Comparison = CompareType.GreaterThan;
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    resolution.Comparison = CompareType.GreaterThanEquals;
                    break;
                case ExpressionType.LessThanOrEqual:
                    resolution.Comparison = CompareType.LessThanEquals;
                    break;
                case ExpressionType.LessThan:
                    resolution.Comparison = CompareType.LessThan;
                    break;
                case ExpressionType.NotEqual:
                    resolution.Comparison = CompareType.NotEqual;
                    break;
            }
        }
        #endregion

        #region Load Table And Column Name
        protected static void LoadColumnAndTableName(MemberExpression expression, LambdaResolution result)
        {
            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                // cannot come from a foriegn key, is the base type
                result.ColumnName = expression.Member.Name;
                result.TableName = expression.Expression.Type.Name;
            }
            else
            {
                // will be from a foreign key, not the base type
                result.ColumnName = expression.Member.Name;
                result.TableName = ((MemberExpression)expression.Expression).Member.Name;
            }
        }

        protected static void LoadColumnAndTableName(MethodCallExpression expression, LambdaResolution result)
        {
            LoadColumnAndTableName(expression.Object as MemberExpression, result);
        }
        #endregion

        #region Load Value
        protected static void LoadValue(ConstantExpression expression, LambdaResolution result)
        {
            result.CompareValue = expression.Value;
        }

        protected static void LoadValue(MemberExpression expression, LambdaResolution result)
        {
            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            result.CompareValue = getter();
        }

        protected static void LoadValue(MethodCallExpression expression, LambdaResolution result)
        {
            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            result.CompareValue = getter();
        }

        protected static void LoadValue(UnaryExpression expression, LambdaResolution result)
        {
            LoadValue(expression.Operand as dynamic, result);
        }
        #endregion

        protected static bool HasComparison(Expression expression)
        {
            return expression.NodeType == ExpressionType.And
                || expression.NodeType == ExpressionType.AndAlso
                || expression.NodeType == ExpressionType.Or
                || expression.NodeType == ExpressionType.OrElse;
        }

        protected static bool HasParameter(MethodCallExpression expression)
        {
            var e = expression.Object;

            return e != null
                ? HasParameter(expression.Object as dynamic)
                : expression.Arguments.Select(arg => HasParameter(arg as dynamic)).Any(hasParameter => hasParameter);
        }

        protected static bool HasParameter(ConstantExpression expression)
        {
            return false;
        }

        protected static bool HasParameter(UnaryExpression expression)
        {
            return expression == null ? false : HasParameter(expression.Operand as dynamic);
        }

        protected static bool HasParameter(ParameterExpression expression)
        {
            return true;
        }

        protected static bool HasParameter(MemberExpression expression)
        {
            return HasParameter(expression.Expression as dynamic);
        }
    }

    public static class ExpressionQueryExtensions
    {
        public static TSource First<TSource>(this ExpressionQuery<TSource> source)
        {
            return default(TSource);
        }

        public static TSource First<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            return default(TSource);
        }

        public static TSource FirstOrDefault<TSource>(this ExpressionQuery<TSource> source)
        {
            return default(TSource);
        }

        public static TSource FirstOrDefault<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);


            return default(TSource);
        }

        public static ExpressionQuery<TSource> Where<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            var sql = WhereExpressionResolver.Resolve(expression);

            return source;
        }

        public static ExpressionQuery<TResult> InnerJoin<TOuter, TInner, TKey, TResult>(this ExpressionQuery<TOuter> outer,
            ExpressionQuery<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            var result = new ExpressionQuery<TResult>();

            //SqlExpressionSelectResolver.Resolve(selector, result.Query);

            return result;
        }

        public static ExpressionQuery<TResult> Select<TSource, TResult>(this ExpressionQuery<TSource> source,
            Expression<Func<TSource, int, TResult>> selector)
        {
            var result = new ExpressionQuery<TResult>();

            //SqlExpressionSelectResolver.Resolve(selector, result.Query);

            return result;
        }

        public static ExpressionQuery<TResult> Select<TSource, TResult>(this ExpressionQuery<TSource> source,
            Expression<Func<TSource, TResult>> selector)
        {
            var result = new ExpressionQuery<TResult>();

            return result;
        }
    }
}
