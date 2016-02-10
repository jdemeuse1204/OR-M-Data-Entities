using System;
using System.Linq.Expressions;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Expressions
{
    public interface IExpressionQueryResolvable<TSource>
    {
        void ResolveWhere(Expression<Func<TSource, bool>> expression);

        void ResolveSelect<TResult>(Expression<Func<TSource, TResult>> selector);

        void SelectAll();

        DataReader<TSource> ExecuteReader();

        void Disconnect();
    }
}
