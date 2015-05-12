using System;
using System.Linq;
using System.Linq.Expressions;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Containers;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class WhereExpressionResolver<T> : ExpressionResolver
    {
        public WhereExpressionResolver(DbQuery query)
            : base(query)
        {
        }

        public void Resolve(Expression<Func<T, bool>> expression)
        {
            _evaluateTree(expression.Body as BinaryExpression);
        }

        // need to evaluate groupings
        private void _evaluateTree(BinaryExpression expression)
        {
            // check to see if we are at the end of the expression
            if (!HasComparison(expression))
            {
                var result = _evaluate(expression as dynamic, WhereResolution, WhereResolution.NextGroupNumber());

                WhereResolution.AddResolution(result);
                return;
            }

            var count = 0;

            while (true)
            {
                // if the right has a left then its a group expression
                if (IsGroup(expression.Right))
                {
                    if (count != 0) WhereResolution.AddConnector(SqlConnector.Or);

                    _evaluateGroup(expression.Right as BinaryExpression, WhereResolution);
                }
                else
                {
                    if (count != 0) WhereResolution.AddConnector(SqlConnector.Or);

                    var result = _evaluate(expression.Right as dynamic, WhereResolution, WhereResolution.NextGroupNumber());

                    WhereResolution.AddResolution(result);
                }

                // check to see if the end is a group
                if (IsGroup(expression.Left))
                {
                    if (count != 0) WhereResolution.AddConnector(SqlConnector.Or);

                    _evaluateGroup(expression.Left as BinaryExpression, WhereResolution);
                    break;
                }

                // check to see if we are at the end of the expression
                if (!HasComparison(expression.Left))
                {
                    if (count != 0) WhereResolution.AddConnector(SqlConnector.Or);

                    var result = _evaluate(expression.Left as dynamic, WhereResolution, WhereResolution.NextGroupNumber());

                    WhereResolution.AddResolution(result);
                    break;
                }

                expression = expression.Left as BinaryExpression;
                count++;
            }
        }

        #region Evaluate

        private void _evaluateGroup(BinaryExpression expression, WhereResolutionContainer resolution)
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
        private LambdaResolution _evaluate(BinaryExpression expression, WhereResolutionContainer resolution, int group = -1)
        {
            var result = new LambdaResolution
            {
                Group = group,
            };
            var isParameterOnLeftSide = HasParameter(expression.Left as dynamic);

            // load the table and column name into the result
            LoadColumnAndTableName((isParameterOnLeftSide ? expression.Left : expression.Right) as dynamic, result);

            // get the value from the expression
            var value = GetValue((isParameterOnLeftSide ? expression.Right : expression.Left) as dynamic);

            result.CompareValue = IsExpressionQuery(value.GetType()) ? value : resolution.AddParameter(value);

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
        private LambdaResolution _evaluate(MethodCallExpression expression, WhereResolutionContainer resolution, int group = -1, ExpressionType? expressionType = null)
        {
            var result = new LambdaResolution
            {
                Group = group
            };
            var isParameterOnLeftSide = HasParameter(expression.Object as dynamic);

            if (isParameterOnLeftSide)
            {
                LoadColumnAndTableName(expression.Object as dynamic, result);

                var value = GetValue(expression.Arguments[0] as dynamic);
                // FIX ME COMPARE IS WRONG!
                result.CompareValue = IsExpressionQuery(value.GetType()) ? value : resolution.AddParameter(value);
            }
            else
            {
                LoadColumnAndTableName(expression.Arguments[0] as dynamic, result);

                GetValue(expression.Object as dynamic);
            }

            var invertComparison = expressionType.HasValue && expressionType.Value == ExpressionType.Not;

            switch (expression.Method.Name.ToUpper())
            {
                case "EQUALS":
                case "OP_EQUALITY":
                    result.Comparison = invertComparison ? CompareType.NotEqual : CompareType.Equals;
                    break;
                case "CONTAINS":
                    result.Comparison = result.CompareValue.IsList()
                        ? (invertComparison ? CompareType.NotIn : CompareType.In)
                        : (invertComparison ? CompareType.NotLike : CompareType.Like);

                    if (!result.CompareValue.IsList())
                    {
                        result.CompareValue = string.Format("%{0}%", result.CompareValue);
                    }
                    break;
                case "STARTSWITH":
                    result.Comparison = invertComparison ? CompareType.NotLike : CompareType.Like;
                    result.CompareValue = string.Format("{0}%", result.CompareValue);
                    break;
                case "ENDSWITH":
                    result.Comparison = invertComparison ? CompareType.NotLike : CompareType.Like;
                    result.CompareValue = string.Format("%{0}", result.CompareValue);
                    break;
            }

            return result;
        }


        private LambdaResolution _evaluate(UnaryExpression expression, WhereResolutionContainer resolution, int group = -1)
        {
            return _evaluate(expression.Operand as dynamic, resolution, group, expression.NodeType);
        }
        #endregion

        #region Load Comparison Type
        private void LoadComparisonType(BinaryExpression expression, LambdaResolution resolution)
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
        private void LoadColumnAndTableName(MemberExpression expression, LambdaResolution result)
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

        private void LoadColumnAndTableName(MethodCallExpression expression, LambdaResolution result)
        {
            LoadColumnAndTableName(expression.Object as MemberExpression, result);
        }
        #endregion

        private bool HasComparison(Expression expression)
        {
            return expression.NodeType == ExpressionType.And
                || expression.NodeType == ExpressionType.AndAlso
                || expression.NodeType == ExpressionType.Or
                || expression.NodeType == ExpressionType.OrElse;
        }

        private bool IsGroup(Expression expression)
        {
            var binaryExpression = expression as BinaryExpression;

            return binaryExpression != null &&
                   (binaryExpression.NodeType == ExpressionType.And ||
                    binaryExpression.NodeType == ExpressionType.AndAlso);
        }

        private bool HasParameter(ConstantExpression expression)
        {
            return false;
        }

        private bool HasParameter(UnaryExpression expression)
        {
            return expression == null ? false : HasParameter(expression.Operand as dynamic);
        }

        private bool HasParameter(ParameterExpression expression)
        {
            return true;
        }

        private bool HasParameter(MemberExpression expression)
        {
            return HasParameter(expression.Expression as dynamic);
        }

        private bool HasParameter(MethodCallExpression expression)
        {
            var e = expression.Object;

            return e != null
                ? HasParameter(expression.Object as dynamic)
                : expression.Arguments.Select(arg => HasParameter(arg as dynamic)).Any(hasParameter => hasParameter);
        }
    }
}
