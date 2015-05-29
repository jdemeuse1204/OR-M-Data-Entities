using System;
using System.Linq.Expressions;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Containers;

namespace OR_M_Data_Entities.Expressions.Resolution.Join
{
    public class JoinExpressionResolver : ExpressionResolver
    {
        public static void Resolve<TOuter, TInner, TKey, TResult>(
            ExpressionQuery<TOuter> outer,
            ExpressionQuery<TInner> inner,
            Expression<Func<TOuter, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector,
            ExpressionQuery<TResult> expressionQuery, 
            JoinType joinType,
            JoinResolutionContainer joinResolution)
        {
            //joinResolution.AddJoin(new JoinGroup
            //{
            //    ParentNode = _resolveInnerKeySelector(innerKeySelector),
            //    ChildNode = _resolveOuterKeySelector(outerKeySelector),
            //    JoinType = joinType
            //});
        }

        private static Type _resolveType<T, TKey>(Expression<Func<T, TKey>> expression)
        {
            if (!expression.Parameters[0].Type.IsAnonymousType()) return expression.Parameters[0].Type;

            var newExpression = null as NewExpression;
            var field = expression.Body as MemberExpression;

            if (field == null) throw new Exception("Could not resolve field in JoinExpressionResolver");

            if (newExpression != null)
            {
                for (var i = 0; i < newExpression.Arguments.Count; i++)
                {
                    var argument = newExpression.Arguments[i];
                    var member = newExpression.Members[i];

                    if (member.Name == field.Member.Name)
                    {
                        return ((MemberExpression) argument).Expression.Type;
                    }
                }
            }

            throw new Exception("Could not resolve type");
        }
    }
}
