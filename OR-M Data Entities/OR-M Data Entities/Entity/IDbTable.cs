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
