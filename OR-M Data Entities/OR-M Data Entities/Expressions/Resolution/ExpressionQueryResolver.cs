using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Transform;
using OR_M_Data_Entities.Expressions.Collections;
using OR_M_Data_Entities.Expressions.Resolution.SubQuery;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Expressions.Resolution
{


    public class LambdaToSqlResolution
    {
        public LambdaToSqlResolution(string sql, List<SqlDbParameter> parameters)
        {
            Sql = sql;
            Parameters = parameters;
        }

        public readonly string Sql;

        public readonly IReadOnlyList<SqlDbParameter> Parameters;
    }

    public class ExpressionQueryResolver : ExpressQueryResolverBase
    {
        public static LambdaToSqlResolution Resolve<T>(ExpressionQuery<T> source, Expression<Func<T, bool>> expressionQuery)
        {
            QueryId = source.Id;
            Parameters = new List<SqlDbParameter>();
            Order = new Queue<KeyValuePair<string, Expression>>();
            Tables = source.Tables;
            Sql = string.Empty;
            
            _evaluate(expressionQuery.Body as dynamic);

            return new LambdaToSqlResolution(Sql, Parameters);
        }

        private static void _evaluate(MethodCallExpression expression)
        {
            return;
        }

        private static void _evaluate(UnaryExpression expression)
        {
            _evaluate(expression.Operand as dynamic);
        }

        private static void _evaluate(MemberExpression expression)
        {
            return;
        }

        private static void _evaluate(BinaryExpression expression)
        {
            // decompile the expression and break it into individual expressions
            var expressions = new List<Expression>
            {
                expression
            };
            var finalList = new List<Expression>();

            for (var i = 0; i < expressions.Count; i++)
            {
                var e = expressions[i];

                if (e.NodeType == ExpressionType.Equal ||
                    e.NodeType == ExpressionType.Call ||
                    e.NodeType == ExpressionType.Lambda ||
                    e.NodeType == ExpressionType.Not)
                {
                    finalList.Add(e);
                    continue;
                }

                expressions.Add(((BinaryExpression)e).Left);
                expressions.Add(((BinaryExpression)e).Right);
            }

            Sql = _getSqlFromExpression(expression);

            // loop through the expressions and get the sql
            foreach (dynamic item in finalList)
            {
                // the key for an expression is its string value.  We can just
                // do a ToString on the expression to get what we want.  We just 
                // need to replace that value in the parent to get our sql value
                var replacementString = _getReplaceString(item);

                if (item.NodeType == ExpressionType.Call)
                {
                    Sql = Sql.Replace(replacementString, _getSql(item as MethodCallExpression, false));
                    continue;
                }

                if (item.NodeType == ExpressionType.Equal)
                {
                    // check to see if the user is using (test == true) instead of (test)
                    bool outLeft ;
                    if (isLeftBooleanValue(item, out outLeft))
                    {
                        Sql = Sql.Replace(replacementString, _getSql(item.Right as MethodCallExpression, outLeft));
                        continue;
                    }

                    // check to see if the user is using (test == true) instead of (test)
                    bool outRight;
                    if (isRightBooleanValue(item, out outRight))
                    {
                        Sql = Sql.Replace(replacementString, _getSql(item.Left as MethodCallExpression, outRight));
                        continue;
                    }

                    Sql = Sql.Replace(replacementString, _getSql(item as BinaryExpression));
                    continue;   
                }

                if (item.NodeType == ExpressionType.Not)
                {
                    Sql = Sql.Replace(replacementString, _getSql(item.Operand as MethodCallExpression, true));
                    continue;
                }

                throw new ArgumentNullException(item.NodeType);
            }
        }

        // checks to see if the left side of the binary expression is a boolean value
        private static bool isLeftBooleanValue(BinaryExpression expression, out bool value)
        {
            value = false;
            var left = expression.Left as ConstantExpression;

            if (left != null && left.Value is bool)
            {
                value = !(bool)left.Value; // invert for method that asks whether its a not expression, true = false

                return true;
            }

            return false;
        }

        // checks to see if the right side of the binary expression is a boolean value
        private static bool isRightBooleanValue(BinaryExpression expression, out bool value)
        {
            value = false;
            var right = expression.Right as ConstantExpression;

            if (right != null && right.Value is bool)
            {
                value = !(bool) right.Value; // invert for method that asks whether its a not expression, false = true

                return true;
            }

            return false;
        }

        private static string _getReplaceString(Expression expression)
        {
            return expression.ToString();
        }

        private static string _getSqlFromExpression(Expression expression)
        {
            return string.Format("WHERE {0}", expression.ToString().Replace("OrElse", "\r\n\tOR").Replace("AndAlso", "\r\n\tAND"));
        }

        private static string _getSql(BinaryExpression expression)
        {
            var aliasAndColumnName = _getTableAliasAndColumnName(expression);
            var parameter = string.Format("@DATA{0}", Parameters.Count);

            Parameters.Add(new SqlDbParameter(parameter, GetValue(expression)));

            return string.Format("({0} = {1})", aliasAndColumnName, parameter);
        }

        private static string _getSqlEquality(MethodCallExpression expression, bool isNotExpressionType)
        {
            var aliasAndColumnName = LoadColumnAndTableName(expression as dynamic);
            var value = GetValue(expression as dynamic);
            var comparison = isNotExpressionType ? "!=" : "=";
            var parameter = string.Format("@DATA{0}", Parameters.Count);

            Parameters.Add(new SqlDbParameter(parameter, value));

            return string.Format("({0} {1} {2})", aliasAndColumnName, comparison, parameter);
        }

        private static string _getSqlContains(MethodCallExpression expression, bool isNotExpressionType)
        {
            var aliasAndColumnName = LoadColumnAndTableName(expression as dynamic);
            var value = GetValue(expression);
            var isEnumerable = IsEnumerable(value.GetType());
            var comparison = isEnumerable
                ? (isNotExpressionType ? "NOT IN({0})" : "IN({0})")
                : (isNotExpressionType ? "LIKE" : "NOT LIKE");

            if (!isEnumerable)
            {
                var containsParameter = string.Format("@DATA{0}", Parameters.Count);

                Parameters.Add(new SqlDbParameter(containsParameter, string.Format("%{0}%", value)));
                return string.Format("{0} {1} {2}", aliasAndColumnName, comparison, containsParameter);
            }

            var inString = string.Empty;

            foreach (var item in ((ICollection)value))
            {
                var inParameter = string.Format("@DATA{0}", Parameters.Count);
                Parameters.Add(new SqlDbParameter(inParameter, item));

                inString = string.Concat(inString, string.Format("{0},", inParameter));
            }

            return string.Format("({0} {1})", aliasAndColumnName, string.Format(comparison, inString.TrimEnd(',')));
        }

        private static string _getSql(MethodCallExpression expression, bool isNotExpressionType)
        {
            var isConverting =
                expression.Arguments.Any(
                    w =>
                        w is MethodCallExpression &&
                        ((MethodCallExpression)w).Method.DeclaringType == typeof(DbTransform) &&
                        ((MethodCallExpression)w).Method.Name == "Convert");

            switch (expression.Method.Name.ToUpper())
            {
                case "EQUALS":
                case "OP_EQUALITY":
                    return _getSqlEquality(expression, isNotExpressionType);
                case "CONTAINS":
                    return _getSqlContains(expression, isNotExpressionType);
                case "STARTSWITH":
                    //result.Comparison = invertComparison ? CompareType.NotLike : CompareType.Like;
                    //result.CompareValue = resolution.GetAndAddParameter(string.Format("{0}%", value));
                    break;
                case "ENDSWITH":
                    //result.Comparison = invertComparison ? CompareType.NotLike : CompareType.Like;
                    //result.CompareValue = resolution.GetAndAddParameter(string.Format("%{0}", value));
                    break;


                case "GREATERTHAN":
                    //result.Comparison = CompareType.GreaterThan;
                    //result.InvertComparison = invertComparison;
                    //result.CompareValue = resolution.GetAndAddParameter(value);
                    break;
                case "GREATERTHANOREQUAL":
                    //result.Comparison = CompareType.GreaterThanEquals;
                    //result.InvertComparison = invertComparison;
                    //result.CompareValue = resolution.GetAndAddParameter(value);
                    break;
                case "LESSTHAN":
                    //result.Comparison = CompareType.LessThan;
                    //result.InvertComparison = invertComparison;
                    //result.CompareValue = resolution.GetAndAddParameter(value);
                    break;
                case "LESSTHANOREQUAL":
                    //result.Comparison = CompareType.LessThanEquals;
                    //result.InvertComparison = invertComparison;
                    //result.CompareValue = resolution.GetAndAddParameter(value);
                    break;
            }

            throw new ArgumentNullException();
        }

        private static string _getTableAliasAndColumnName(BinaryExpression expression)
        {
            if (expression.Right.NodeType == ExpressionType.Constant)
            {
                return LoadColumnAndTableName(expression.Left as dynamic);
            }

            return LoadColumnAndTableName(expression.Right as dynamic);
        }
    }

    public abstract class ExpressQueryResolverBase
    {
        public static string Sql { get; protected set; }

        protected static List<SqlDbParameter> Parameters { get; set; }

        protected static Queue<KeyValuePair<string, Expression>> Order;

        protected static ReadOnlyTableCollection Tables { get; set; }

        protected static Guid QueryId { get; set; }

        protected static bool IsSubQuery(MethodCallExpression expression)
        {
            return
                expression.Arguments.OfType<MethodCallExpression>()
                    .Select(
                        methodCallExpression =>
                            methodCallExpression.IsExpressionQuery() || IsSubQuery(methodCallExpression))
                    .FirstOrDefault();
        }

        #region Get Value
        protected static object GetValue(ConstantExpression expression)
        {
            return expression.Value ?? "IS NULL";
        }

        protected static object GetValue(MemberExpression expression)
        {
            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            var value = getter();

            return value ?? "IS NULL";
        }

        protected static object GetValue(MethodCallExpression expression)
        {
            var methodName = expression.Method.Name.ToUpper();

            if (IsSubQuery(expression))
            {
                return SubQueryResolver.Resolve(expression, null);
            }

            if (methodName.Equals("CONTAINS"))
            {
                if (expression.Object != null)
                {
                    var type = expression.Object.Type;

                    if (IsEnumerable(type)) return _compile(expression.Object);
                }

                // search in arguments for the Ienumerable
                foreach (var argument in expression.Arguments)
                {
                    if (IsEnumerable(argument.Type)) return _compile(argument);
                }

                throw new ArgumentException("Comparison value not found");
            }

            if (methodName.Equals("EQUALS"))
            {
                // need to look for the argument that has the constant
                foreach (var argument in expression.Arguments)
                {
                    var contstant = argument as ConstantExpression;

                    if (contstant != null)
                    {
                        return contstant.Value;
                    }

                    var memberExpression = argument as MemberExpression;

                    if (memberExpression == null) continue;

                    contstant = memberExpression.Expression as ConstantExpression;

                    if (contstant != null) return contstant.Value;
                }

                throw new ArgumentException("Comparison value not found");
            }

            return _compile(expression.Object as MethodCallExpression);
        }

        private static object _compile(Expression expression)
        {
            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            var value = getter();

            return value ?? "IS NULL";
        }

        protected static object GetValue(UnaryExpression expression)
        {
            return GetValue(expression.Operand as dynamic);
        }

        protected static object GetValue(BinaryExpression expression)
        {
            return expression.Right.NodeType == ExpressionType.Constant
                ? ((ConstantExpression)expression.Right).Value
                : ((ConstantExpression)expression.Left).Value;
        }
        #endregion

        #region Has Parameter
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

        protected static bool HasParameter(MethodCallExpression expression)
        {
            var e = expression.Object;

            return e != null
                ? HasParameter(expression.Object as dynamic)
                : expression.Arguments.Select(arg => HasParameter(arg as dynamic)).Any(hasParameter => hasParameter);
        }
        #endregion

        #region Load Table And Column Name
        protected static string LoadColumnAndTableName(MemberExpression expression)
        {
            var columnAttribute = expression.Member.GetCustomAttribute<ColumnAttribute>();
            var columnName = columnAttribute != null ? columnAttribute.Name : expression.Member.Name;
            string alias;

            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                // cannot come from a foriegn key, is the base type
                //result.ColumnName = columnAttribute != null ? columnAttribute.Name : expression.Member.Name;
                //result.TableName = expression.Expression.Type.GetTableName();
                alias = Tables.Find(expression.Expression.Type, QueryId).Alias;
                //tableName = expression.Expression.Type.GetTableName();
            }
            else
            {
                // will be from a foreign key, not the base type
                //result.ColumnName = columnAttribute != null ? columnAttribute.Name : expression.Member.Name;
                //result.TableName = ((MemberExpression)expression.Expression).Member.Name;
                alias = Tables.FindByPropertyName(((MemberExpression)expression.Expression).Member.Name, QueryId).Alias;
                //tableName = ((MemberExpression)expression.Expression).Member.Name;
            }

            return string.Format("[{0}].[{1}]", alias, columnName);
        }

        protected static string LoadColumnAndTableName(MethodCallExpression expression)
        {
            if (expression.Object == null)
            {
                return LoadColumnAndTableName(expression.Arguments.First(w => HasParameter(w as dynamic)) as dynamic);
            }

            if (IsEnumerable(expression.Object.Type))
            {
                foreach (var memberExpression in expression.Arguments.OfType<MemberExpression>())
                {
                    return LoadColumnAndTableName(memberExpression);
                }
            }

            return LoadColumnAndTableName(expression.Object as MemberExpression);
        }

        protected static string LoadColumnAndTableName(UnaryExpression expression)
        {
            return LoadColumnAndTableName(expression.Operand as MemberExpression);
        }

        protected static bool IsEnumerable(Type type)
        {
            return (type.IsGenericType &&
                   type.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)) ||
                   type.IsArray);
        }
        #endregion
    }
}
