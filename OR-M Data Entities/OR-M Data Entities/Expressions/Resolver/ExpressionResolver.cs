/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.Transform;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Types;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Expressions.Resolver
{
    public abstract class ExpressionResolver
    {
        #region Resolvers
        protected static IEnumerable<ExpressionWhereResult> ResolveWhere<T>(Expression<Func<T, bool>> expression)
        {
            var evaluationResults = new List<ExpressionWhereResult>();
            // lambda string, tablename
            var tableNameLookup = new Dictionary<string, string>();

            for (var i = 0; i < expression.Parameters.Count; i++)
            {
                var parameter = expression.Parameters[i];

                tableNameLookup.Add(parameter.Name, DatabaseSchemata.GetTableName(parameter.Type));
            }

            _evaluateExpressionTree(expression.Body, evaluationResults, tableNameLookup);

            return evaluationResults;
        }

        protected static IEnumerable<ExpressionWhereResult> ResolveJoin<TParent, TChild>(Expression<Func<TParent, TChild, bool>> expression)
        {
            var evaluationResults = new List<ExpressionWhereResult>();
            // lambda string, tablename
            var tableNameLookup = new Dictionary<string, string>();

            for (var i = 0; i < expression.Parameters.Count; i++)
            {
                var parameter = expression.Parameters[i];

                tableNameLookup.Add(parameter.Name, DatabaseSchemata.GetTableName(parameter.Type));
            }

            _evaluateExpressionTree(expression.Body, evaluationResults, tableNameLookup);

            return evaluationResults;
        }

        protected static IEnumerable<ExpressionSelectResult> ResolveSelect<T>(Expression<Func<T, object>> expression)
        {
            var evaluationResults = new List<ExpressionSelectResult>();
            // lambda string, tablename
            var tableNameLookup = new Dictionary<string, string>();

            for (var i = 0; i < expression.Parameters.Count; i++)
            {
                var parameter = expression.Parameters[i];

                tableNameLookup.Add(parameter.Name, DatabaseSchemata.GetTableName(parameter.Type));
            }

            _evaltateSelectExpressionTree(expression.Body, evaluationResults, tableNameLookup);

            return evaluationResults;
        }
        #endregion

        #region Tree Evaluation
        /// <summary>
        /// Evaluates the expression tree to resolve it into ExpressionSelectResult's
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="evaluationResults"></param>
        /// <param name="tableNameLookup"></param>
        private static void _evaltateSelectExpressionTree(Expression expression, ICollection<ExpressionSelectResult> evaluationResults,
            Dictionary<string, string> tableNameLookup)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.New:
                    var e = expression as NewExpression;

                    for (var i = 0; i < e.Arguments.Count; i++)
                    {
                        var arg = e.Arguments[i] as MemberExpression;
                        var newExpressionColumnAndTableName = _getColumnAndTableName(arg, tableNameLookup, SqlDbType.VarChar);

                        evaluationResults.Add(new ExpressionSelectResult
                        {
                            ColumnName = newExpressionColumnAndTableName.ColumnName,
                            TableName = newExpressionColumnAndTableName.TableName
                        });
                    }
                    break;
                case ExpressionType.Convert:
                    var convertExpressionColumnAndTableName = _getTableName((dynamic)expression, tableNameLookup);

                    evaluationResults.Add(new ExpressionSelectResult
                    {
                        ColumnName = convertExpressionColumnAndTableName.ColumnName,
                        TableName = convertExpressionColumnAndTableName.TableName
                    });
                    break;
                case ExpressionType.Call:

                    var callExpressionColumnAndTableName = _getColumnAndTableName(((MethodCallExpression)expression), tableNameLookup, SqlDbType.VarChar);

                    evaluationResults.Add(new ExpressionSelectResult
                    {
                        ColumnName = callExpressionColumnAndTableName.ColumnName,
                        TableName = callExpressionColumnAndTableName.TableName,
                        ShouldConvert = callExpressionColumnAndTableName.ShouldConvert,
                        ConversionStyle = callExpressionColumnAndTableName.ConversionStyle,
                        Transform = callExpressionColumnAndTableName.Transform
                    });
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Evaluates the expression tree to resolve it into ExpressionWhereResult's
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="evaluationResults"></param>
        /// <param name="tableNameLookup"></param>
        private static void _evaluateExpressionTree(Expression expression, ICollection<ExpressionWhereResult> evaluationResults, Dictionary<string, string> tableNameLookup)
        {
            if (HasLeft(expression))
            {
                var result = _evaluate(((dynamic)expression).Right, tableNameLookup);

                evaluationResults.Add(result);

                _evaluateExpressionTree(((BinaryExpression)expression).Left, evaluationResults, tableNameLookup);
            }
            else
            {
                var result = _evaluate((dynamic)expression, tableNameLookup);

                evaluationResults.Add(result);
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Gets the table name from an unary expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="tableNameLookup"></param>
        /// <returns></returns>
        private static ExpressionSelectResult _getTableName(UnaryExpression expression, Dictionary<string, string> tableNameLookup)
        {
            var parameterExpression = expression.Operand as ParameterExpression;

            return new ExpressionSelectResult
            {
                TableName = tableNameLookup.ContainsKey(parameterExpression.Name) ? tableNameLookup[parameterExpression.Name] : parameterExpression.Name,
                ColumnName = "*"
            };
        }

        #region Get Column and Table Name

        private static ExpressionSelectResult _getColumnAndTableName(BinaryExpression expression,
            Dictionary<string, string> tableNameLookup, SqlDbType transformType, bool isCasting = false,
            bool isConverting = false, int conversionStyle = 0)
        {
            var leftSide = expression.Left;
            var rightSide = expression.Right;
            var leftSideHasParameter = _hasParameter(leftSide);
            var e = leftSideHasParameter ? leftSide : rightSide;

            // Get column options (Column Name, Table Name, Transform Type)
            return _getColumnAndTableName(e, tableNameLookup,
                SqlDbType.VarChar, isCasting, isConverting, conversionStyle);
        }

        private static ExpressionSelectResult _getColumnAndTableName(UnaryExpression expression,
            Dictionary<string, string> tableNameLookup, SqlDbType transformType, bool isCasting = false,
            bool isConverting = false, int conversionStyle = 0)
        {
            return _getColumnAndTableName(expression.Operand, tableNameLookup, transformType, isCasting,
                isConverting, conversionStyle);
        }

        private static ExpressionSelectResult _getColumnAndTableName(MethodCallExpression expression,
           Dictionary<string, string> tableNameLookup, SqlDbType transformType, bool isCasting = false,
           bool isConverting = false, int conversionStyle = 0)
        {
            var dataTransform = SqlDbType.VarChar;
            var conversionNumber = 0;
            var cast = IsCasting(expression);
            var convert = IsConverting(expression);

            if (cast || convert)
            {
                dataTransform = GetTransformType(expression);

                if (convert)
                {
                    conversionNumber = Convert.ToInt32(((dynamic)expression.Arguments[2]).Value);
                }
            }

            var evaluatedExpression = expression.Object ?? expression.Arguments.FirstOrDefault(
                w => w.NodeType == ExpressionType.MemberAccess || w.NodeType == ExpressionType.Convert);

            return _getColumnAndTableName(evaluatedExpression, tableNameLookup, dataTransform, cast, convert, conversionNumber);
        }

        private static ExpressionSelectResult _getColumnAndTableName(MemberExpression expression,
           Dictionary<string, string> tableNameLookup, SqlDbType transformType, bool isCasting = false,
           bool isConverting = false, int conversionStyle = 0)
        {
            return expression.Expression.NodeType == ExpressionType.Parameter
                ? (new ExpressionSelectResult
                {
                    TableName = tableNameLookup.ContainsKey(((dynamic)expression.Expression).Name) ? tableNameLookup[((dynamic)expression.Expression).Name] : ((dynamic)expression.Expression).Name,
                    ColumnName = expression.Member.GetCustomAttribute<ColumnAttribute>() == null
                            ? expression.Member.Name
                            : expression.Member.GetCustomAttribute<ColumnAttribute>().Name,
                    Transform = isCasting || isConverting ? transformType : expression.Member.GetCustomAttribute<DbTranslationAttribute>() == null ?
                            ExpressionTypeTransform.GetSqlDbType(expression.Type)
                            : expression.Member.GetCustomAttribute<DbTranslationAttribute>().Type,
                    ShouldCast = isCasting,
                    ColumnType = expression.Type,
                    ShouldConvert = isConverting,
                    ConversionStyle = conversionStyle
                })
                : _getColumnAndTableName(expression.Expression as MemberExpression, tableNameLookup, transformType, isCasting, isConverting, conversionStyle);
        }

        private static ExpressionSelectResult _getColumnAndTableName(object expression, Dictionary<string, string> tableNameLookup, SqlDbType transformType, bool isCasting = false, bool isConverting = false, int conversionStyle = 0)
        {
            return _getColumnAndTableName(expression as dynamic, tableNameLookup, transformType, isCasting, isConverting, conversionStyle);
        }
        #endregion

        #endregion

        #region Expression Evaluation

        private static ExpressionWhereResult _evaluate(object expression, Dictionary<string, string> tableNameLookup)
        {
            return _evaluate(expression as dynamic, tableNameLookup);
        }

        private static ExpressionWhereResult _evaluate(MethodCallExpression expression, Dictionary<string, string> tableNameLookup)
        {
            var result = new ExpressionWhereResult();
            var columnOptions = new ExpressionSelectResult();
            var argsHaveParameter = false;

            foreach (var arg in expression.Arguments.Where(_hasParameter))
            {
                argsHaveParameter = true;
                columnOptions = _getColumnAndTableName(arg, tableNameLookup, SqlDbType.VarChar);
                result.CompareValue = _getValue(expression.Object as dynamic);
                break;
            }

            if (!argsHaveParameter)
            {
                columnOptions = _getColumnAndTableName(expression.Object as dynamic, tableNameLookup, SqlDbType.VarChar);
                result.CompareValue = _getValue(expression.Arguments[0] as dynamic);
            }

            result.ColumnName = columnOptions.ColumnName;
            result.TableName = columnOptions.TableName;
            result.ComparisonType = GetComparisonType(expression.Method.Name);
            result.Transform = columnOptions.Transform;
            result.ShouldCast = columnOptions.ShouldCast;

            return result;
        }

        /// <summary>
        /// Evaluates a binary expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="tableNameLookup"></param>
        /// <returns></returns>
        private static ExpressionWhereResult _evaluate(BinaryExpression expression, Dictionary<string, string> tableNameLookup)
        {
            var result = new ExpressionWhereResult();

            // Get column options (Column Name, Table Name, Transform Type)
            var columnOptions = _getColumnAndTableName(expression, tableNameLookup, SqlDbType.VarChar);

            result.ColumnName = columnOptions.ColumnName;
            result.TableName = columnOptions.TableName;
            result.Transform = columnOptions.Transform;
            result.CompareValue = GetCompareValue(expression, tableNameLookup, SqlDbType.VarChar);
            result.ComparisonType = GetComparisonType(expression.NodeType);
            result.ShouldCast = columnOptions.ShouldCast;

            return result;
        }

        /// <summary>
        /// Evaulates an unary expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="tableNameLookup"></param>
        /// <returns></returns>
        private static ExpressionWhereResult _evaluate(UnaryExpression expression, Dictionary<string, string> tableNameLookup)
        {
            var result = _evaluate(expression.Operand as dynamic, tableNameLookup);

            result.ComparisonType = GetComparisonType(expression.NodeType);

            return result;
        }
        #endregion

        private static object GetCompareValue(BinaryExpression expression, Dictionary<string, string> tableNameLookup,
           SqlDbType transformType)
        {
            var leftSideHasParameter = _hasParameter(expression.Left);
            var rightSideHasParameter = _hasParameter(expression.Right);

            return rightSideHasParameter ?
                _getColumnAndTableName(expression.Right, tableNameLookup, transformType)
                :
                _getValue(leftSideHasParameter ? expression.Right as dynamic : expression.Left as dynamic);
        }

        #region Get Expression Values
        /// <summary>
        /// Gets the value from a constant expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static object _getValue(ConstantExpression expression)
        {
            return expression.Value;
        }

        /// <summary>
        /// Gets the value from a member expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static object _getValue(MemberExpression expression)
        {
            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }

        /// <summary>
        /// Gets the valye from a method call expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static object _getValue(MethodCallExpression expression)
        {
            var isCasting = IsCasting(expression);

            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            var result = getter();

            if (!isCasting)
            {
                return result;
            }

            var transform = GetTransformType(expression);

            return new DataTransformContainer(result, transform);
        }

        /// <summary>
        /// Gets a value from a unary expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static object _getValue(UnaryExpression expression)
        {
            return _getValue(expression.Operand as dynamic);
        }
        #endregion

        #region Has Parameter
        public static bool _hasParameter(object expression)
        {
            return _hasParameter(expression as dynamic);
        }

        private static bool _hasParameter(MethodCallExpression expression)
        {
            var e = expression.Object;

            return e != null ? _hasParameter(expression.Object as dynamic) : expression.Arguments.Select(arg => _hasParameter(arg as dynamic)).Any(hasParameter => hasParameter);
        }

        private static bool _hasParameter(ConstantExpression expression)
        {
            return false;
        }

        private static bool _hasParameter(UnaryExpression expression)
        {
            return expression == null ? false : _hasParameter(expression.Operand as dynamic);
        }

        private static bool _hasParameter(ParameterExpression expression)
        {
            return true;
        }

        private static bool _hasParameter(MemberExpression expression)
        {
            return _hasParameter(expression.Expression as dynamic);
        }
        #endregion

        #region Cast and Convert
        public static bool IsConverting(object expression)
        {
            if (!(expression is MethodCallExpression)) return false;

            return ((MethodCallExpression)expression).Method.DeclaringType == typeof(Conversion);
        }

        public static bool IsCasting(object expression)
        {
            if (!(expression is MethodCallExpression)) return false;

            return ((MethodCallExpression)expression).Method.DeclaringType == typeof(Cast);
        }
        #endregion

        #region Get Transform and Comparison Types
        public static SqlDbType GetTransformType(MethodCallExpression expression)
        {
            return (from arg in expression.Arguments where arg.Type == typeof(SqlDbType) select ((ConstantExpression)arg).Value into value select value is SqlDbType ? (SqlDbType)value : SqlDbType.VarChar).FirstOrDefault();
        }

        public static ComparisonType GetComparisonType(string methodName)
        {
            switch (methodName.Replace(" ", "").ToUpper())
            {
                case "EQUALS":
                    return ComparisonType.Equals;
                case "NOTEQUALS":
                    return ComparisonType.NotEqual;
                case "LESSTHAN":
                    return ComparisonType.LessThan;
                case "GREATERTHAN":
                    return ComparisonType.GreaterThan;
                case "LESSTHANEQUALS":
                    return ComparisonType.LessThanEquals;
                case "GREATERTHANEQUALS":
                    return ComparisonType.GreaterThanEquals;
                case "CONTAINS":
                    return ComparisonType.Contains;
                case "STARTSWITH":
                    return ComparisonType.BeginsWith;
                case "ENDSWITH":
                    return ComparisonType.EndsWith;
                default:
                    throw new Exception("ExpressionType not in tree");
            }
        }

        public static ComparisonType GetComparisonType(ExpressionType expresssionType)
        {
            switch (expresssionType)
            {
                case ExpressionType.Equal:
                    return ComparisonType.Equals;
                case ExpressionType.GreaterThan:
                    return ComparisonType.GreaterThan;
                case ExpressionType.GreaterThanOrEqual:
                    return ComparisonType.GreaterThanEquals;
                case ExpressionType.LessThan:
                    return ComparisonType.LessThan;
                case ExpressionType.LessThanOrEqual:
                    return ComparisonType.LessThanEquals;
                case ExpressionType.NotEqual:
                case ExpressionType.Not:
                    return ComparisonType.NotEqual;
                default:
                    throw new Exception("ExpressionType not in tree");
            }
        }
        #endregion

        public static bool HasLeft(Expression expression)
        {
            return expression.NodeType == ExpressionType.And
                || expression.NodeType == ExpressionType.AndAlso
                || expression.NodeType == ExpressionType.Or
                || expression.NodeType == ExpressionType.OrElse;
        }
    }
}
