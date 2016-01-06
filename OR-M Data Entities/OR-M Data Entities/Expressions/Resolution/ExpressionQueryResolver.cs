using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
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
            return Resolve(source, new List<SqlDbParameter>(), expressionQuery);
        }

        public static LambdaToSqlResolution Resolve<T>(ExpressionQuery<T> source, List<SqlDbParameter> parameters, Expression<Func<T, bool>> expressionQuery)
        {
            return Resolve(source.Id, source.Tables, parameters, expressionQuery.Body);
        }

        public static LambdaToSqlResolution Resolve(Guid queryId, ReadOnlyTableCollection tables, List<SqlDbParameter> parameters, Expression expressionQuery)
        {
            QueryId = queryId;
            Parameters = new List<SqlDbParameter>();
            Order = new Queue<KeyValuePair<string, Expression>>();
            Tables = tables;
            Sql = string.Empty;

            _evaluate(expressionQuery as dynamic);

            return new LambdaToSqlResolution(Sql, Parameters);
        }

        private static void _evaluate(MethodCallExpression expression)
        {
            // get the sql from the expression
            Sql = WhereUtilities.GetSqlFromExpression(expression);

            _processExpression(expression, expression.ToString(), false);
        }

        private static void _evaluate(UnaryExpression expression)
        {
            _evaluate(expression.Operand as dynamic);
        }

        private static void _evaluate(MemberExpression expression)
        {
            // get the sql from the expression
            Sql = WhereUtilities.GetSqlFromExpression(expression);

            _processExpression(expression, expression.ToString(), false);
        }

        private static void _evaluate(BinaryExpression expression)
        {
            // get the sql from the expression
            Sql = WhereUtilities.GetSqlFromExpression(expression);

            // decompile the expression and break it into individual expressions
            var expressions = new List<Expression>
            {
                expression
            };

            for (var i = 0; i < expressions.Count; i++)
            {
                var e = expressions[i];

                if (WhereUtilities.IsFinalExpressionNodeType(e.NodeType))
                {
                    // the key for an expression is its string value.  We can just
                    // do a ToString on the expression to get what we want.  We just 
                    // need to replace that value in the parent to get our sql value
                    var replacementString = WhereUtilities.GetReplaceString(e);

                    // process the not nodes like this because the actual
                    // expression is embedded inside of it
                    if (e.NodeType == ExpressionType.Not)
                    {
                        _processNotExpression(e, replacementString);
                        continue;
                    }

                    // process normal expression
                    _processExpression(e, replacementString, false);
                    continue;
                }

                expressions.Add(((BinaryExpression)e).Left);
                expressions.Add(((BinaryExpression)e).Right);
            }
        }

        private static void _processNotExpression(dynamic item, string replacementString)
        {
            var unaryExpression = item as UnaryExpression;

            if (unaryExpression != null)
            {
                _processExpression(unaryExpression.Operand, replacementString, true);
                return;
            }

            var binaryExpression = item as BinaryExpression;

            if (binaryExpression != null)
            {
                _processExpression(binaryExpression, replacementString, true);
                return;
            }

            throw new Exception(string.Format("Expression Type not valid.  Type: {0}", item.NodeType));
        }

        private static void _processExpression(dynamic item, string replacementString, bool isNotExpressionType)
        {
            if (item.NodeType == ExpressionType.Call)
            {
                Sql = Sql.Replace(replacementString, _getSql(item as MethodCallExpression, isNotExpressionType));
                return;
            }

            if (item.NodeType == ExpressionType.Equal)
            {
                // check to see if the user is using (test == true) instead of (test)
                bool outLeft;
                if (WhereUtilities.IsLeftBooleanValue(item, out outLeft))
                {
                    Sql = Sql.Replace(replacementString, _getSql(item.Right as MethodCallExpression, outLeft || isNotExpressionType));
                    return;
                }

                // check to see if the user is using (test == true) instead of (test)
                bool outRight;
                if (WhereUtilities.IsRightBooleanValue(item, out outRight))
                {
                    Sql = Sql.Replace(replacementString, _getSql(item.Left as MethodCallExpression, outRight || isNotExpressionType));
                    return;
                }

                Sql = Sql.Replace(replacementString, _getSqlEquals(item as BinaryExpression, isNotExpressionType));
                return;
            }

            if (item.NodeType == ExpressionType.GreaterThan ||
                item.NodeType == ExpressionType.GreaterThanOrEqual ||
                item.NodeType == ExpressionType.LessThan ||
                item.NodeType == ExpressionType.LessThanOrEqual)
            {
                // check to see the order the user typed the statement
                if (WhereUtilities.IsType<ConstantExpression>(item.Left))
                {
                    Sql = Sql.Replace(replacementString, _getSqlGreaterThanLessThan(item as BinaryExpression, isNotExpressionType, item.NodeType));
                    return;
                }

                // check to see the order the user typed the statement
                if (WhereUtilities.IsType<ConstantExpression>(item.Right))
                {
                    Sql = Sql.Replace(replacementString, _getSqlGreaterThanLessThan(item as BinaryExpression, isNotExpressionType, item.NodeType));
                    return;
                }

                throw new Exception("invalid comparison");
            }

            // double negative check will go into recursive check
            if (item.NodeType == ExpressionType.Not)
            {
                _processNotExpression(item, replacementString);
                return;
            }

            throw new ArgumentNullException(item.NodeType);
        }

        private static string _getSqlEquals(BinaryExpression expression, bool isNotExpressionType)
        {
            var comparison = isNotExpressionType ? "!=" : "=";
            var left = string.Empty;
            var right = string.Empty;

            // check to see if the left and right are not constants, if so they need to be evaulated
            if (WhereUtilities.IsType<ConstantExpression>(expression.Left) || WhereUtilities.IsType<ConstantExpression>(expression.Right))
            {
                left = _getTableAliasAndColumnName(expression);
                right = string.Format("@DATA{0}", Parameters.Count);

                Parameters.Add(new SqlDbParameter(right, GetValue(expression)));

                return string.Format("({0} {1} {2})", left, comparison, right);
            }

            var isLeftLambdaMethod = WhereUtilities.IsLambdaMethod(expression.Left as dynamic);
            var isRightLambdaMethod = WhereUtilities.IsLambdaMethod(expression.Right as dynamic);

            if (isLeftLambdaMethod)
            {
                // first = Select Top 1 X From Y Where X
                left = _getSqlFromLambdaMethod(expression.Left as dynamic);
            }

            if (isRightLambdaMethod)
            {
                right = _getSqlFromLambdaMethod(expression.Right as dynamic);
            }

            if (!isLeftLambdaMethod && !isRightLambdaMethod)
            {
                right = LoadColumnAndTableName(expression.Right as dynamic);
                left = LoadColumnAndTableName(expression.Left as dynamic);
            }

            return string.Format("({0} {1} {2})", left, comparison, right);
        }

        private static string _getSqlFromLambdaMethod(MemberExpression expression)
        {
            var methodCallExpression = expression.Expression as MethodCallExpression;

            if (methodCallExpression == null)
            {
                throw new InvalidExpressionException("Expected MethodCallExpression");
            }

            var methodName = methodCallExpression.Method.Name;

            switch (methodName.ToUpper())
            {
                case "FIRST":
                case "FIRSTORDEFAULT":
                    var c = Resolve(QueryId, Tables, Parameters, methodCallExpression);
                    return string.Format("SELECT {0} FROM {1} WHERE {2}","", "", c.Sql);
            }

            throw new Exception(string.Format("Lambda Method not recognized.  Method Name: {0}", methodName));
        }

        private static string _getSqlGreaterThanLessThan(BinaryExpression expression, bool isNotExpressionType, ExpressionType comparisonType)
        {
            var aliasAndColumnName = _getTableAliasAndColumnName(expression);
            var parameter = string.Format("@DATA{0}", Parameters.Count);
            string comparison;

            switch (comparisonType)
            {
                case ExpressionType.GreaterThan:
                    comparison = IsValueOnRight(expression) ? ">" : "<";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    comparison = IsValueOnRight(expression) ? ">=" : "<=";
                    break;
                case ExpressionType.LessThan:
                    comparison = IsValueOnRight(expression) ? "<" : ">";
                    break;
                case ExpressionType.LessThanOrEqual:
                    comparison = IsValueOnRight(expression) ? "<=" : ">=";
                    break;
                default:
                    throw new Exception(string.Format("Comparison not valid.  Comparison Type: {0}", comparisonType));
            }

            Parameters.Add(new SqlDbParameter(parameter, GetValue(expression)));
            var result = string.Format("({0} {1} {2})", aliasAndColumnName, comparison, parameter);

            return isNotExpressionType ? string.Format("(NOT{0})", result) : result;
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

        private static string _getSqlStartsEndsWith(MethodCallExpression expression, bool isNotExpressionType, bool isStartsWith)
        {
            var aliasAndColumnName = LoadColumnAndTableName(expression as dynamic);
            var value = GetValue(expression as dynamic);
            var comparison = isNotExpressionType ? "NOT LIKE" : "LIKE";
            var parameter = string.Format("@DATA{0}", Parameters.Count);

            Parameters.Add(new SqlDbParameter(parameter, string.Format(isStartsWith ? "{0}%" : "%{0}", value)));

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
            //var isConverting =
            //    expression.Arguments.Any(
            //        w =>
            //            w is MethodCallExpression &&
            //            ((MethodCallExpression)w).Method.DeclaringType == typeof(DbTransform) &&
            //            ((MethodCallExpression)w).Method.Name == "Convert");
            var methodName = expression.Method.Name;

            switch (methodName.ToUpper())
            {
                case "EQUALS":
                case "OP_EQUALITY":
                    return _getSqlEquality(expression, isNotExpressionType);
                case "CONTAINS":
                    return _getSqlContains(expression, isNotExpressionType);
                case "STARTSWITH":
                    return _getSqlStartsEndsWith(expression, isNotExpressionType, true);
                case "ENDSWITH":
                    return _getSqlStartsEndsWith(expression, isNotExpressionType, false);
            }

            throw new Exception(string.Format("Method does not translate into Sql.  Method Name: {0}", methodName));
        }

        private static string _getTableAliasAndColumnName(BinaryExpression expression)
        {
            if (expression.Right.NodeType == ExpressionType.Constant)
            {
                return LoadColumnAndTableName(expression.Left as dynamic);
            }

            return LoadColumnAndTableName(expression.Right as dynamic);
        }

        private static class WhereUtilities
        {
            public static bool IsLambdaMethod(Expression expression)
            {
                var e = expression as MemberExpression;

                if (e == null) return false;

                var methodCallExpression = e.Expression as MethodCallExpression;

                return methodCallExpression != null && methodCallExpression.ToString().Contains("=>");
            }

            public static string GetSqlFromExpression(Expression expression)
            {
                return string.Format("WHERE {0}", expression.ToString().Replace("OrElse", "\r\n\tOR").Replace("AndAlso", "\r\n\tAND"));
            }

            // checks to see if the left side of the binary expression is a boolean value
            public static bool IsLeftBooleanValue(BinaryExpression expression, out bool value)
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
            public static bool IsRightBooleanValue(BinaryExpression expression, out bool value)
            {
                value = false;
                var right = expression.Right as ConstantExpression;

                if (right != null && right.Value is bool)
                {
                    value = !(bool)right.Value; // invert for method that asks whether its a not expression, false = true

                    return true;
                }

                return false;
            }

            public static bool IsType<T>(Expression expression)
            {
                return expression is T;
            }

            public static string GetReplaceString(Expression expression)
            {
                return expression.ToString();
            }

            public static bool IsFinalExpressionNodeType(ExpressionType expressionType)
            {
                var finalExpressionTypes = new[]
                {
                    ExpressionType.Equal, ExpressionType.Call, ExpressionType.Lambda, ExpressionType.Not,
                    ExpressionType.GreaterThan, ExpressionType.GreaterThanOrEqual, ExpressionType.LessThan,
                    ExpressionType.LessThanOrEqual
                };

                return finalExpressionTypes.Contains(expressionType);
            }
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

            if (methodName.Equals("EQUALS") || methodName.Equals("STARTSWITH") || methodName.Equals("ENDSWITH"))
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

        protected static bool IsValueOnRight(BinaryExpression expression)
        {
            return expression.Right is ConstantExpression;
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
