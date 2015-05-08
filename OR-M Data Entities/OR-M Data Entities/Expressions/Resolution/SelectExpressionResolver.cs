using System;
using System.Linq.Expressions;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Containers;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class SelectExpressionResolver : ExpressionResolver
    {
        public SelectExpressionResolver(DbQuery query)
            : base(query)
        {
        }

        public void Resolve<TOuter, TInner, TResult>(Expression<Func<TOuter, TInner, TResult>> selector)
        {
            // What if we are reshaping a reshaped container?

            _resolveShape(selector.Body as dynamic);
        }

        public void Resolve<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
        {
            _resolveShape(selector.Body as dynamic);
        }

        private void _resolveShape(NewExpression expression)
        {
            var count = expression.Members.Count;

            for (var i = 0; i < count; i++)
            {
                var argument = expression.Arguments[i];
                var member = expression.Members[i];
                SelectResolution.AddColumn(new SelectNode
                {
                    MappedProperty = member,
                    ColumnName = GetColumnName(argument as MemberExpression),
                    TableName = GetTableName(argument as MemberExpression)
                });
            }
        }

        private void _resolveShape(MemberInitExpression expression)
        {

        }
    }
}
