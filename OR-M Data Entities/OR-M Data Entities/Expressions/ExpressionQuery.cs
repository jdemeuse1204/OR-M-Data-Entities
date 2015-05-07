using System.Collections;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Containers;

namespace OR_M_Data_Entities.Expressions
{
    public class ExpressionQuery<T> : IEnumerable, IExpressionQuery
    {
        public readonly WhereResolutionContainer WhereResolution;
        public readonly SelectResolutionContainer SelectResolution;
        public readonly JoinResolutionContainer JoinResolution;

        public ExpressionQuery()
        {
            WhereResolution = new WhereResolutionContainer();
            SelectResolution = new SelectResolutionContainer();
            JoinResolution = new JoinResolutionContainer();
        }

        public ExpressionQuery(WhereResolutionContainer resolution)
        {
            WhereResolution = resolution;
        }

        public IEnumerator GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public string GetSql()
        {
            throw new System.NotImplementedException();
        }
    }
}
