using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Lite.Expressions
{
    public interface IExpressionQuery<TSource> : IEnumerable<TSource>
    {
        IExpressionQuery<TSource> Where(Expression<Func<TSource, bool>> expression);

        TSource First(Expression<Func<TSource, bool>> expression);
        TSource First();

        TSource FirstOrDefault(Expression<Func<TSource, bool>> expression);
        TSource FirstOrDefault();

        List<TSource> ToList(Expression<Func<TSource, bool>> expression);
        List<TSource> ToList();

        IExpressionQuery<TSource> Include(string pathOrTableName);
        IExpressionQuery<TSource> IncludeAll();

        IExpressionQuery<TSource> Select<TResult>(Expression<Func<TSource, TResult>> selector);
    }
}
