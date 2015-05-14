using System;
using System.Linq;
using System.Linq.Expressions;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution.Base;

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

            // need to unselect all for reshape
            SelectList.UnSelectAll();

            // resolve the expressions shape
            _resolveShape(selector.Body as dynamic);
        }

        public void Resolve<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
        {
            SelectList.UnSelectAll();

            _resolveShape(selector.Body as dynamic);
        }


        private SelectInfo _find(MemberExpression expression)
        {
            return SelectList.Infos.First(
                    w =>
                        w.NewType == expression.Expression.Type &&
                        w.NewProperty.Name == expression.Member.Name);
        }

        private void _resolveShape(MemberExpression expression)
        {
            var info = _find(expression);

            // is it always the same ?
            info.NewType = expression.Expression.Type;
            info.NewProperty = expression.Member;
            info.IsSelected = true;

            SelectList.ReturnPropertyOnly = true;
        }

        private void _resolveShape(NewExpression expression)
        {
            var count = expression.Members.Count;

            for (var i = 0; i < count; i++)
            {
                var argument = expression.Arguments[i];
                var member = expression.Members[i];
                var memberExpression = argument as MemberExpression;
                var parameterExpression = argument as ParameterExpression;

                if (memberExpression != null)
                {
                    var info = _find(memberExpression);

                    info.NewType = member.ReflectedType;
                    info.NewProperty = member;
                    info.IsSelected = true;
                    continue;
                }

                if (parameterExpression != null)
                {
                    foreach (var info in SelectList.Infos.Where(w => w.NewType == parameterExpression.Type))
                    {
                        info.IsSelected = true;
                    }
                }
            }
        }

        private void _resolveShape(MemberInitExpression expression)
        {
            var newType = expression.Type;

            foreach (var binding in expression.Bindings)
            {
                switch (binding.BindingType)
                {
                    case MemberBindingType.Assignment:
                        var assignment = (MemberAssignment)binding;

                        var listInitExpression = assignment.Expression as ListInitExpression;
                        var memberExpression = assignment.Expression as MemberExpression;

                        if (memberExpression != null)
                        {
                            var info = _find(memberExpression);

                            info.NewType = newType;
                            info.NewProperty = assignment.Member;
                            info.IsSelected = true;
                            continue;
                        }

                        if (listInitExpression != null)
                        {

                        }

                        break;
                    case MemberBindingType.ListBinding:
                        break;
                    case MemberBindingType.MemberBinding:
                        break;
                }
            }
        }
    }
}
