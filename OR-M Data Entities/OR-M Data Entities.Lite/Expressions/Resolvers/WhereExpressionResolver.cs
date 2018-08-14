using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Lite.Expressions.Resolvers
{
    internal static class WhereExpressionResolver
    {
        private static int blockId;
        // separate the expression into blocks
        public static ResolvedWhereExpression Resolve<T>(Expression<Func<T, bool>> expression)
        {
            blockId = 0;
            var result = new ResolvedWhereExpression();
            Evaluate(expression.Body as dynamic, result);
            return result;
        }

        // should be able to select the parent property and keep passing it through to the next method

        private static void Evaluate(MemberExpression expression, ResolvedWhereExpression result)
        {

        }

        private static void Evaluate(MethodCallExpression expression, ResolvedWhereExpression result)
        {
        }

        private static void Evaluate(ParameterExpression expression, ResolvedWhereExpression result)
        {
            
        }

        private static void Evaluate(MemberInitExpression expression, ResolvedWhereExpression result)
        {
          
        }

        private static void Evaluate(NewExpression expression, ResolvedWhereExpression result)
        {
          
        }

        private static void Evaluate(BinaryExpression expression, ResolvedWhereExpression result)
        {
            // try not to use recursion, its slow
            // almost everything enters as a binary expression
            if (expression.NodeType == ExpressionType.AndAlso || expression.NodeType == ExpressionType.OrElse)
            {
                // same block
                Evaluate(expression.Left as dynamic, result);
                Evaluate(expression.Right as dynamic, result);
                return;
            }

            if (expression.NodeType == ExpressionType.Call)
            {
                return;
            }

            // new block
            blockId++;
        }

        private static void Evaluate(UnaryExpression expression, ResolvedWhereExpression result)
        {
         
        }

        public static ExpressionBlock<T> CreateExpression<T>(T expression, int blockId)
        {
            return new ExpressionBlock<T>
            {
                Expression = expression,
                BlockId = blockId
            };
        }
    }

    internal class ExpressionBlock<T>
    {
        public T Expression { get; set; }
        public int BlockId { get; set; }
    }
}
