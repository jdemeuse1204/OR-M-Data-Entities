using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.Secure.StatementParts;
using OR_M_Data_Entities.Commands.Transform;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Operations.ObjectMapping;
using OR_M_Data_Entities.Expressions.Operations.ObjectMapping.Base;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Expressions.Operations.LambdaResolution
{
    public class LambdaResolver
    {
        #region Resolvers
        public static void ResolveWhereExpression<T>(Expression<Func<T, bool>> expression, ObjectMap map)
        {
            _evaluateExpressionTree(expression.Body, map);
        }

        public static void ResolveJoinExpression<TParent, TChild>(Expression<Func<TParent, TChild, bool>> expression, JoinType joinType, ObjectMap map)
        {
            _evaluateJoinExpressionTree(expression.Body, joinType, map);
        }

        public static void ResolveSelectExpression<T>(Expression<Func<T, object>> selector, ObjectMap map)
        {
            _evaltateSelectExpressionTree(selector.Body, map);
        }
        #endregion

        #region Tree Evaluation
        /// <summary>
        /// Evaluates the expression tree to resolve it into ExpressionSelectResult's
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="evaluationResults"></param>
        /// <param name="tableNameLookup"></param>
        private static void _evaltateSelectExpressionTree(Expression expression, ObjectMap map)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.New:
                    var newExpression = expression as NewExpression;

                    for (var i = 0; i < newExpression.Arguments.Count; i++)
                    {
                        var arg = newExpression.Arguments[i] as MemberExpression;
                        var table = map.Tables.FirstOrDefault(w => w.Type == arg.Expression.Type);

                        if (table == null)
                        {
                            map.AddSingleTable(arg.Expression.Type);

                            table = map.Tables.FirstOrDefault(w => w.Type == arg.Expression.Type);
                        }

                        var column = table.Columns.First(w => w.PropertyName == arg.Member.Name);

                        column.IsSelected = true;
                        column.ColumnAlias = newExpression.Members[i].Name;
                    }
                    break;
                case ExpressionType.Convert:
                    var convertExpressionColumnAndTableName = _getTableNameAndColumns((dynamic)expression);

                    //evaluationResults.AddRange(convertExpressionColumnAndTableName);
                    break;
                case ExpressionType.Call:

                    //var callExpressionColumnAndTableName = _getColumnAndTableName(((MethodCallExpression)expression), SqlDbType.VarChar, null);

                    //evaluationResults.Add(callExpressionColumnAndTableName);
                    break;
                case ExpressionType.MemberInit:
                    var memberInitExpression = expression as MemberInitExpression;
                    map.DataReturnType = ObjectMapReturnType.MemberInit;
                    map.MemberInitCount++;

                    if (map.MemberInitCount > 1)
                    {
                        throw new Exception("Cannot select multiple types.  If you wish to select more than one type, select anonymous type and return a dynamic");
                    }

                    for (var i = 0; i < memberInitExpression.Bindings.Count; i++)
                    {
                        var assignment = memberInitExpression.Bindings[i] as MemberAssignment;

                        var tableType = ((dynamic)assignment.Expression).Expression.Type;
                        var targetMemberName = ((dynamic)assignment.Expression).Member.Name;

                        var targetTable = map.Tables.FirstOrDefault(w => w.Type == tableType);

                        if (targetTable == null)
                        {
                            map.AddSingleTable(assignment.Expression.Type);

                            targetTable = map.Tables.FirstOrDefault(w => w.Type == tableType);
                        }

                        var targetColumn = targetTable.Columns.First(w => w.PropertyName == targetMemberName);

                        // need to change for the reader to load correctly
                        targetColumn.TableAlias = "";
                        targetColumn.ColumnAlias = string.Format("{0}{1}", DatabaseSchemata.GetTableName(assignment.Member.DeclaringType), assignment.Member.Name);
                        targetColumn.IsSelected = true;
                    }

                    break;
                case ExpressionType.MemberAccess:
                    map.DataReturnType = ObjectMapReturnType.Value;
                    map.MemberInitCount++;

                    var memberAccessTableType = ((dynamic)expression).Expression.Type;
                    var memberAccessName = ((dynamic)expression).Member.Name;
                    var memberAccessTable = map.Tables.FirstOrDefault(w => w.Type == memberAccessTableType);

                    if (memberAccessTable == null)
                    {
                        map.AddSingleTable(memberAccessTableType);

                        memberAccessTable = map.Tables.FirstOrDefault(w => w.Type == memberAccessTableType);
                    }

                    var memberAccessColumn = memberAccessTable.Columns.First(w => w.PropertyName == memberAccessName);

                    memberAccessColumn.IsSelected = true;

                    break;
                default:
                    break;
            }
        }

        private static void _evaluateExpressionTree(Expression expression, ObjectMap map)
        {
            if (HasLeft(expression))
            {
                _evaluateWhere(((dynamic)expression).Right, map);

                _evaluateExpressionTree(((BinaryExpression)expression).Left, map);
            }
            else
            {
                _evaluateWhere((dynamic)expression, map);
            }
        }

        private static void _evaluateJoinExpressionTree(Expression expression, JoinType joinType, ObjectMap map)
        {
            if (HasLeft(expression))
            {
                _evaluateJoin(((dynamic)expression).Right, joinType, map);

                _evaluateJoinExpressionTree(((BinaryExpression)expression).Left, joinType, map);
            }
            else
            {
                _evaluateJoin((dynamic)expression, joinType, map);
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

        private static void _addWhereValidation(
            BinaryExpression expression,
            ObjectMap map,
            ConversionPackage conversionPackage,
            ComparisonType compareType,
            object compareValue)
        {
            var leftSide = expression.Left;
            var rightSide = expression.Right;
            var leftSideHasParameter = _hasParameter(leftSide);
            var e = leftSideHasParameter ? leftSide : rightSide;

            _addWhereValidation(e, map, conversionPackage, compareType, compareValue);
        }

        private static void _addWhereValidation(
            UnaryExpression expression,
            ObjectMap map,
            ConversionPackage conversionPackage,
            ComparisonType compareType,
            object compareValue)
        {
            _addWhereValidation(expression.Operand, map, conversionPackage, compareType, compareValue);
        }

        private static void _addWhereValidation(
            MethodCallExpression expression,
            ObjectMap map,
            ConversionPackage conversionPackage,
            ComparisonType compareType,
            object compareValue)
        {
            conversionPackage.IsCasting = DatabaseOperations.IsCasting(expression);
            conversionPackage.IsConverting = DatabaseOperations.IsConverting(expression);

            if (conversionPackage.IsCasting || conversionPackage.IsConverting)
            {
                conversionPackage.TransformType = GetTransformType(expression);

                if (conversionPackage.IsConverting)
                {
                    object argValue;

                    if (expression.Arguments[2] is UnaryExpression)
                    {
                        argValue = _getValue((dynamic)expression.Arguments[2]);
                    }
                    else
                    {
                        argValue = ((dynamic)expression.Arguments[2]).Value;
                    }

                    conversionPackage.ConversionStyle = Convert.ToInt32(argValue);
                }
            }

            var evaluatedExpression = expression.Object ?? expression.Arguments.FirstOrDefault(
                w => w.NodeType == ExpressionType.MemberAccess || w.NodeType == ExpressionType.Convert);

            _addWhereValidation(evaluatedExpression, map, conversionPackage, compareType, compareValue);
        }

        private static void _addWhereValidation(
            MemberExpression expression,
            ObjectMap map,
            ConversionPackage conversionPackage,
            ComparisonType compareType,
            object compareValue)
        {
            if (expression.Expression.NodeType != ExpressionType.Parameter)
            {
                _addWhereValidation(expression.Expression as MemberExpression, map, conversionPackage, compareType, compareValue);
            }

            var table = map.Tables.FirstOrDefault(w => w.Type == expression.Expression.Type);

            if (table == null)
            {
                throw new Exception(string.Format("Could not find Table from type of {0}",
                    expression.Expression.Type.Name));
            }

            var columnName = DatabaseSchemata.GetColumnName(expression.Member);
            var column = table.Columns.FirstOrDefault(w => w.Name == columnName);

            if (column == null)
            {
                throw new Exception(string.Format("Could not find column {0}",
                    columnName));
            }

            column.DataType = conversionPackage.IsCasting || conversionPackage.IsConverting
                ? conversionPackage.TransformType
                : expression.Member.GetCustomAttribute<DbTranslationAttribute>() == null
                    ? DatabaseSchemata.GetSqlDbType(expression.Type)
                    : expression.Member.GetCustomAttribute<DbTranslationAttribute>().Type;

            if (conversionPackage.IsCasting)
            {
                column.AddFunction(DbFunctions.Cast, column.DataType);
            }

            if (conversionPackage.IsConverting)
            {
                column.AddFunction(DbFunctions.Convert, column.DataType, conversionPackage.ConversionStyle);
            }

            column.CompareValues.Add(new KeyValuePair<object, ComparisonType>(compareValue, compareType));
        }

        private static void _addWhereValidation(
            object expression,
            ObjectMap map,
            ConversionPackage conversionPackage,
            ComparisonType compareType,
            object compareValue)
        {
            _addWhereValidation(expression as dynamic, map, conversionPackage, compareType, compareValue);
        }
        #endregion

        #endregion

        #region Expression Join Evaluation
        private static void _evaluateJoin(object expression, JoinType joinType, ObjectMap map)
        {
            _evaluateJoin(expression as dynamic, joinType, map);
        }

        private static void _evaluateJoin(MethodCallExpression expression, JoinType joinType, ObjectMap map)
        {
            var result = new SqlJoin();
            var columnOptions = new SqlTableColumnPair();
            var argsHaveParameter = false;

            foreach (var arg in expression.Arguments.Where(_hasParameter))
            {
                argsHaveParameter = true;
                //columnOptions = _getColumnAndTableName(arg, SqlDbType.VarChar, null);
                result.JoinEntity = _getValue(expression.Object as dynamic) as SqlTableColumnPair;
                break;
            }

            if (!argsHaveParameter)
            {
                // columnOptions = _addWhereValidation(expression.Object as dynamic, SqlDbType.VarChar, null);
                result.JoinEntity = _getValue(expression.Arguments[0] as dynamic) as SqlTableColumnPair;
            }

            result.ParentEntity = columnOptions;
            result.Type = joinType;
        }

        private static void _evaluateJoin(BinaryExpression expression, JoinType joinType, ObjectMap map)
        {
            var left = ((MemberExpression)expression.Left);
            var right = ((MemberExpression)expression.Right);
            var leftTable = map.Tables.FirstOrDefault(w => w.Type == left.Expression.Type);
            var rightTable = map.Tables.FirstOrDefault(w => w.Type == right.Expression.Type);

            if (leftTable == null)
            {
                map.AddSingleTable(left.Expression.Type);

                leftTable = map.Tables.FirstOrDefault(w => w.Type == left.Expression.Type);
            }

            if (rightTable == null)
            {
                map.AddSingleTable(right.Expression.Type);

                rightTable = map.Tables.FirstOrDefault(w => w.Type == right.Expression.Type);
            }

            var leftColumnName = DatabaseSchemata.GetColumnName(left.Member);
            var rightColumnName = DatabaseSchemata.GetColumnName(right.Member);
            var leftColumn = leftTable.Columns.FirstOrDefault(w => w.Name == leftColumnName);
            var rightColumn = rightTable.Columns.FirstOrDefault(w => w.Name == rightColumnName);

            if (leftColumn == null)
            {
                throw new Exception(string.Format("Could not find column {0}",
                    leftColumnName));
            }

            if (rightColumn == null)
            {
                throw new Exception(string.Format("Could not find column {0}",
                    rightColumnName));
            }

            leftColumn.Joins.Add(new KeyValuePair<ObjectColumn, JoinType>(rightColumn, joinType));
        }

        private static void _evaluateJoin(UnaryExpression expression, JoinType joinType, ObjectMap map)
        {
            _evaluateJoin(expression.Operand as dynamic, joinType, map);
        }
        #endregion

        #region Expression Where Evaluation

        private static void _evaluateWhere(object expression, ObjectMap map)
        {
            _evaluateWhere(expression as dynamic, map);
        }

        private static void _evaluateWhere(MethodCallExpression expression, ObjectMap map)
        {
            var argsHaveParameter = false;
            var conversionPackage = new ConversionPackage();
            var compareType = GetComparisonType(expression.Method.Name);

            foreach (var arg in expression.Arguments.Where(_hasParameter))
            {
                argsHaveParameter = true;
                var compareValue = _getValue(expression.Object as dynamic);
                _addWhereValidation(arg, map, conversionPackage, compareType, compareValue);
                break;
            }

            if (!argsHaveParameter)
            {
                var compareValue = _getValue(expression.Arguments[0] as dynamic);
                _addWhereValidation(expression.Object as dynamic, map, conversionPackage, compareType, compareValue);
            }
        }

        private static void _evaluateWhere(BinaryExpression expression, ObjectMap map)
        {
            var conversionPackage = new ConversionPackage();
            var compareValue = GetCompareValue(expression, SqlDbType.VarChar);
            var comparisonType = GetComparisonType(expression.NodeType);

            _addWhereValidation(expression, map, conversionPackage, comparisonType, compareValue);
        }

        private static void _evaluateWhere(UnaryExpression expression, ObjectMap map)
        {
            var result = _evaluateWhere(expression.Operand as dynamic, map);

            result.ComparisonType = GetComparisonType(expression.NodeType);
        }
        #endregion

        private static object GetCompareValue(BinaryExpression expression, SqlDbType transformType)
        {
            var leftSideHasParameter = _hasParameter(expression.Left);

            return _getValue(leftSideHasParameter ? expression.Right as dynamic : expression.Left as dynamic);
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

        #region helpers

        class ConversionPackage
        {
            public ConversionPackage()
            {
                TransformType = SqlDbType.VarChar;
            }

            public bool IsCasting { get; set; }

            public bool IsConverting { get; set; }

            public SqlDbType TransformType { get; set; }

            public int? ConversionStyle { get; set; }
        }
        #endregion
    }
}
