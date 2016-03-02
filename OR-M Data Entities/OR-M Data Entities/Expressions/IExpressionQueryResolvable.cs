﻿/*
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

        void ResolveWhere(Expression<Func<TSource, bool>> expression);

        IExpressionQuery<TResult> ResolveSelect<TResult>(IExpressionQuery<TSource> source, Expression<Func<TSource, TResult>> selector);

        IOrderedExpressionQuery<TResult> ResolveSelect<TResult>(IOrderedExpressionQuery<TSource> source, Expression<Func<TSource, TResult>> selector);

        void ResolveFind<TResult>(object[] pks, IConfigurationOptions configuration);

        void ResolveOrderBy<TKey>(Expression<Func<TSource, TKey>> keySelector);

        void ResolveOrderByDescending<TKey>(Expression<Func<TSource, TKey>> keySelector);

        IExpressionQuery<TResult> ResolveJoin<TOuter, TInner, TKey, TResult>(
                IExpressionQuery<TOuter> outer,
                IExpressionQuery<TInner> inner,
                Expression<Func<TOuter, TKey>> outerKeySelector,
                Expression<Func<TInner, TKey>> innerKeySelector,
                Expression<Func<TOuter, TInner, TResult>> resultSelector,
                JoinType joinType);

        void ResolveMax();

        void ResolveAny();

        void ResolveCount();

        void ResolveMin();

        void ResolveForeignKeyJoins();

        void SelectAll();

        void MakeDistinct();

        void Take(int count);

        IDataTranslator<T> ExecuteReader<T>();

        void IncludeAll();

        void IncludeTo(string tableName);

        void Include(string tableName);

        void OrderByPrimaryKeys();

        void Disconnect();
    }
}
