using System;
using System.Linq.Expressions;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Containers;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class JoinExpressionResolver : ExpressionResolver
    {
        public JoinExpressionResolver(DbQuery query)
            : base(query)
        {
        }

        public void Resolve<TOuter, TInner, TKey, TResult>(
            ExpressionQuery<TOuter> outer,
            ExpressionQuery<TInner> inner,
            Expression<Func<TOuter, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector,
            ExpressionQuery<TResult> expressionQuery, JoinType joinType)
        {

            var joinId = JoinResolution.NextJoinId();

            var outerNode = _resolveOuterKeySelector(outerKeySelector, joinId, joinType);

            var innerNode = _resolveInnerKeySelector(innerKeySelector, joinId, joinType);

            JoinResolution.AddJoin(outerNode);
            JoinResolution.AddJoin(innerNode);
        }

        private JoinNode _resolveOuterKeySelector<TOuter, TKey>(Expression<Func<TOuter, TKey>> outerKeySelector, int joinId, JoinType joinType)
        {
            return new JoinNode
            {
                TableName = DatabaseSchemata.GetTableName(outerKeySelector.Parameters[0].Type),
                JoinId = joinId,
                ColumnName = GetColumnName(outerKeySelector.Body as MemberExpression),
                JoinType = joinType
            };
        }

        private JoinNode _resolveInnerKeySelector<TInner, TKey>(Expression<Func<TInner, TKey>> innerKeySelector, int joinId, JoinType joinType)
        {
            return new JoinNode
            {
                TableName = DatabaseSchemata.GetTableName(innerKeySelector.Parameters[0].Type),
                JoinId = joinId,
                ColumnName = GetColumnName(innerKeySelector.Body as MemberExpression),
                JoinType = joinType
            };
        }
    }
}
