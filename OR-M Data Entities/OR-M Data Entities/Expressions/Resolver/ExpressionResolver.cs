/*
 * OR-M Data Entities v1.0.0
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
using OR_M_Data_Entities.Commands.Transform;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Evaluation;
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
            if (ExpressionEvaluator.HasLeft(expression))
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

		/// <summary>
		/// Gets the column name, table name, and transform type
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="tableNameLookup"></param>
		/// <param name="transformType"></param>
		/// <returns></returns>
		private static ExpressionSelectResult _getColumnAndTableName(object expression, Dictionary<string, string> tableNameLookup, SqlDbType transformType, bool isCasting = false, bool isConverting = false, int conversionStyle = 0)
		{
			if (expression is UnaryExpression)
			{
				return _getColumnAndTableName(((UnaryExpression)expression).Operand, tableNameLookup, transformType, isCasting, isConverting, conversionStyle);
			}

			if (expression is MethodCallExpression)
			{
				var dataTransform = SqlDbType.VarChar;
				var conversionNumber = 0;
                var cast = ExpressionEvaluator.IsCasting(expression);
                var convert = ExpressionEvaluator.IsConverting(expression);

				if (cast || convert)
				{
                    dataTransform = ExpressionEvaluator.GetTransformType(expression as dynamic);

					if (convert)
					{
						conversionNumber = Convert.ToInt32(((dynamic)((MethodCallExpression)expression).Arguments[2]).Value);
					}
				}

				return _getColumnAndTableName(((MethodCallExpression)expression).Object ?? ((MethodCallExpression)expression).Arguments.FirstOrDefault(w => w.NodeType == ExpressionType.MemberAccess || w.NodeType == ExpressionType.Convert), tableNameLookup, dataTransform, cast, convert, conversionNumber);
			}

			var e = expression as MemberExpression;

			if (e == null) throw new Exception("Expression cannot be null");

			return e.Expression.NodeType == ExpressionType.Parameter
				? (new ExpressionSelectResult
				{
					TableName = tableNameLookup.ContainsKey(((dynamic)e.Expression).Name) ? tableNameLookup[((dynamic)e.Expression).Name] : ((dynamic)e.Expression).Name,
					ColumnName = e.Member.GetCustomAttribute<ColumnAttribute>() == null
							? e.Member.Name
							: e.Member.GetCustomAttribute<ColumnAttribute>().Name,
					Transform = isCasting || isConverting ? transformType : e.Member.GetCustomAttribute<DbTranslationAttribute>() == null ?
							ExpressionTypeTransform.GetSqlDbType(e.Type)
							: e.Member.GetCustomAttribute<DbTranslationAttribute>().Type,
					ShouldCast = isCasting,
					ColumnType = e.Type,
					ShouldConvert = isConverting,
					ConversionStyle = conversionStyle
				})
				: _getColumnAndTableName(e.Expression as MemberExpression, tableNameLookup, transformType, isCasting, isConverting, conversionStyle);
		}

		/// <summary>
		/// checks to see if a expression has a lambda parameter or not
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		private static bool _hasParameter(object expression)
		{
			if (expression == null)
			{
				return false;
			}

			if (((Expression)expression).NodeType == ExpressionType.Parameter)
			{
				return true;
			}

			if (expression is ConstantExpression)
			{
				return false;
			}

			if (expression is UnaryExpression)
			{
				return _hasParameter(((UnaryExpression)expression).Operand);
			}

			if (expression is MethodCallExpression)
			{
				return _hasParameter(((MethodCallExpression)expression).Object ?? ((MethodCallExpression)expression).Arguments[0]);
			}

			return _hasParameter(((MemberExpression)expression).Expression);
		}
		#endregion

		#region Expression Evaluation
		/// <summary>
		/// Evaulates a method call expression
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="tableNameLookup"></param>
		/// <returns></returns>
		private static ExpressionWhereResult _evaluate(MethodCallExpression expression, Dictionary<string, string> tableNameLookup)
		{
			var result = new ExpressionWhereResult();
			var columnOptions = _getColumnAndTableName(expression.Arguments[0] is MemberExpression ? expression.Arguments[0] as dynamic : expression.Object as dynamic, tableNameLookup, SqlDbType.VarChar);

			result.ColumnName = columnOptions.ColumnName;
			result.TableName = columnOptions.TableName;
			result.CompareValue = _getValue((!(expression.Arguments[0] is MemberExpression)) ? expression.Arguments[0] as dynamic : expression.Object as dynamic);
            result.ComparisonType = ExpressionEvaluator.GetComparisonType(expression.Method.Name);
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
			var leftSide = expression.Left;
			var rightSide = expression.Right;

			// Check each side for lambda parameters.  Both sides can have the parameter
			var leftSideHasParameter = _hasParameter(leftSide);
			var rightSideHasParameter = _hasParameter(rightSide);

			// Get column options (Column Name, Table Name, Transform Type)
			var columnOptions = _getColumnAndTableName(leftSideHasParameter ? leftSide : rightSide, tableNameLookup, SqlDbType.VarChar);

			result.ColumnName = columnOptions.ColumnName;
			result.TableName = columnOptions.TableName;
			result.Transform = columnOptions.Transform;

			// cast as dynamic so runtime can choose which method to use
			result.CompareValue = rightSideHasParameter ? _getColumnAndTableName(rightSide, tableNameLookup, SqlDbType.VarChar) : _getValue(leftSideHasParameter ? rightSide as dynamic : leftSide as dynamic);
            result.ComparisonType = ExpressionEvaluator.GetComparisonType(expression.NodeType);
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

            result.ComparisonType = ExpressionEvaluator.GetComparisonType(expression.NodeType);

			return result;
		}
		#endregion

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
            var isCasting = ExpressionEvaluator.IsCasting(expression);

			var objectMember = Expression.Convert(expression, typeof(object));

			var getterLambda = Expression.Lambda<Func<object>>(objectMember);

			var getter = getterLambda.Compile();

			var result = getter();

			if (!isCasting)
			{
				return result;
			}

            var transform = ExpressionEvaluator.GetTransformType(expression);

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
	}
}
