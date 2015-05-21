﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Select.Info;

namespace OR_M_Data_Entities.Expressions.Resolution.Select
{
    public class SelectExpressionResolver : ExpressionResolver
    {
        public SelectExpressionResolver(DbQueryBase query)
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
            // what if we are selecting a foreign key, then we need to select all from that table
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

        private bool _isForeignKey(MemberExpression expression)
        {
            var propertyInfo = expression.Member as PropertyInfo;

            if (propertyInfo == null) return false;

            var type = propertyInfo.PropertyType.IsList() ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType;

            return Types.Contains(type);
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
                    if (!_isForeignKey(memberExpression))
                    {
                        var info = _find(memberExpression);

                        info.NewType = member.ReflectedType;
                        info.NewProperty = member;
                        info.IsSelected = true;
                    }
                    else
                    {
                        _selectAll(member.ReflectedType.IsList()
                            ? member.ReflectedType.GetGenericArguments()[0]
                            : member.ReflectedType); 
                    }
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

        private void _selectAll(Type type)
        {
            foreach (var column in SelectList.Infos.Where(w => w.NewType == type))
            {
                column.IsSelected = true;
            }
        }
    }
}