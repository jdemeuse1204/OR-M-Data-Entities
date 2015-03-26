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
using OR_M_Data_Entities.Commands.StatementParts;
using OR_M_Data_Entities.Commands.Transform;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Expressions
{
    public abstract class ExpressionResolver
    {
        #region Resolvers
        protected static IEnumerable<SqlWhere> ResolveWhere<T>(Expression<Func<T, bool>> expression)
        {
            var evaluationResults = new List<SqlWhere>();

            _evaluateExpressionTree(expression.Body, evaluationResults);

            return evaluationResults;
        }

        protected static Dictionary<KeyValuePair<Type, Type>, SqlJoin> ResolveJoin<TParent, TChild>(Expression<Func<TParent, TChild, bool>> expression, JoinType joinType)
        {
            var evaluationResults = new Dictionary<KeyValuePair<Type, Type>, SqlJoin>();

            _evaluateJoinExpressionTree(expression.Body, evaluationResults, joinType);

            return evaluationResults;
        }

        protected static IEnumerable<SqlTableColumnPair> ResolveSelect<T>(Expression<Func<T, object>> expression)
        {
            var evaluationResults = new List<SqlTableColumnPair>();

            _evaltateSelectExpressionTree(expression.Body, evaluationResults);

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
        private static void _evaltateSelectExpressionTree(Expression expression, List<SqlTableColumnPair> evaluationResults)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.New:
                    var e = expression as NewExpression;

                    for (var i = 0; i < e.Arguments.Count; i++)
                    {
                        var arg = e.Arguments[i] as MemberExpression;
                        var newExpressionColumnAndTableName = _getColumnAndTableName(arg, SqlDbType.VarChar);

                        evaluationResults.Add(newExpressionColumnAndTableName);
                    }
                    break;
                case ExpressionType.Convert:
                    var convertExpressionColumnAndTableName = _getTableNameAndColumns((dynamic)expression);

                    evaluationResults.AddRange(convertExpressionColumnAndTableName);
                    break;
                case ExpressionType.Call:

                    var callExpressionColumnAndTableName = _getColumnAndTableName(((MethodCallExpression)expression), SqlDbType.VarChar);

                    evaluationResults.Add(callExpressionColumnAndTableName);
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
        private static void _evaluateExpressionTree(Expression expression, List<SqlWhere> evaluationResults)
        {
            if (HasLeft(expression))
            {
                var result = _evaluateWhere(((dynamic)expression).Right);

                evaluationResults.Add(result);

                _evaluateExpressionTree(((BinaryExpression)expression).Left, evaluationResults);
            }
            else
            {
                var result = _evaluateWhere((dynamic)expression);

                evaluationResults.Add(result);
            }
        }

        private static void _evaluateJoinExpressionTree(Expression expression, Dictionary<KeyValuePair<Type, Type>, SqlJoin> evaluationResults, JoinType joinType)
        {
            if (HasLeft(expression))
            {
                var result = _evaluateJoin(((dynamic)expression).Right, joinType);
                var key = new KeyValuePair<Type, Type>(
                    ((SqlJoin) result).ParentEntity.Table,
                    ((SqlJoin) result).JoinEntity.Table);

                if (!evaluationResults.ContainsKey(key))
                {
                    evaluationResults.Add(key, result);
                }

                _evaluateJoinExpressionTree(((BinaryExpression)expression).Left, evaluationResults, joinType);
            }
            else
            {
                var result = _evaluateJoin((dynamic)expression, joinType);
                var key = new KeyValuePair<Type, Type>(
                    ((SqlJoin)result).ParentEntity.Table,
                    ((SqlJoin)result).JoinEntity.Table);

                if (!evaluationResults.ContainsKey(key))
                {
                    evaluationResults.Add(key, result);
                }
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
        private static IEnumerable<SqlTableColumnPair> _getTableNameAndColumns(UnaryExpression expression)
        {
            var parameterExpression = expression.Operand as ParameterExpression;

            if (parameterExpression == null) throw new InvalidExpressionException("Unary Expression's Operand must be Parameter Expression");

            return DatabaseSchemata.GetTableFields(parameterExpression.Type).Select(column => new SqlTableColumnPair
            {
                Column = column,
                DataType = column.GetCustomAttribute<DbTranslationAttribute>() == null ? 
                    DatabaseSchemata.GetSqlDbType(column.PropertyType) : 
                    column.GetCustomAttribute<DbTranslationAttribute>().Type,
                Table = parameterExpression.Type
            }).ToList();
        }

        #region Get Column and Table Name

        private static SqlTableColumnPair _getColumnAndTableName(BinaryExpression expression, SqlDbType transformType, bool isCasting = false,
            bool isConverting = false, int conversionStyle = 0)
        {
            var leftSide = expression.Left;
            var rightSide = expression.Right;
            var leftSideHasParameter = _hasParameter(leftSide);
            var e = leftSideHasParameter ? leftSide : rightSide;

            // Get column options (Column Name, Table Name, Transform Type)
            return _getColumnAndTableName(e,
                SqlDbType.VarChar, isCasting, isConverting, conversionStyle);
        }

        private static SqlTableColumnPair _getColumnAndTableName(UnaryExpression expression,
            SqlDbType transformType, bool isCasting = false,
            bool isConverting = false, int conversionStyle = 0)
        {
            return _getColumnAndTableName(expression.Operand, transformType, isCasting,
                isConverting, conversionStyle);
        }

        private static SqlTableColumnPair _getColumnAndTableName(
            MethodCallExpression expression,
            SqlDbType transformType,
            bool isCasting = false,
            bool isConverting = false,
            int conversionStyle = 0)
        {
            var dataTransform = SqlDbType.VarChar;
            var conversionNumber = 0;
            var cast = DatabaseOperations.IsCasting(expression);
            var convert = DatabaseOperations.IsConverting(expression);

            if (cast || convert)
            {
                dataTransform = GetTransformType(expression);

                if (convert)
                {
                    object argValue;

                    if (expression.Arguments[2] is UnaryExpression)
                    {
                        argValue = _getValue((dynamic) expression.Arguments[2]);
                    }
                    else
                    {
                        argValue = ((dynamic)expression.Arguments[2]).Value;
                    }

                    conversionNumber = Convert.ToInt32(argValue);
                }
            }

            var evaluatedExpression = expression.Object ?? expression.Arguments.FirstOrDefault(
                w => w.NodeType == ExpressionType.MemberAccess || w.NodeType == ExpressionType.Convert);

            return _getColumnAndTableName(evaluatedExpression, dataTransform, cast, convert, conversionNumber);
        }

        private static SqlTableColumnPair _getColumnAndTableName(
            MemberExpression expression,
            SqlDbType transformType,
            bool isCasting = false,
            bool isConverting = false,
            int conversionStyle = 0)
        {
            if (expression.Expression.NodeType != ExpressionType.Parameter)
            {
                return _getColumnAndTableName(expression.Expression as MemberExpression, transformType, isCasting, isConverting, conversionStyle);
            }

            var result = new SqlTableColumnPair();
            result.Table = expression.Expression.Type;
            result.Column = expression.Member;
            result.DataType = isCasting || isConverting
                ? transformType
                : expression.Member.GetCustomAttribute<DbTranslationAttribute>() == null
                    ? DatabaseSchemata.GetSqlDbType(expression.Type)
                    : expression.Member.GetCustomAttribute<DbTranslationAttribute>().Type;

            if (isCasting)
            {
                result.AddFunction(DbFunctions.Cast, result.DataType);
            }

            if (isConverting)
            {
                result.AddFunction(DbFunctions.Convert, result.DataType, conversionStyle);
            }

            return result;
        }

        private static SqlTableColumnPair _getColumnAndTableName(object expression, SqlDbType transformType, bool isCasting = false, bool isConverting = false, int conversionStyle = 0)
        {
            return _getColumnAndTableName(expression as dynamic, transformType, isCasting, isConverting, conversionStyle);
        }
        #endregion

        #endregion

        #region Expression Join Evaluation
        private static SqlJoin _evaluateJoin(object expression, JoinType joinType)
        {
            return _evaluateJoin(expression as dynamic, joinType);
        }

        private static SqlJoin _evaluateJoin(MethodCallExpression expression, JoinType joinType)
        {
            var result = new SqlJoin();
            var columnOptions = new SqlTableColumnPair();
            var argsHaveParameter = false;

            foreach (var arg in expression.Arguments.Where(_hasParameter))
            {
                argsHaveParameter = true;
                columnOptions = _getColumnAndTableName(arg, SqlDbType.VarChar);
                result.JoinEntity = _getValue(expression.Object as dynamic) as SqlTableColumnPair;
                break;
            }

            if (!argsHaveParameter)
            {
                columnOptions = _getColumnAndTableName(expression.Object as dynamic, SqlDbType.VarChar);
                result.JoinEntity = _getValue(expression.Arguments[0] as dynamic) as SqlTableColumnPair;
            }

            result.ParentEntity = columnOptions;
            result.Type = joinType;

            return result;
        }

        private static SqlJoin _evaluateJoin(BinaryExpression expression, JoinType joinType)
        {
            var result = new SqlJoin();

            // Get column options (Column Name, Table Name, Transform Type)
            var columnOptions = _getColumnAndTableName(expression, SqlDbType.VarChar);

            result.ParentEntity = columnOptions;
            result.JoinEntity = GetCompareValue(expression, SqlDbType.VarChar) as SqlTableColumnPair;
            result.Type = joinType;

            return result;
        }

        private static SqlJoin _evaluateJoin(UnaryExpression expression, JoinType joinType)
        {
            var result = _evaluateJoin(expression.Operand as dynamic, joinType);

            result.ComparisonType = GetComparisonType(expression.NodeType);

            return result;
        }
        #endregion

        #region Expression Where Evaluation

        private static SqlWhere _evaluateWhere(object expression)
        {
            return _evaluateWhere(expression as dynamic);
        }

        private static SqlWhere _evaluateWhere(MethodCallExpression expression)
        {
            var result = new SqlWhere();
            var columnOptions = new SqlTableColumnPair();
            var argsHaveParameter = false;

            foreach (var arg in expression.Arguments.Where(_hasParameter))
            {
                argsHaveParameter = true;
                columnOptions = _getColumnAndTableName(arg, SqlDbType.VarChar);
                result.ObjectCompareValue = _getValue(expression.Object as dynamic);
                break;
            }

            if (!argsHaveParameter)
            {
                columnOptions = _getColumnAndTableName(expression.Object as dynamic, SqlDbType.VarChar);
                result.ObjectCompareValue = _getValue(expression.Arguments[0] as dynamic);
            }

            result.TableCompareValue = columnOptions;
            result.ComparisonType = GetComparisonType(expression.Method.Name);

            return result;
        }

        private static SqlWhere _evaluateWhere(BinaryExpression expression)
        {
            var result = new SqlWhere();

            // Get column options (Column Name, Table Name, Transform Type)
            var columnOptions = _getColumnAndTableName(expression, SqlDbType.VarChar);

            result.TableCompareValue = columnOptions;
            result.ObjectCompareValue = GetCompareValue(expression, SqlDbType.VarChar);
            result.ComparisonType = GetComparisonType(expression.NodeType);

            return result;
        }

        private static SqlWhere _evaluateWhere(UnaryExpression expression)
        {
            var result = _evaluateWhere(expression.Operand as dynamic);

            result.ComparisonType = GetComparisonType(expression.NodeType);

            return result;
        }
        #endregion

        private static object GetCompareValue(BinaryExpression expression, SqlDbType transformType)
        {
            var leftSideHasParameter = _hasParameter(expression.Left);
            var rightSideHasParameter = _hasParameter(expression.Right);

            return rightSideHasParameter ?
                _getColumnAndTableName(expression.Right, transformType)
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
            var isCasting = DatabaseOperations.IsCasting(expression);
            var isConverting = DatabaseOperations.IsConverting(expression);
            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
   
            var getter = getterLambda.Compile();

            var result = getter();

            if (!isCasting && !isConverting) return result;

            var transform = GetTransformType(expression);
            var transformResult = new SqlValue(result, transform);

            if (isConverting)
            {
                // only from where statement
                transformResult.AddFunction(DbFunctions.Convert, transform, 1);
            }

            if (isCasting)
            {
                transformResult.AddFunction(DbFunctions.Cast, transform);
            }

            return transformResult;
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
