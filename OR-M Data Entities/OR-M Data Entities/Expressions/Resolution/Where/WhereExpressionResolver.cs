/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Containers;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Expressions.Resolution.Where
{
    public class WhereExpressionResolver : ExpressionResolver
    {
        public static void Resolve<T>(Expression<Func<T, bool>> expression, WhereResolutionContainer container, IExpressionQueryResolvable baseQuery, string viewId)
        {
            _evaluateTree(expression.Body as dynamic, container, baseQuery, viewId);
        }

        public static void ResolveFind<T>(object[] pks, WhereResolutionContainer container, IExpressionQueryResolvable baseQuery)
        {
            var table = baseQuery.Tables.Find(typeof(T), baseQuery.Id);
            var infos =
                baseQuery.SelectInfos.Where(w => w.IsPrimaryKey && w.Table.Type == typeof (T))
                    .OrderBy(w => w.Ordinal)
                    .ToList();

            for (var i = 0; i < pks.Count(); i++)
            {
                var isConnector = (i%2) != 0;

                if (isConnector)
                {
                    container.AddConnector(ConnectorType.And);
                    continue;
                }

                container.AddResolution(new WhereResolutionPart
                {
                    ColumnName = infos[i].NewPropertyName,
                    CompareValue = container.GetAndAddParameter(pks[i]),
                    Comparison = CompareType.Equals,
                    ExpressionQueryId = baseQuery.Id,
                    TableAlias = table.Alias,
                    TableName = table.Name
                });
            }
        }

        private static void _evaluateTree(MethodCallExpression expression, WhereResolutionContainer container, IExpressionQueryResolvable baseQuery, string viewId)
        {
            var result = _evaluate(expression, container, baseQuery, viewId, container.NextGroupNumber());

            container.AddResolution(result);
        }

        // need to evaluate groupings
        private static void _evaluateTree(BinaryExpression expression, WhereResolutionContainer container, IExpressionQueryResolvable baseQuery, string viewId)
        {
            // check to see if we are at the end of the expression
            if (!HasComparison(expression))
            {
                var result = _evaluate(expression as dynamic, container, baseQuery, viewId, container.NextGroupNumber());

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

                    _evaluateGroup(expression.Right as BinaryExpression, container, baseQuery, viewId);
                }
                else
                {
                    if (count != 0) container.AddConnector(ConnectorType.Or);

                    var result = _evaluate(expression.Right as dynamic, container, baseQuery, viewId, container.NextGroupNumber());

                    container.AddResolution(result);
                    wasRightAdded = true;
                }

                // check to see if the end is a group
                if (IsGroup(expression.Left))
                {
                    if (count != 0) container.AddConnector(ConnectorType.Or);

                    if (count == 0 && wasRightAdded) container.AddConnector(ConnectorType.And);

                    _evaluateGroup(expression.Left as BinaryExpression, container, baseQuery, viewId);
                    break;
                }

                // check to see if we are at the end of the expression
                if (!HasComparison(expression.Left))
                {
                    if (count != 0) container.AddConnector(ConnectorType.Or);

                    if (count == 0 && wasRightAdded) container.AddConnector(ConnectorType.And);

                    var result = _evaluate(expression.Left as dynamic, container, baseQuery, viewId, container.NextGroupNumber());

                    container.AddResolution(result);
                    break;
                }

                expression = expression.Left as BinaryExpression;
                count++;
            }
        }

        #region Evaluate

        private static void _evaluateGroup(BinaryExpression expression, WhereResolutionContainer resolution, IExpressionQueryResolvable baseQuery, string viewId)
        {
            var groupNumber = resolution.NextGroupNumber();
            // comparisons are always AND here, will be no groups in here
            while (true)
            {
                var rightResult = _evaluate(expression.Right as dynamic, resolution, baseQuery, viewId);

                rightResult.Group = groupNumber;

                resolution.AddResolution(rightResult);
                resolution.AddConnector(ConnectorType.And);

                if (HasComparison(expression.Left))
                {
                    expression = expression.Left as BinaryExpression;
                    continue;
                }

                var leftResult = _evaluate(expression.Left as dynamic, resolution, baseQuery, viewId);

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
        private static WhereResolutionPart _evaluate(BinaryExpression expression, WhereResolutionContainer resolution, IExpressionQueryResolvable baseQuery, string viewId, int group = -1)
        {
            var result = new WhereResolutionPart
            {
                Group = group,
            };
            var isParameterOnLeftSide = HasParameter(expression.Left as dynamic);

            // load the table and column name into the result
            LoadColumnAndTableName((isParameterOnLeftSide ? expression.Left : expression.Right) as dynamic, result, baseQuery, viewId);

            // get the value from the expression
            var value = GetValue((isParameterOnLeftSide ? expression.Right : expression.Left) as dynamic, baseQuery);

            // if its an expression query we need to combine parameters into the main query
            result.CompareValue = IsExpressionQuery(value) ? value : resolution.GetAndAddParameter(value);

            // get the comparison tyoe
            LoadComparisonType(expression, result);

            // add the result to the list
            return result;
        }

        private static WhereResolutionPart _evaluateLinqSubQuery(MethodCallExpression expression,
            WhereResolutionContainer resolution, IExpressionQueryResolvable baseQuery, string viewId, int group = -1,
            ExpressionType? expressionType = null)
        {
            for (var i = 1; i < expression.Arguments.Count; i++)
            {
                var argument = expression.Arguments[i];

                // evaluate the argument
                var result =_evaluate(((LambdaExpression)argument).Body as dynamic, resolution, baseQuery, viewId, group);

                result.Group = group;

                result.ExpressionQueryId = baseQuery.Id;

                // will be added when the value is returned from the method
                if (i == (expression.Arguments.Count - 1))
                {
                    return result;
                }

                resolution.AddResolution(result);
            }

            throw new Exception("Subquery Could not be evaluated");
        }

        /// <summary>
        /// Happens when the method uses a method to compare values, IE: Contains, Equals
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="resolution"></param>
        private static WhereResolutionPart _evaluate(MethodCallExpression expression, WhereResolutionContainer resolution, IExpressionQueryResolvable baseQuery, string viewId, int group = -1, ExpressionType? expressionType = null)
        {
            if (expression.Object == null)
            {
                return _evaluateLinqSubQuery(expression, resolution, baseQuery, viewId, group, expressionType);
            }

            var result = new WhereResolutionPart
            {
                Group = group
            };

            var isParameterOnLeftSide = HasParameter(expression.Object as dynamic);
            object value;

            if (isParameterOnLeftSide)
            {
                LoadColumnAndTableName(expression.Object as dynamic, result, baseQuery, viewId);

                value = GetValue(expression.Arguments[0] as dynamic, baseQuery);
            }
            else
            {
                LoadColumnAndTableName(expression.Arguments[0] as dynamic, result, baseQuery, viewId);

                value = GetValue(expression.Object as dynamic, baseQuery);
            }

            var invertComparison = expressionType.HasValue && expressionType.Value == ExpressionType.Not;

            switch (expression.Method.Name.ToUpper())
            {
                case "EQUALS":
                case "OP_EQUALITY":
                    result.Comparison = invertComparison ? CompareType.NotEqual : CompareType.Equals;
                    result.CompareValue = resolution.GetAndAddParameter(value);
                    break;
                case "CONTAINS":
                    result.Comparison = value.IsList()
                        ? (invertComparison ? CompareType.NotIn : CompareType.In)
                        : (invertComparison ? CompareType.NotLike : CompareType.Like);

                    if (!value.IsList())
                    {
                        result.CompareValue = resolution.GetAndAddParameter(string.Format("%{0}%", value));
                    }
                    else
                    {
                        result.CompareValue = new List<SqlDbParameter>();

                        foreach (var item in ((ICollection) value))
                        {
                            ((List<SqlDbParameter>)result.CompareValue).Add(resolution.GetAndAddParameter(string.Format("{0}", item)));
                        }
                    }
                    break;
                case "STARTSWITH":
                    result.Comparison = invertComparison ? CompareType.NotLike : CompareType.Like;
                    result.CompareValue = resolution.GetAndAddParameter(string.Format("{0}%", value));
                    break;
                case "ENDSWITH":
                    result.Comparison = invertComparison ? CompareType.NotLike : CompareType.Like;
                    result.CompareValue = resolution.GetAndAddParameter(string.Format("%{0}", value));;
                    break;
            }

            return result;
        }

        private static WhereResolutionPart _evaluate(UnaryExpression expression, WhereResolutionContainer resolution, IExpressionQueryResolvable baseQuery, string viewId, int group = -1)
        {
            return _evaluate(expression.Operand as dynamic, resolution, baseQuery, viewId, group, expression.NodeType);
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
        private static void LoadColumnAndTableName(MemberExpression expression, WhereResolutionPart result, IExpressionQueryResolvable baseQuery, string viewId)
        {
            if (!string.IsNullOrWhiteSpace(viewId) && !DatabaseSchemata.IsPartOfView(expression.Expression.Type, viewId))
            {
                throw new ViewException(string.Format("Type Of {0} Does not contain attribute for View - {1}",
                    expression.Expression.Type, viewId));
            }

            var columnAttribute = expression.Member.GetCustomAttribute<ColumnAttribute>();

            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                // cannot come from a foriegn key, is the base type
                result.ColumnName = columnAttribute != null ? columnAttribute.Name : expression.Member.Name;
                result.TableName = DatabaseSchemata.GetTableName(expression.Expression.Type);
                result.TableAlias = baseQuery.Tables.Find(expression.Expression.Type, baseQuery.Id).Alias;
            }
            else
            {
                // will be from a foreign key, not the base type
                result.ColumnName = columnAttribute != null ? columnAttribute.Name : expression.Member.Name;
                result.TableName = ((MemberExpression)expression.Expression).Member.Name;
                result.TableAlias = baseQuery.Tables.FindByPropertyName(((MemberExpression)expression.Expression).Member.Name, baseQuery.Id).Alias;
            }
        }

        private static void LoadColumnAndTableName(MethodCallExpression expression, WhereResolutionPart result, IExpressionQueryResolvable baseQuery, string viewId)
        {
            LoadColumnAndTableName(expression.Object as MemberExpression, result, baseQuery, viewId);
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
