/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions.Collections;
using OR_M_Data_Entities.Expressions.Resolution.SubQuery;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Expressions.Resolution
{
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

        public static LambdaToSqlResolution Resolve(Guid queryId, ReadOnlyTableCollection tables, List<SqlDbParameter> parameters, Expression expressionQuery, bool isSubQuery = false)
        {
            QueryId = queryId;
            Parameters = new List<SqlDbParameter>();
            Order = new Queue<KeyValuePair<string, Expression>>();
            Tables = tables;
            Sql = string.Empty;
            IsSubQuery = isSubQuery;

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
                if (WhereUtilities.IsConstant(item.Left))
                {
                    Sql = Sql.Replace(replacementString, _getSqlGreaterThanLessThan(item as BinaryExpression, isNotExpressionType, item.NodeType));
                    return;
                }

                // check to see the order the user typed the statement
                if (WhereUtilities.IsConstant(item.Right))
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
            if (WhereUtilities.IsConstant(expression.Left) || WhereUtilities.IsConstant(expression.Right))
            {
                left = _getTableAliasAndColumnName(expression).GetTableAndColumnName(IsSubQuery);
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
                right = LoadColumnAndTableName(expression.Right as dynamic).GetTableAndColumnName(IsSubQuery);
            }

            if (isRightLambdaMethod)
            {
                right = _getSqlFromLambdaMethod(expression.Right as dynamic);
                left = LoadColumnAndTableName(expression.Left as dynamic).GetTableAndColumnName(IsSubQuery);
            }

            if (!isLeftLambdaMethod && !isRightLambdaMethod)
            {
                right = LoadColumnAndTableName(expression.Right as dynamic).GetTableAndColumnName(IsSubQuery);
                left = LoadColumnAndTableName(expression.Left as dynamic).GetTableAndColumnName(IsSubQuery);
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

                    dynamic lambdaExpression =
                        methodCallExpression.Arguments.FirstOrDefault(w => w.ToString().Contains("=>"));
                    
                    if (lambdaExpression == null)
                    {
                        throw new Exception("Lambda subquery expression not found");
                    }

                    var columnName = expression.Member.GetColumnName();
                    var tableName = WhereUtilities.GetTableNameFromLambdaParameter(lambdaExpression.Body);

                    var c = Resolve(QueryId, Tables, Parameters, lambdaExpression.Body, true);
                    return string.Format("(SELECT TOP 1 {0} FROM {1} {2}", string.Format("[{0}].[{1}])", tableName, columnName), tableName, c.Sql);
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
            var result = string.Format("({0} {1} {2})", aliasAndColumnName.GetTableAndColumnName(IsSubQuery), comparison, parameter);

            return isNotExpressionType ? string.Format("(NOT{0})", result) : result;
        }

        private static string _getSqlEquality(MethodCallExpression expression, bool isNotExpressionType)
        {
            var aliasAndColumnName = LoadColumnAndTableName(expression as dynamic);
            var value = GetValue(expression as dynamic);
            var comparison = isNotExpressionType ? "!=" : "=";
            var parameter = string.Format("@DATA{0}", Parameters.Count);

            Parameters.Add(new SqlDbParameter(parameter, value));

            return string.Format("({0} {1} {2})", aliasAndColumnName.GetTableAndColumnName(IsSubQuery), comparison, parameter);
        }

        private static string _getSqlStartsEndsWith(MethodCallExpression expression, bool isNotExpressionType, bool isStartsWith)
        {
            var aliasAndColumnName = LoadColumnAndTableName(expression as dynamic);
            var value = GetValue(expression as dynamic);
            var comparison = isNotExpressionType ? "NOT LIKE" : "LIKE";
            var parameter = string.Format("@DATA{0}", Parameters.Count);

            Parameters.Add(new SqlDbParameter(parameter, string.Format(isStartsWith ? "{0}%" : "%{0}", value)));

            return string.Format("({0} {1} {2})", aliasAndColumnName.GetTableAndColumnName(IsSubQuery), comparison, parameter);
        }

        private static string _getSqlContains(MethodCallExpression expression, bool isNotExpressionType)
        {
            var aliasAndColumnName = LoadColumnAndTableName(expression as dynamic);
            var value = GetValue(expression);
            var isEnumerable = IsEnumerable(value.GetType());
            var comparison = isEnumerable
                ? (isNotExpressionType ? "NOT IN ({0})" : "IN ({0})")
                : (isNotExpressionType ? "LIKE" : "NOT LIKE");

            if (!isEnumerable)
            {
                var containsParameter = string.Format("@DATA{0}", Parameters.Count);

                Parameters.Add(new SqlDbParameter(containsParameter, string.Format("%{0}%", value)));
                return string.Format("{0} {1} {2}", aliasAndColumnName.GetTableAndColumnName(IsSubQuery), comparison, containsParameter);
            }

            var inString = string.Empty;

            foreach (var item in ((ICollection)value))
            {
                var inParameter = string.Format("@DATA{0}", Parameters.Count);
                Parameters.Add(new SqlDbParameter(inParameter, item));

                inString = string.Concat(inString, string.Format("{0},", inParameter));
            }

            return string.Format("({0} {1})", aliasAndColumnName.GetTableAndColumnName(IsSubQuery), string.Format(comparison, inString.TrimEnd(',')));
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

        private static TableColumnContainer _getTableAliasAndColumnName(BinaryExpression expression)
        {
            if (WhereUtilities.IsConstant(expression.Right))
            {
                return LoadColumnAndTableName(expression.Left as dynamic);
            }

            return LoadColumnAndTableName(expression.Right as dynamic);
        }
    }

    public class ExpressionQuerySelectResolver : ExpressQueryResolverBase
    {
        public static object ReturnObject { get; private set; }

        public static LambdaToSqlResolution Resolve<TSource, TResult>(ExpressionQuery<TSource> source, Expression<Func<TSource, TResult>> expressionQuery)
        {
            return Resolve(source, new List<SqlDbParameter>(), expressionQuery);
        }

        public static LambdaToSqlResolution Resolve<TSource, TResult>(ExpressionQuery<TSource> source, List<SqlDbParameter> parameters, Expression<Func<TSource, TResult>> expressionQuery)
        {
            return Resolve(source.Id, source.Tables, parameters, expressionQuery.Body);
        }

        public static LambdaToSqlResolution Resolve(Guid queryId, ReadOnlyTableCollection tables, List<SqlDbParameter> parameters, Expression expressionQuery, bool isSubQuery = false)
        {
            QueryId = queryId;
            Parameters = null;
            Order = new Queue<KeyValuePair<string, Expression>>();
            Tables = tables;
            Sql = SelectUtilities.GetSqlFromExpression(expressionQuery);
            IsSubQuery = isSubQuery;

            _evaluate(expressionQuery as dynamic);

            return new LambdaToSqlResolution(Sql, Parameters);
        }

        private static void _evaluate(MemberInitExpression expression)
        {
            var sql = _getSql(expression);

            Sql = Sql.Replace(expression.ToString(), sql);

            ReturnObject = expression;
        }

        private static void _evaluate(NewExpression expression)
        {
            var sql = string.Empty;

            for (var i = 0; i < expression.Arguments.Count; i++)
            {
                var argument = expression.Arguments[i];
                var member = expression.Members[i];

                var tableAndColumnName = LoadColumnAndTableName((dynamic)argument);
                var tableAndColmnNameSql = SelectUtilities.GetTableAndColumnNameWithAlias(tableAndColumnName, member.Name);

                sql = string.Concat(sql, SelectUtilities.GetSqlSelectColumn(tableAndColmnNameSql));
            }

            Sql = Sql.Replace(expression.ToString(), sql);

            ReturnObject = expression;
        }

        private static void _evaluate(MemberExpression expression)
        {
            var tableAndColumnName = LoadColumnAndTableName(expression);
            var tableAndColumnNameSql = tableAndColumnName.GetTableAndColumnName(false);

            Sql = Sql.Replace(expression.ToString(), SelectUtilities.GetSqlSelectColumn(tableAndColumnNameSql));
        }

        private static string _getSql(MemberInitExpression expression)
        {
            var sql = string.Empty;

            for (var i = 0; i < expression.Bindings.Count; i++)
            {
                var binding = expression.Bindings[i];

                switch (binding.BindingType)
                {
                    case MemberBindingType.Assignment:
                        var assignment = (MemberAssignment)binding;
                        var memberInitExpression = assignment.Expression as MemberInitExpression;

                        if (memberInitExpression != null)
                        {
                            sql = string.Concat(sql, _getSql(memberInitExpression));
                            continue;
                        }

                        var tableAndColumnName = LoadColumnAndTableName((dynamic)assignment.Expression);
                        var tableAndColmnNameSql = SelectUtilities.GetTableAndColumnNameWithAlias(tableAndColumnName, assignment.Member.Name);

                        sql = string.Concat(sql, SelectUtilities.GetSqlSelectColumn(tableAndColmnNameSql));
                        break;
                    case MemberBindingType.ListBinding:
                        break;
                    case MemberBindingType.MemberBinding:
                        break;
                }
            }

            return sql;
        }
    }

    internal static class WhereUtilities
    {
        public static bool IsLambdaMethod(Expression expression)
        {
            var e = expression as MemberExpression;

            if (e == null) return false;

            var methodCallExpression = e.Expression as MethodCallExpression;

            return methodCallExpression != null && methodCallExpression.ToString().Contains("=>");
        }

        public static string GetTableNameFromLambdaParameter(BinaryExpression expression)
        {
            var left = expression.Left as MemberExpression;
            var right = expression.Right as MemberExpression;

            if (left != null && left.Expression != null && left.Expression is ParameterExpression)
            {
                return left.Expression.Type.GetTableName();
            }

            if (right != null && right.Expression != null && right.Expression is ParameterExpression)
            {
                return right.Expression.Type.GetTableName();
            }
            return "";
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

        public static bool IsConstant(Expression expression)
        {
            var constant = expression as ConstantExpression;

            if (constant != null) return true;

            var memberExpression = expression as MemberExpression;

            // could be something like Guid.Empty, DateTime.Now
            if (memberExpression != null) return memberExpression.Member.DeclaringType == memberExpression.Type;

            return false;
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

    internal static class SelectUtilities
    {
        public static string GetSqlFromExpression(Expression expression)
        {
            return string.Format("SELECT\r\n {0}", expression);
        }

        public static string GetSqlSelectColumn(string tableAndColumnName)
        {
            return string.Format("{0}{1}{2}", "\t", tableAndColumnName, "\r\n");
        }

        public static string GetTableAndColumnNameWithAlias(TableColumnContainer container, string alias)
        {
            return string.Format("{0} AS [{1}],", container.GetTableAndColumnName(false), alias);
        }
    }

    public abstract class ExpressQueryResolverBase
    {
        public static string Sql { get; protected set; }

        public static bool IsSubQuery { get; protected set; }

        protected static List<SqlDbParameter> Parameters { get; set; }

        protected static Queue<KeyValuePair<string, Expression>> Order;

        protected static ReadOnlyTableCollection Tables { get; set; }

        protected static Guid QueryId { get; set; }

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
            return WhereUtilities.IsConstant(expression.Right)
                ?  GetValue(expression.Right as dynamic)
                : GetValue(expression.Left as dynamic);
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
        protected static TableColumnContainer LoadColumnAndTableName(MemberExpression expression)
        {
            var columnAttribute = expression.Member.GetCustomAttribute<ColumnAttribute>();
            var columnName = columnAttribute != null ? columnAttribute.Name : expression.Member.Name;
            string alias;
            string tableName;

            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                alias = Tables.Find(expression.Expression.Type, QueryId).Alias;
                tableName = expression.Expression.Type.GetTableName();
            }
            else
            {
                alias = Tables.FindByPropertyName(((MemberExpression)expression.Expression).Member.Name, QueryId).Alias;
                tableName = ((MemberExpression)expression.Expression).Member.Name;
            }

            return new TableColumnContainer(tableName,columnName, alias);
        }

        protected static TableColumnContainer LoadColumnAndTableName(MethodCallExpression expression)
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

        protected static TableColumnContainer LoadColumnAndTableName(UnaryExpression expression)
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

    public class TableColumnContainer
    {
        public readonly string TableName;

        public readonly string ColumnName;

        public readonly string Alias;

        public TableColumnContainer(string tableName, string columnName, string alias)
        {
            TableName = tableName;
            ColumnName = columnName;
            Alias = alias;
        }

        public string GetTableAndColumnName(bool isSubQuery)
        {
            return isSubQuery
                ? string.Format("[{0}].[{1}]", TableName, ColumnName)
                : string.Format("[{0}].[{1}]", Alias, ColumnName);
        }
    }
}
