/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System;
using System.Linq.Expressions;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Expressions
{
    public interface IExpressionQueryResolvable<TSource>
    {
        bool AreForeignKeysSelected();

        bool HasForeignKeys();

        void Where(Expression<Func<TSource, bool>> expression);

        IExpressionQuery<TResult> Select<TResult>(IExpressionQuery<TSource> source, Expression<Func<TSource, TResult>> selector);

        IOrderedExpressionQuery<TResult> Select<TResult>(IOrderedExpressionQuery<TSource> source, Expression<Func<TSource, TResult>> selector);

        void Find<TResult>(object[] pks, IConfigurationOptions configuration);

        void OrderBy<TKey>(Expression<Func<TSource, TKey>> keySelector);

        void OrderByDescending<TKey>(Expression<Func<TSource, TKey>> keySelector);

        IExpressionQuery<TResult> Join<TOuter, TInner, TKey, TResult>(
                IExpressionQuery<TOuter> outer,
                IExpressionQuery<TInner> inner,
                Expression<Func<TOuter, TKey>> outerKeySelector,
                Expression<Func<TInner, TKey>> innerKeySelector,
                Expression<Func<TOuter, TInner, TResult>> resultSelector,
                JoinType joinType);

        void Max();

        void Any();

        void Count();

        void Min();

        void ForeignKeyJoins();

        void SelectAll();

        void Distinct();

        void Take(int count);

        IDataTranslator<T> ExecuteReader<T>();

        void IncludeAll();

        void Include(string pathOrTableName);

        void OrderByPrimaryKeys();

        void Disconnect();
    }
}
