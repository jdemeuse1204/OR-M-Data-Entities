using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Containers;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class WhereExpressionResolver<T> : IResolver<T>
    {
        public static void Resolve(Expression<Func<T, bool>> expression, ExpressionQuery<T> expressionQuery)
        {
            _evaluateTree(expression.Body as BinaryExpression, expressionQuery.WhereResolution);
        }

        // need to evaluate groupings
        public static void _evaluateTree(BinaryExpression expression, WhereResolutionContainer resolution)
        {
            // check to see if we are at the end of the expression
            if (!HasComparison(expression))
            {
                var result = _evaluate(expression as dynamic, resolution, resolution.NextGroupNumber());

                resolution.AddResolution(result);
                return;
            }

            var count = 0;

            while (true)
            {
                // if the right has a left then its a group expression
                if (IsGroup(expression.Right))
                {
                    if (count != 0) resolution.AddConnector(SqlConnector.Or);

                    _evaluateGroup(expression.Right as BinaryExpression, resolution);
                }
                else
                {
                    if (count != 0) resolution.AddConnector(SqlConnector.Or);

                    var result = _evaluate(expression.Right as dynamic, resolution, resolution.NextGroupNumber());

                    resolution.AddResolution(result);
                }

                // check to see if the end is a group
                if (IsGroup(expression.Left))
                {
                    if (count != 0) resolution.AddConnector(SqlConnector.Or);

                    _evaluateGroup(expression.Left as BinaryExpression, resolution);
                    break;
                }

                // check to see if we are at the end of the expression
                if (!HasComparison(expression.Left))
                {
                    if (count != 0) resolution.AddConnector(SqlConnector.Or);

                    var result = _evaluate(expression.Left as dynamic, resolution, resolution.NextGroupNumber());

                    resolution.AddResolution(result);
                    break;
                }

                expression = expression.Left as BinaryExpression;
                count++;
            }
        }

        #region Evaluate

        protected static void _evaluateGroup(BinaryExpression expression, WhereResolutionContainer resolution)
        {
            var groupNumber = resolution.NextGroupNumber();
            // comparisons are always AND here, will be no groups in here
            while (true)
            {
                var rightResult = _evaluate(expression.Right as dynamic, resolution);

                rightResult.Group = groupNumber;

                resolution.AddResolution(rightResult);
                resolution.AddConnector(SqlConnector.And);

                if (HasComparison(expression.Left))
                {
                    expression = expression.Left as BinaryExpression;
                    continue;
                }

                var leftResult = _evaluate(expression.Left as dynamic, resolution);

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
        protected static LambdaResolution _evaluate(BinaryExpression expression, WhereResolutionContainer resolution, int group = -1)
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
        protected static LambdaResolution _evaluate(MethodCallExpression expression, WhereResolutionContainer resolution, int group = -1)
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
            if (IsSubQuery(expression))
            {
                result.CompareValue = SubQueryResolver.Resolve(expression);
                return;
            }

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


        protected static bool IsGroup(Expression expression)
        {
            var binaryExpression = expression as BinaryExpression;

            return binaryExpression != null &&
                   (binaryExpression.NodeType == ExpressionType.And ||
                    binaryExpression.NodeType == ExpressionType.AndAlso);
        }

        public static bool IsSubQuery(MethodCallExpression expression)
        {
            foreach (var methodCallExpression in expression.Arguments.Select(argument => argument as MethodCallExpression))
            {
                if (!IsExpressionQuery(methodCallExpression))
                {
                    return IsSubQuery(methodCallExpression);
                }
                return true;
            }
            return false;
        }

        protected static bool IsExpressionQuery(MethodCallExpression expression)
        {
            return expression.Method.ReturnType.IsGenericType &&
                   expression.Method.ReturnType.GetGenericTypeDefinition()
                       .IsAssignableFrom(typeof (ExpressionQuery<>));
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

        protected static bool HasParameter(MethodCallExpression expression)
        {
            var e = expression.Object;

            return e != null
                ? HasParameter(expression.Object as dynamic)
                : expression.Arguments.Select(arg => HasParameter(arg as dynamic)).Any(hasParameter => hasParameter);
        }
    }
}
