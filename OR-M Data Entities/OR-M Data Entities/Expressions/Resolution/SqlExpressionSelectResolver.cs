using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions.Resolution.Base;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class SqlExpressionSelectResolver : SqlExpressionResolver
    {
        public static void Resolve<TSource, TResult>(Expression<Func<TSource, TResult>> selector, SqlQuery query)
        {
            _evaltateExpressionTree(selector.Body, query);
        }

        private static void _evaluateMemberInit(Expression expression, SqlQuery query)
        {
            var memberInitExpression = expression as MemberInitExpression;

            foreach (MemberAssignment binding in memberInitExpression.Bindings)
            {
                if (binding.Expression.NodeType == ExpressionType.New) continue;

                var bindingExpression = binding.Expression as MemberInitExpression;

                if (bindingExpression != null)
                {
                    _evaluateMemberInit(bindingExpression, query);
                    continue;
                }

                var tableAndColumnName = GetTableAndColumnName(binding.Expression);
                var columnSchematic = new SqlColumnSchematic(tableAndColumnName[0], tableAndColumnName[1],
                    ((MemberExpression) binding.Expression).Expression.Type)
                {
                    Alias = binding.Member.Name
                };

                query.Schematic.AddColumnSchematic(columnSchematic);
            }
        }

        private static void _evaltateExpressionTree(Expression expression, SqlQuery query)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.New:
                    var newExpression = expression as NewExpression;

                    for (var i = 0; i < newExpression.Arguments.Count; i++)
                    {
                        var arg = newExpression.Arguments[i] as MemberExpression;

                    }
                    break;
                case ExpressionType.Convert:
                    _evaltateExpressionTree(((UnaryExpression)expression).Operand, query);
                    break;
                case ExpressionType.MemberInit:
                    var memberInitExpression = expression as MemberInitExpression;

                    var sqlColumns = new List<SqlColumnSchematic>();

                    query.Schematic.RemoveAllColumnSchematics();

                    _evaluateMemberInit(memberInitExpression, query);

                    //query.Select = sqlColumns;

                    break;
                case ExpressionType.MemberAccess:
                    var memberAccessExpression = ((MemberExpression)expression);
                    var memberAccessTableName = DatabaseSchemata.GetTableName(memberAccessExpression.Expression.Type);
                    var memberAccessColumnName = DatabaseSchemata.GetColumnName(memberAccessExpression.Member);

                    var sqlColumnSelect = query.Schematic.FindColumnSchematic(memberAccessTableName, memberAccessColumnName, memberAccessExpression.Expression.Type);


                    break;
            }
        }
    }
}
