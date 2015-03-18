/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace OR_M_Data_Entities.Entity
{
	public interface IDbTable<T> 
	{
        string TableName { get; }

        bool HasChanges { get; }

		void Add(T entity);

		void Remove(T entity);

	    bool RemoveLocal(T entity);

		void Clear();

        T Find(params object[] pks);

        List<T> Where(Expression<Func<T, bool>> propertyLambda);

        T FirstOrDefault(Expression<Func<T, bool>> propertyLambda);

		List<T> All();
	}
}
