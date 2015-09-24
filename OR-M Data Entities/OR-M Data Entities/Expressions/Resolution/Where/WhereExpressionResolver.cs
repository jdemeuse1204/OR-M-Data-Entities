/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities.Commands.Transform;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Containers;
using OR_M_Data_Entities.Extensions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Expressions.Resolution.Where
{
    public class WhereExpressionResolver : ExpressionResolver
    {
        public static void Resolve<T>(Expression<Func<T, bool>> expression, WhereResolutionContainer container, IExpressionQueryResolvable baseQuery, string viewId)
        {
            _evaluateTree(expression.Body as dynamic, container, baseQuery, viewId, expression.Body.NodeType);
        }

        public static void ResolveFind<T>(object[] pks, WhereResolutionContainer container, IExpressionQueryResolvable baseQuery)
        {
            var table = baseQuery.Tables.Find(typeof(T), baseQuery.Id);
            var infos =
                baseQuery.SelectInfos.Where(w => w.IsPrimaryKey && w.Table.Type == typeof(T))
                    .OrderBy(w => w.Ordinal)
                    .ToList();

            for (var i = 0; i < pks.Count(); i++)
            {
                var isConnector = (i % 2) != 0;

                if (isConnector)
                {
                    container.AddConnector(ConnectorType.And);
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

        private static void _evaluateTree(MethodCallExpression expression, WhereResolutionContainer container, IExpressionQueryResolvable baseQuery, string viewId, ExpressionType? expressionType = null)
        {
            var result = _evaluate(expression, container, baseQuery, viewId, container.NextGroupNumber(), expressionType);

            container.AddResolution(result);
        }

        private static void _evaluateTree(UnaryExpression expression, WhereResolutionContainer container, IExpressionQueryResolvable baseQuery, string viewId, ExpressionType? expressionType = null)
        {
            var memberExpresssion = expression.Operand as MemberExpression;

            if (memberExpresssion == null)
            {
                _evaluateTree(expression.Operand as dynamic, container, baseQuery, viewId, expressionType);
                return;
            }

            var result = _evaluate(memberExpresssion, container, baseQuery, viewId, container.NextGroupNumber(), expressionType);

            container.AddResolution(result);
        }

        // need to evaluate groupings
        private static void _evaluateTree(BinaryExpression expression, WhereResolutionContainer container, IExpressionQueryResolvable baseQuery, string viewId, ExpressionType? expressionType = null)
        {
            // check to see if we are at the end of the expression
            if (!HasComparison(expression))
            {
                var result = _evaluate(expression as dynamic, container, baseQuery, viewId, container.NextGroupNumber(), expressionType);

                container.AddResolution(result);
                return;
            }

            var count = 0;
            var wasRightAdded = false;

            while (true)
            {
                // if the right has a left then its a group expression
                if (IsAnd(expression.Right))
                {
                    if (count != 0) container.AddConnector(ConnectorType.Or);

                    _evaluateGroup(expression.Right as BinaryExpression, container, baseQuery, viewId);
                }
                else
                {
                    if (count != 0) container.AddConnector(ConnectorType.Or);

                    var result = _evaluate(expression.Right as dynamic, container, baseQuery, viewId, container.NextGroupNumber(), expressionType);

                    container.AddResolution(result);
                    wasRightAdded = true;
                }

                // check to see if the end is a group
                if (IsAnd(expression.Left))
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

                    var result = _evaluate(expression.Left as dynamic, container, baseQuery, viewId, container.NextGroupNumber(), expressionType);

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
                if (IsAnd(expression.Right) || IsOr(expression.Right))
                {
                    _evaluateGroup(expression.Right as BinaryExpression, resolution, baseQuery, viewId);
                }
                else
                {
                    var rightResult = _evaluate(expression.Right as dynamic, resolution, baseQuery, viewId);

                    if (rightResult != null)
                    {
                        rightResult.Group = groupNumber;
                    }

                    resolution.AddResolution(rightResult);
                    resolution.AddConnector(GetConnectorType(expression.NodeType));

                    if (HasComparison(expression.Left))
                    {
                        expression = expression.Left as BinaryExpression;
                        continue;
                    }
                }

                if (IsAnd(expression.Left) || IsOr(expression.Left))
                {
                    _evaluateGroup(expression.Left as BinaryExpression, resolution, baseQuery, viewId);
                }
                else
                {
                    var leftResult = _evaluate(expression.Left as dynamic, resolution, baseQuery, viewId);

                    leftResult.Group = groupNumber;

                    resolution.AddResolution(leftResult);

                    if (IsOr(expression)) resolution.AddConnector(GetConnectorType(expression.NodeType));
                }

                break;
            }
        }


        /// <summary>
        /// Happens when the method uses an operator
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="resolution"></param>
        private static WhereResolutionPart _evaluate(BinaryExpression expression, WhereResolutionContainer resolution, IExpressionQueryResolvable baseQuery, string viewId, int group = -1, ExpressionType? expressionType = null)
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

            var rigthMethodCallTransformExpression = expression.Right as MethodCallExpression;
            var leftMethodCallTransformExpression = expression.Left as MethodCallExpression;

            // casting and converting
            if (rigthMethodCallTransformExpression != null && rigthMethodCallTransformExpression.Method.DeclaringType == typeof(DbTransform))
            {
                var isConverting =
                    rigthMethodCallTransformExpression.Method.DeclaringType == typeof (DbTransform) &&
                    rigthMethodCallTransformExpression.Method.Name == "Convert";
                var type = (SqlDbType?)GetValue(rigthMethodCallTransformExpression.Arguments[isConverting ? 0 : 1] as dynamic, baseQuery);

                (isParameterOnLeftSide ? result.Transform : ((SqlDbParameter)result.CompareValue).Transform).CastType = rigthMethodCallTransformExpression.Method.Name == "Cast" ? type : null;
                (isParameterOnLeftSide ? result.Transform : ((SqlDbParameter)result.CompareValue).Transform).ConvertStyle = rigthMethodCallTransformExpression.Method.Name == "Convert" ? (int?)GetValue(rigthMethodCallTransformExpression.Arguments[2] as dynamic, baseQuery) : null;
                (isParameterOnLeftSide ? result.Transform : ((SqlDbParameter)result.CompareValue).Transform).ConvertType = rigthMethodCallTransformExpression.Method.Name == "Convert" ? type : null;
            }

            if (leftMethodCallTransformExpression != null && leftMethodCallTransformExpression.Method.DeclaringType == typeof(DbTransform))
            {
                var isConverting =
                    leftMethodCallTransformExpression.Method.DeclaringType == typeof(DbTransform) &&
                    leftMethodCallTransformExpression.Method.Name == "Convert";
                var type = (SqlDbType?)GetValue(leftMethodCallTransformExpression.Arguments[isConverting ? 0 : 1] as dynamic, baseQuery);

                (!isParameterOnLeftSide ? result.Transform : ((SqlDbParameter)result.CompareValue).Transform).CastType = leftMethodCallTransformExpression.Method.Name == "Cast" ? type : null;
                (!isParameterOnLeftSide ? result.Transform : ((SqlDbParameter)result.CompareValue).Transform).ConvertStyle = leftMethodCallTransformExpression.Method.Name == "Convert" ? (int?)GetValue(leftMethodCallTransformExpression.Arguments[2] as dynamic, baseQuery) : null;
                (!isParameterOnLeftSide ? result.Transform : ((SqlDbParameter)result.CompareValue).Transform).ConvertType = leftMethodCallTransformExpression.Method.Name == "Convert" ? type : null;
            }

            // get the comparison tyoe
            LoadComparisonType(expression, result);

            // add the result to the list
            return result;
        }

        private static WhereResolutionPart _evaluateLinqSubQuery(MethodCallExpression expression,
            WhereResolutionContainer resolution, IExpressionQueryResolvable baseQuery, string viewId, int group = -1,
            ExpressionType? expressionType = null)
        {
            if (expression.Method.Name.ToUpper() == "ANY" && expression.Arguments.Count == 1)
            {
                var e = (MemberExpression)expression.Arguments[0];
                var table = baseQuery.Tables.FindByPropertyName(e.Member.Name,
                    baseQuery.Id);
                var baseTable = baseQuery.Tables.Find(e.Expression.Type, baseQuery.Id);

                var foreignKey = e.Expression.Type.FindForeignKeyAttribute(table.ForeignKeyPropertyName);

                // always comes from a list
                var joinString = string.Format("[{0}].[{1}] = [{2}].[{3}]", table.Name, foreignKey.ForeignKeyColumnName,
                    baseTable.Alias, e.Expression.Type.GetPrimaryKeyNames()[0]);

                var result = new WhereResolutionPart
                {
                    Group = @group,
                    ExpressionQueryId = baseQuery.Id,
                    CompareValue =
                        new SqlExistsString(expressionType.HasValue && expressionType.Value == ExpressionType.Not, joinString, table.Name)
                };

                return result;
            }

            for (var i = 1; i < expression.Arguments.Count; i++)
            {
                var argument = expression.Arguments[i];

                var lambdaExpression = (LambdaExpression)argument;

                if (lambdaExpression.Body is BinaryExpression)
                {
                    _evaluateTree(lambdaExpression.Body as BinaryExpression, resolution, baseQuery, viewId);
                    return null;
                }

                if (lambdaExpression.Body is UnaryExpression)
                {
                    _evaluateTree(lambdaExpression.Body as UnaryExpression, resolution, baseQuery, viewId, expressionType);
                    return null;
                }

                // evaluate the argument
                var result = _evaluate(lambdaExpression.Body as dynamic, resolution, baseQuery, viewId, group);

                if (result != null)
                {
                    result.Group = group;

                    result.ExpressionQueryId = baseQuery.Id;
                }

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
                return expression.Arguments.Any(
                    w =>
                        w is MethodCallExpression &&
                        ((MethodCallExpression)w).Method.DeclaringType == typeof(DbTransform))
                    ? _evaluateTransform(expression, resolution, baseQuery, viewId, @group, expressionType)
                    : _evaluateLinqSubQuery(expression, resolution, baseQuery, viewId, @group, expressionType);
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

                        foreach (var item in ((ICollection)value))
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
                    result.CompareValue = resolution.GetAndAddParameter(string.Format("%{0}", value));
                    break;
            }

            return result;
        }

        private static WhereResolutionPart _evaluate(UnaryExpression expression, WhereResolutionContainer resolution, IExpressionQueryResolvable baseQuery, string viewId, int group = -1, ExpressionType? expressionType = null)
        {
            return _evaluate(expression.Operand as dynamic, resolution, baseQuery, viewId, group, expression.NodeType);
        }

        private static WhereResolutionPart _evaluate(MemberExpression expression, WhereResolutionContainer resolution, IExpressionQueryResolvable baseQuery, string viewId, int group = -1, ExpressionType? expressionType = null)
        {
            // bit comparison
            var result = new WhereResolutionPart
            {
                Group = group
            };
            
            LoadColumnAndTableName(expression as dynamic, result, baseQuery, viewId);

            var value = expressionType.HasValue && expressionType.Value == ExpressionType.Not ? 0 : 1;

            result.Comparison = CompareType.Equals;
            result.CompareValue = resolution.GetAndAddParameter(value);

            return result;
        }

        private static WhereResolutionPart _evaluateTransform(MethodCallExpression expression, WhereResolutionContainer resolution, IExpressionQueryResolvable baseQuery, string viewId, int group = -1, ExpressionType? expressionType = null)
        {
            var result = new WhereResolutionPart
            {
                Group = group
            };

            var isParameterOnLeftSide = HasParameter(expression.Arguments[1] as dynamic);
            var isConverting =
                expression.Arguments.Any(
                    w =>
                        w is MethodCallExpression &&
                        ((MethodCallExpression)w).Method.DeclaringType == typeof(DbTransform) &&
                        ((MethodCallExpression)w).Method.Name == "Convert");

            object value;

            if (!isParameterOnLeftSide)
            {
                LoadColumnAndTableName(((MethodCallExpression)expression.Arguments[0]).Arguments[isConverting ? 1 : 0] as dynamic, result, baseQuery, viewId);

                value = GetValue(expression.Arguments[1] as dynamic, baseQuery);
            }
            else
            {
                LoadColumnAndTableName(((MethodCallExpression)expression.Arguments[1]).Arguments[isConverting ? 1 : 0] as dynamic, result, baseQuery, viewId);

                value = GetValue(expression.Arguments[0] as dynamic, baseQuery);
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

                        foreach (var item in ((ICollection)value))
                        {
                            ((List<SqlDbParameter>)result.CompareValue).Add(resolution.GetAndAddParameter(string.Format("{0}", item)));
                        }
                    }
                    return result;
                case "STARTSWITH":
                    result.Comparison = invertComparison ? CompareType.NotLike : CompareType.Like;
                    result.CompareValue = resolution.GetAndAddParameter(string.Format("{0}%", value));
                    break;
                case "ENDSWITH":
                    result.Comparison = invertComparison ? CompareType.NotLike : CompareType.Like;
                    result.CompareValue = resolution.GetAndAddParameter(string.Format("%{0}", value));
                    break;


                case "GREATERTHAN":
                    result.Comparison = CompareType.GreaterThan;
                    result.InvertComparison = invertComparison;
                    result.CompareValue = resolution.GetAndAddParameter(value);
                    break;
                case "GREATERTHANOREQUAL":
                    result.Comparison = CompareType.GreaterThanEquals;
                    result.InvertComparison = invertComparison;
                    result.CompareValue = resolution.GetAndAddParameter(value);
                    break;
                case "LESSTHAN":
                    result.Comparison = CompareType.LessThan;
                    result.InvertComparison = invertComparison;
                    result.CompareValue = resolution.GetAndAddParameter(value);
                    break;
                case "LESSTHANOREQUAL":
                    result.Comparison = CompareType.LessThanEquals;
                    result.InvertComparison = invertComparison;
                    result.CompareValue = resolution.GetAndAddParameter(value);
                    break;
            }

            var firstArgMethodCallExpression = expression.Arguments[0] as MethodCallExpression;
            var secondArgMethodCallExpression = expression.Arguments[1] as MethodCallExpression;

            if (firstArgMethodCallExpression != null &&
                firstArgMethodCallExpression.Method.DeclaringType == typeof(DbTransform))
            {
                var type = (SqlDbType?)((ConstantExpression)firstArgMethodCallExpression.Arguments[isConverting ? 0 : 1]).Value;

                ((SqlDbParameter)result.CompareValue).Transform.CastType = firstArgMethodCallExpression.Method.Name == "Cast" ? type : null;
                ((SqlDbParameter)result.CompareValue).Transform.ConvertStyle = firstArgMethodCallExpression.Method.Name == "Convert" ? (int?)GetValue(firstArgMethodCallExpression.Arguments[2] as dynamic, baseQuery) : null;
                ((SqlDbParameter)result.CompareValue).Transform.ConvertType = firstArgMethodCallExpression.Method.Name == "Convert" ? type : null;
            }

            if (secondArgMethodCallExpression != null &&
               secondArgMethodCallExpression.Method.DeclaringType == typeof(DbTransform))
            {
                var type = (SqlDbType?)((ConstantExpression)secondArgMethodCallExpression.Arguments[isConverting ? 0 : 1]).Value;

                result.Transform.CastType = secondArgMethodCallExpression.Method.Name == "Cast" ? type : null; ;
                result.Transform.ConvertStyle = secondArgMethodCallExpression.Method.Name == "Convert" ? (int?)GetValue(secondArgMethodCallExpression.Arguments[2] as dynamic, baseQuery) : null;
                result.Transform.ConvertType = secondArgMethodCallExpression.Method.Name == "Convert" ? type : null;
            }

            return result;
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
            if (!string.IsNullOrWhiteSpace(viewId) && !expression.Expression.Type.IsPartOfView(viewId))
            {
                throw new ViewException(string.Format("Type Of {0} Does not contain attribute for View - {1}",
                    expression.Expression.Type, viewId));
            }

            var columnAttribute = expression.Member.GetCustomAttribute<ColumnAttribute>();

            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                // cannot come from a foriegn key, is the base type
                result.ColumnName = columnAttribute != null ? columnAttribute.Name : expression.Member.Name;
                result.TableName = expression.Expression.Type.GetTableName();
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
            if (expression.Object == null)
            {
                LoadColumnAndTableName(expression.Arguments.First(w => HasParameter(w as dynamic)) as dynamic,
                    result, baseQuery, viewId);
                return;
            }

            LoadColumnAndTableName(expression.Object as MemberExpression, result, baseQuery, viewId);
        }

        private static void LoadColumnAndTableName(UnaryExpression expression, WhereResolutionPart result, IExpressionQueryResolvable baseQuery, string viewId)
        {
            LoadColumnAndTableName(expression.Operand as MemberExpression, result, baseQuery, viewId);
        }
        #endregion

        private static bool HasComparison(Expression expression)
        {
            return expression.NodeType == ExpressionType.And
                || expression.NodeType == ExpressionType.AndAlso
                || expression.NodeType == ExpressionType.Or
                || expression.NodeType == ExpressionType.OrElse;
        }

        private static bool IsAnd(Expression expression)
        {
            var binaryExpression = expression as BinaryExpression;

            return binaryExpression != null &&
                   (binaryExpression.NodeType == ExpressionType.And ||
                    binaryExpression.NodeType == ExpressionType.AndAlso);
        }

        private static bool IsOr(Expression expression)
        {
            var binaryExpression = expression as BinaryExpression;

            return binaryExpression != null &&
                   (binaryExpression.NodeType == ExpressionType.Or ||
                    binaryExpression.NodeType == ExpressionType.OrElse);
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

        private static ConnectorType GetConnectorType(ExpressionType expressionType)
        {
            if (expressionType == ExpressionType.And || expressionType == ExpressionType.AndAlso)
            {
                return ConnectorType.And;
            }

            if (expressionType == ExpressionType.Or || expressionType == ExpressionType.OrElse)
            {
                return ConnectorType.Or;
            }
                
            throw new Exception("Cannot get connector");
        }
    }
}
