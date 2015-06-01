using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Collections;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Containers;

namespace OR_M_Data_Entities.Expressions.Resolution.Where
{
    public class WhereExpressionResolver : ExpressionResolver
    {
        public static void Resolve<T>(Expression<Func<T, bool>> expression, WhereResolutionContainer container, ReadOnlyTableTypeCollection tables)
        {
            _evaluateTree(expression.Body as dynamic, container, tables);
        }

        private static void _evaluateTree(MethodCallExpression expression, WhereResolutionContainer container, ReadOnlyTableTypeCollection tables)
        {
            var result = _evaluate(expression, container, tables, container.NextGroupNumber());

            container.AddResolution(result);
        }

        // need to evaluate groupings
        private static void _evaluateTree(BinaryExpression expression, WhereResolutionContainer container, ReadOnlyTableTypeCollection tables)
        {
            // check to see if we are at the end of the expression
            if (!HasComparison(expression))
            {
                var result = _evaluate(expression as dynamic, container, tables, container.NextGroupNumber());

                container.AddResolution(result);
                return;
            }

            var count = 0;
            var wasRightAdded = false;

            while (true)
            {
                // if the right has a left then its a group expression
                if (IsGroup(expression.Right))
                {
                    if (count != 0) container.AddConnector(ConnectorType.Or);

                    _evaluateGroup(expression.Right as BinaryExpression, container, tables);
                }
                else
                {
                    if (count != 0) container.AddConnector(ConnectorType.Or);

                    var result = _evaluate(expression.Right as dynamic, container, tables, container.NextGroupNumber());

                    container.AddResolution(result);
                    wasRightAdded = true;
                }

                // check to see if the end is a group
                if (IsGroup(expression.Left))
                {
                    if (count != 0) container.AddConnector(ConnectorType.Or);

                    if (count == 0 && wasRightAdded) container.AddConnector(ConnectorType.And);

                    _evaluateGroup(expression.Left as BinaryExpression, container, tables);
                    break;
                }

                // check to see if we are at the end of the expression
                if (!HasComparison(expression.Left))
                {
                    if (count != 0) container.AddConnector(ConnectorType.Or);

                    if (count == 0 && wasRightAdded) container.AddConnector(ConnectorType.And);

                    var result = _evaluate(expression.Left as dynamic, container, tables, container.NextGroupNumber());

                    container.AddResolution(result);
                    break;
                }

                expression = expression.Left as BinaryExpression;
                count++;
            }
        }

        #region Evaluate

        private static void _evaluateGroup(BinaryExpression expression, WhereResolutionContainer resolution, ReadOnlyTableTypeCollection tables)
        {
            var groupNumber = resolution.NextGroupNumber();
            // comparisons are always AND here, will be no groups in here
            while (true)
            {
                var rightResult = _evaluate(expression.Right as dynamic, resolution, tables);

                rightResult.Group = groupNumber;

                resolution.AddResolution(rightResult);
                resolution.AddConnector(ConnectorType.And);

                if (HasComparison(expression.Left))
                {
                    expression = expression.Left as BinaryExpression;
                    continue;
                }

                var leftResult = _evaluate(expression.Left as dynamic, resolution, tables);

                leftResult.Group = groupNumber;

                resolution.AddResolution(leftResult);
                break;
            }
        }


        /// <summary>
        /// Happens when the method uses an operator
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="resolution"></param>
        private static WhereResolutionPart _evaluate(BinaryExpression expression, WhereResolutionContainer resolution, ReadOnlyTableTypeCollection tables, int group = -1)
        {
            var result = new WhereResolutionPart
            {
                Group = group,
            };
            var isParameterOnLeftSide = HasParameter(expression.Left as dynamic);

            // load the table and column name into the result
            LoadColumnAndTableName((isParameterOnLeftSide ? expression.Left : expression.Right) as dynamic, result, tables);

            // get the value from the expression
            var value = GetValue((isParameterOnLeftSide ? expression.Right : expression.Left) as dynamic);

            // if its an expression query we need to combine parameters into the main query
            result.CompareValue = IsExpressionQuery(value) ? value : resolution.GetParameter(value);

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
        private static WhereResolutionPart _evaluate(MethodCallExpression expression, WhereResolutionContainer resolution, ReadOnlyTableTypeCollection tables, int group = -1, ExpressionType? expressionType = null)
        {
            var result = new WhereResolutionPart
            {
                Group = group
            };
            var isParameterOnLeftSide = HasParameter(expression.Object as dynamic);
            object value;

            if (isParameterOnLeftSide)
            {
                LoadColumnAndTableName(expression.Object as dynamic, result, tables);

                value = GetValue(expression.Arguments[0] as dynamic);
            }
            else
            {
                LoadColumnAndTableName(expression.Arguments[0] as dynamic, result, tables);

                value = GetValue(expression.Object as dynamic);
            }

            var invertComparison = expressionType.HasValue && expressionType.Value == ExpressionType.Not;

            switch (expression.Method.Name.ToUpper())
            {
                case "EQUALS":
                case "OP_EQUALITY":
                    result.Comparison = invertComparison ? CompareType.NotEqual : CompareType.Equals;
                    result.CompareValue = resolution.GetParameter(value);
                    break;
                case "CONTAINS":
                    result.Comparison = value.IsList()
                        ? (invertComparison ? CompareType.NotIn : CompareType.In)
                        : (invertComparison ? CompareType.NotLike : CompareType.Like);

                    if (!value.IsList())
                    {
                        result.CompareValue = resolution.GetParameter(string.Format("%{0}%", value));
                    }
                    else
                    {
                        result.CompareValue = new List<SqlDbParameter>();

                        foreach (var item in ((ICollection) value))
                        {
                            ((List<SqlDbParameter>)result.CompareValue).Add(resolution.GetParameter(string.Format("{0}", item)));
                        }
                    }
                    break;
                case "STARTSWITH":
                    result.Comparison = invertComparison ? CompareType.NotLike : CompareType.Like;
                    result.CompareValue = resolution.GetParameter(string.Format("{0}%", value));
                    break;
                case "ENDSWITH":
                    result.Comparison = invertComparison ? CompareType.NotLike : CompareType.Like;
                    result.CompareValue = resolution.GetParameter(string.Format("%{0}", value));;
                    break;
            }

            return result;
        }

        private static WhereResolutionPart _evaluate(UnaryExpression expression, WhereResolutionContainer resolution, ReadOnlyTableTypeCollection tables, int group = -1)
        {
            return _evaluate(expression.Operand as dynamic, resolution, tables, group, expression.NodeType);
        }
        #endregion

        #region Load Comparison Type
        private static void LoadComparisonType(BinaryExpression expression, WhereResolutionPart resolution)
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
        private static void LoadColumnAndTableName(MemberExpression expression, WhereResolutionPart result, ReadOnlyTableTypeCollection tables)
        {
            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                // cannot come from a foriegn key, is the base type
                result.ColumnName = expression.Member.Name;
                result.TableName = DatabaseSchemata.GetTableName(expression.Expression.Type);
                result.TableAlias = tables.Find(expression.Expression.Type).Alias;
            }
            else
            {
                // will be from a foreign key, not the base type
                result.ColumnName = expression.Member.Name;
                result.TableName = ((MemberExpression)expression.Expression).Member.Name;
                result.TableAlias = tables.FindByPropertyName(((MemberExpression)expression.Expression).Member.Name).Alias;
            }
        }

        private static void LoadColumnAndTableName(MethodCallExpression expression, WhereResolutionPart result, ReadOnlyTableTypeCollection tables)
        {
            LoadColumnAndTableName(expression.Object as MemberExpression, result, tables);
        }
        #endregion

        private static bool HasComparison(Expression expression)
        {
            return expression.NodeType == ExpressionType.And
                || expression.NodeType == ExpressionType.AndAlso
                || expression.NodeType == ExpressionType.Or
                || expression.NodeType == ExpressionType.OrElse;
        }

        private static bool IsGroup(Expression expression)
        {
            var binaryExpression = expression as BinaryExpression;

            return binaryExpression != null &&
                   (binaryExpression.NodeType == ExpressionType.And ||
                    binaryExpression.NodeType == ExpressionType.AndAlso);
        }

        private static bool HasParameter(ConstantExpression expression)
        {
            return false;
        }

        private static bool HasParameter(UnaryExpression expression)
        {
            return expression == null ? false : HasParameter(expression.Operand as dynamic);
        }

        private static bool HasParameter(ParameterExpression expression)
        {
            return true;
        }

        private static bool HasParameter(MemberExpression expression)
        {
            return HasParameter(expression.Expression as dynamic);
        }

        private static bool HasParameter(MethodCallExpression expression)
        {
            var e = expression.Object;

            return e != null
                ? HasParameter(expression.Object as dynamic)
                : expression.Arguments.Select(arg => HasParameter(arg as dynamic)).Any(hasParameter => hasParameter);
        }
    }
}
