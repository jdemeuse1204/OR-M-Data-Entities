using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.StatementParts;
using OR_M_Data_Entities.Expressions.Support;

namespace OR_M_Data_Entities.Expressions.Types.Base
{
    public abstract class SqlExpressionResolvableBase : ExpressionResolver
    {
        public abstract void Resolve();

        protected readonly ExpressionQuery Query;

        protected SqlExpressionResolvableBase(ExpressionQuery query)
        {
            Query = query;
        }

        protected List<SqlWhere> ResolveWheresList()
        {
            var where = new List<SqlWhere>();

            foreach (var resolution in Query.WheresList.Select(item => GetWheres(item as dynamic)))
            {
                @where.AddRange(resolution);
            }

            return where;
        }

        protected List<SqlTableColumnPair> ResolveSelectsList()
        {
            var select = new List<SqlTableColumnPair>();

            foreach (var resolution in Query.SelectsList.Select(item => GetSelects(item as dynamic)))
            {
                @select.AddRange(resolution);
            }

            return select;
        }

        protected SqlJoinCollection ResolveInnerJoinsList()
        {
            var leftJoinCollection = new SqlJoinCollection();

            foreach (var leftJoin in Query.LeftJoinsList)
            {
                var join = leftJoin as SqlJoin;

                if (join != null)
                {
                    leftJoinCollection.Add(join);
                    continue;
                }

                GetJoins(leftJoin as dynamic, JoinType.Left, leftJoinCollection);
            }

            return leftJoinCollection;
        }

        protected SqlJoinCollection ResolveLeftJoinsList()
        {
            var innerJoinCollection = new SqlJoinCollection();

            foreach (var innerJoin in Query.InnerJoinsList)
            {
                var join = innerJoin as SqlJoin;

                if (join != null)
                {
                    innerJoinCollection.Add(join);
                    continue;
                }

                GetJoins(innerJoin as dynamic, JoinType.Inner, innerJoinCollection);
            }

            return innerJoinCollection;
        } 
    }
}
