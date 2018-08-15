using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Lite.Expressions
{
    internal class ExpressionBlock
    {
        public ExpressionBlock(Expression expression, int blockId)
        {
            Expression = expression;
            BlockId = blockId;
        }

        public Expression Expression { get; }
        public int BlockId { get; }
    }
}
