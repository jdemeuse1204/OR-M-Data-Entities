using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Containers;
using OR_M_Data_Entities.Expressions.Resolution.Select.Info;

namespace OR_M_Data_Entities.Expressions.Resolution.Select
{
    public class SelectExpressionResolver : ExpressionResolver
    {
        public static void Resolve<TOuter, TInner, TResult>(Expression<Func<TOuter, TInner, TResult>> selector, SelectInfoResolutionContainer selectList)
        {
            // What if we are reshaping a reshaped container?

            // need to unselect all for reshape
            selectList.UnSelectAll();

            // resolve the expressions shape
            _resolveShape(selector.Body as dynamic, selectList);
        }

        public static void Resolve<TSource, TResult>(Expression<Func<TSource, TResult>> selector, SelectInfoResolutionContainer selectList)
        {
            selectList.UnSelectAll();

            _resolveShape(selector.Body as dynamic, selectList);
        }


        private static SelectInfo _find(MemberExpression expression, SelectInfoResolutionContainer selectList)
        {
            // what if we are selecting a foreign key, then we need to select all from that table
            return selectList.Infos.First(
                    w =>
                        w.NewType == expression.Expression.Type &&
                        w.NewProperty.Name == expression.Member.Name);
        }

        private static void _resolveShape(MemberExpression expression, SelectInfoResolutionContainer selectList)
        {
            var info = _find(expression, selectList);

            // is it always the same ?
            info.NewType = expression.Expression.Type;
            info.NewProperty = expression.Member;
            info.IsSelected = true;

            selectList.ReturnPropertyOnly = true;
        }

        private static bool _isForeignKey(MemberExpression expression)
        {
            var propertyInfo = expression.Member as PropertyInfo;

            if (propertyInfo == null) return false;

            var type = propertyInfo.PropertyType.IsList() ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType;

            return Types.Contains(type);
        }

        private static void _resolveShape(NewExpression expression, SelectInfoResolutionContainer selectList)
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
                    if (!_isForeignKey(memberExpression))
                    {
                        var info = _find(memberExpression, selectList);

                        info.NewType = member.ReflectedType;
                        info.NewProperty = member;
                        info.IsSelected = true;
                    }
                    else
                    {
                        _selectAll(member.ReflectedType.IsList()
                            ? member.ReflectedType.GetGenericArguments()[0]
                            : member.ReflectedType,
                            selectList);
                    }
                    continue;
                }

                if (parameterExpression != null)
                {
                    foreach (var info in selectList.Infos.Where(w => w.NewType == parameterExpression.Type))
                    {
                        info.IsSelected = true;
                    }
                }
            }
        }

        private static void _resolveShape(MemberInitExpression expression, SelectInfoResolutionContainer selectList)
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
                            var info = _find(memberExpression, selectList);

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

        private static void _selectAll(Type type, SelectInfoResolutionContainer selectList)
        {
            foreach (var column in selectList.Infos.Where(w => w.NewType == type))
            {
                column.IsSelected = true;
            }
        }
    }
}
