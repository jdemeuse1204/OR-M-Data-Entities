using OR_M_Data_Entities.Lite.Data;
using OR_M_Data_Entities.Lite.Expressions.Query;
using OR_M_Data_Entities.Lite.Expressions.Utilities;
using OR_M_Data_Entities.Lite.Mapping.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace OR_M_Data_Entities.Lite.Expressions.Resolvers
{
    internal class WhereExpressionResolver
    {
        private readonly List<SqlParameter> parameters;
        private readonly IReadOnlyDictionary<Type, TableSchema> tables;
        private readonly Type baseType;

        public WhereExpressionResolver(Type baseType, IReadOnlyDictionary<Type, TableSchema> tables)
        {
            parameters = new List<SqlParameter>();
            this.tables = tables;
            this.baseType = baseType;
        }

        public ResolvedWhereExpression Resolve<T>(Expression<Func<T, bool>> expression)
        {
            // send in a new block number
            // have a list of blocks
            var blocks = new List<ResolvedExpressionBlock>();
            Entrance(expression.Body as dynamic, blocks);
            var sql = new StringBuilder();

            sql.Append("\tWhere\r\n\t(");

            for (var i = 0; i < blocks.Count; i++)
            {
                var currentBlock = blocks[i];
                var nextBlock = i < (blocks.Count - 1) ? blocks[i + 1] : null;

                sql.Append(currentBlock.Clause);

                if (nextBlock == null)
                {
                    sql.Append(")");
                }
                else if (nextBlock.BlockId != currentBlock.BlockId)
                {
                    sql.Append(") Or (");
                }
                else
                {
                    sql.Append(" And ");
                }
            }

            return new ResolvedWhereExpression(sql.ToString(), parameters);
        }

        #region entrance methods
        private void Entrance(BinaryExpression expression, List<ResolvedExpressionBlock> blocks)
        {
            // possibly more than one eval
            var currentBlockId = 0;
            //IsTerminatingExpression
            // break down query into blocks

            if (ExpressionUtilities.IsTerminatingExpressionType(expression.NodeType))
            {
                Evaluate(expression, blocks, currentBlockId, false);
            }
            else
            {
                Expression nextExpression = expression.Left;
                Evaluate(expression.Right as dynamic, blocks, currentBlockId, false);

                if (expression.NodeType == ExpressionType.OrElse)
                {
                    currentBlockId++;
                }

                while (ExpressionUtilities.IsBinaryNodeExpressionType(nextExpression.NodeType))
                {
                    var binary = ((BinaryExpression)nextExpression);
                    nextExpression = binary.Left;

                    Evaluate(binary.Right as dynamic, blocks, currentBlockId, false);

                    if (binary.NodeType == ExpressionType.OrElse)
                    {
                        currentBlockId++;
                    }
                }

                Evaluate(nextExpression as dynamic, blocks, currentBlockId, false);
            }
        }

        private void Entrance(MethodCallExpression expression, List<ResolvedExpressionBlock> blocks)
        {
            Evaluate(expression, blocks, 0, false);
        }

        private void Entrance(UnaryExpression expression, List<ResolvedExpressionBlock> blocks)
        {
            Evaluate(expression.Operand as dynamic, blocks, 0, expression.NodeType == ExpressionType.Not);
        }
        #endregion

        #region evaluation methods
        private void Evaluate(MethodCallExpression expression, List<ResolvedExpressionBlock> blocks, int blockId, bool invertComparison)
        {
            var block = GetBlock(expression, blockId, invertComparison);

            blocks.Add(block);
        }

        private void Evaluate(ParameterExpression expression, List<ResolvedExpressionBlock> blocks, int blockId, bool invertComparison)
        {
            throw new NotImplementedException("No implementation for Parameter Expression");
        }

        private void Evaluate(MemberInitExpression expression, List<ResolvedExpressionBlock> blocks, int blockId, bool invertComparison)
        {
            throw new NotImplementedException("No implementation for Member Init Expression");
        }

        private void Evaluate(NewExpression expression, List<ResolvedExpressionBlock> blocks, int blockId, bool invertComparison)
        {
            throw new NotImplementedException("No implementation for New Expression");
        }

        private void Evaluate(BinaryExpression expression, List<ResolvedExpressionBlock> blocks, int blockId, bool invertComparison)
        {
            // should be a terminating expression if we are here
            if (expression.NodeType == ExpressionType.Equal || expression.NodeType == ExpressionType.NotEqual)
            {
                var block = GetBlock(expression, blockId, false);
                blocks.Add(block);
                return;
            }
        }

        private void Evaluate(UnaryExpression expression, List<ResolvedExpressionBlock> blocks, int blockId, bool invertComparison)
        {
            if (expression.NodeType == ExpressionType.Not)
            {
                Evaluate(expression.Operand as dynamic, blocks, blockId, true);
                return;
            }
        }
        #endregion

        private ResolvedExpressionBlock GetBlock(BinaryExpression expression, int blockId, bool invertComparison)
        {
            if (expression.NodeType == ExpressionType.Equal || expression.NodeType == ExpressionType.NotEqual)
            {

                // Right = Constant
                // Left = Member
                var value = GetValue(expression.Right as dynamic);
                var parametersSql = $"{CreateAndAddParameter(value)}";
                var comparison = expression.NodeType == ExpressionType.NotEqual || invertComparison ? "!=" : "=";

                return new ResolvedExpressionBlock
                {
                    BlockId = blockId,
                    Clause = $"{GetTableAndColumnName(expression.Left as dynamic)} {comparison} {parametersSql}"
                };
            }

            throw new NotImplementedException($"No implementation for method: {expression.Method.Name}");
        }

        private ResolvedExpressionBlock GetBlock(MethodCallExpression expression, int blockId, bool invertComparison)
        {
            if (string.Equals(expression.Method.Name, "Contains", StringComparison.OrdinalIgnoreCase))
            {
                var list = Compile(expression.Object);
                var parametersSql = new StringBuilder();
                var comparison = invertComparison ? "Not In" : "In";

                foreach (var item in ((ICollection)list))
                {
                    parametersSql.Append($"{CreateAndAddParameter(item)},");
                }

                return new ResolvedExpressionBlock
                {
                    BlockId = blockId,
                    Clause = $"{GetTableAndColumnName(expression.Arguments[0] as dynamic)} {comparison} ({parametersSql.ToString().TrimEnd(',')})"
                };
            }

            throw new NotImplementedException($"No implementation for method: {expression.Method.Name}");
        }

        private object GetValue(ConstantExpression expression)
        {
            return expression.Value;
        }

        private string GetTableAndColumnName(MemberExpression member)
        {
            var table = tables[member.Expression.Type];
            var column = table.Columns.First(w => w.PropertyName == member.Member.Name);
            var record = ObjectReader.GetFuzzyObjectRecord(this.baseType, member.Expression.Type);

            return $"[{SqlCreator.GetTableAlias(record.LevelId)}].[{column.ColumnName}]";
        }

        private static object Compile(Expression expression)
        {
            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            var value = getter();

            return value;
        }

        private string CreateAndAddParameter(object value)
        {
            var key = $"@PARAM{this.parameters.Count}";
            this.parameters.Add(new SqlParameter(key, value));
            return key;
        }
    }
}
