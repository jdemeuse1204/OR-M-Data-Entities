/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Linq.Expressions;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Expressions
{
    public interface IExpressionQueryResolvable<TSource>
    {
        void ResolveWhere(Expression<Func<TSource, bool>> expression);

        void ResolveSelect<TResult>(Expression<Func<TSource, TResult>> selector);

        void ResolveForeignKeyJoins();

        void SelectAll();

        void MakeDistinct();

        void Take(int count);

        DataReader<TSource> ExecuteReader();

        void IncludeAll();

        void IncludeTo(string tableName);

        void Include(string tableName);

        void OrderByPrimaryKeys();

        void Disconnect();
    }
}
