using OR_M_Data_Entities.Lite.Data;
using OR_M_Data_Entities.Lite.Expressions.Query;
using OR_M_Data_Entities.Lite.Mapping.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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


            return new ResolvedWhereExpression(null, parameters);
        }

        #region entrance methods
        private void Entrance(BinaryExpression expression, List<ResolvedExpressionBlock> blocks)
        {
            // possibly more than one eval
            var expressions = new List<Expression>();
            //IsTerminatingExpression
            // break down query into blocks

        }

        private void Entrance(MethodCallExpression expression, List<ResolvedExpressionBlock> blocks)
        {
            // one eval
            Evaluate(expression, blocks, 0, false);
        }

        private void Entrance(UnaryExpression expression, List<ResolvedExpressionBlock> blocks)
        {
            // one eval
            Evaluate(expression.Operand as dynamic, blocks, 0, expression.NodeType == ExpressionType.Not);
        }
        #endregion

        #region evaluation methods
        private void Evaluate(MethodCallExpression expression, List<ResolvedExpressionBlock> blocks, int blockId, bool invertComparison)
        {
            // add new expression block
            var block = GetBlock(expression, blockId, true);

            blocks.Add(block);
        }

        private void Evaluate(ParameterExpression expression, List<ResolvedExpressionBlock> blocks, int blockId, bool invertComparison)
        {
            
        }

        private void Evaluate(MemberInitExpression expression, List<ResolvedExpressionBlock> blocks, int blockId, bool invertComparison)
        {
          
        }

        private void Evaluate(NewExpression expression, List<ResolvedExpressionBlock> blocks, int blockId, bool invertComparison)
        {
          
        }

        private void Evaluate(BinaryExpression expression, List<ResolvedExpressionBlock> blocks, int blockId, bool invertComparison)
        {

        }

        private void Evaluate(UnaryExpression expression, List<ResolvedExpressionBlock> blocks, int blockId, bool invertComparison)
        {

        }
        #endregion

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

        private string GetTableAndColumnName(MemberExpression member)
        {
            var table = tables[member.Expression.Type];
            var column = table.Columns.First(w => w.PropertyName == member.Member.Name);
            var record = ObjectReader.GetFuzzyObjectRecord(this.baseType, member.Expression.Type);

            //record.LevelId
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

        private bool IsTerminatingExpression(ExpressionType type)
        {
            return type == ExpressionType.Call || 
                type == ExpressionType.Equal || 
                type == ExpressionType.NotEqual ||
                type == ExpressionType.LessThan || 
                type == ExpressionType.LessThanOrEqual || 
                type == ExpressionType.GreaterThan || 
                type == ExpressionType.GreaterThanOrEqual;
        }
    }
}
