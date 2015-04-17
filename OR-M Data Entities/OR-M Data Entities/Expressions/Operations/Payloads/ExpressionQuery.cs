using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Operations.ObjectMapping.Base;
using OR_M_Data_Entities.Expressions.Operations.Payloads.Base;
using OR_M_Data_Entities.Expressions.Operations.QueryResolution;
using OR_M_Data_Entities.Expressions.Operations.QueryResolution.Base;

namespace OR_M_Data_Entities.Expressions.Operations.Payloads
{
    public abstract class ExpressionQuery : Builder, IEnumerable
    {
		protected readonly DataFetching Context;

		protected ExpressionQuery(DataFetching context)
		{
			Context = context;
		}

        protected void Table<T>() where T : class
        {
            Map = new ObjectMap(typeof(T));
        }

		private bool _hasJoins()
		{
			return Map.Tables.Any(w => w.Columns.Any(x => x.HasJoins));
		}

		private bool _hasWheres()
		{
			return Map.Tables.Any(w => w.Columns.Any(x => x.IsPartOfValidation));
		}

		private T _createInstance<T>() where T : Resolver
		{
			return
				Activator.CreateInstance(typeof(T), new object[] { Map }) as T;
		}

		protected override BuildContainer Build()
		{
			var resolver = _getResolver();

			return resolver.Resolve();
		}

		private Resolver _getResolver()
		{
			var hasJoins = _hasJoins();
			var hasWheres = _hasWheres();

			if (hasJoins)
			{
				if (hasWheres)
				{
					return _createInstance<SelectWhereJoinResolver>();
				}

				return _createInstance<SelectJoinResolver>();
			}

			if (hasWheres)
			{
				return _createInstance<SelectWhereResolver>();
			}

			return _createInstance<SelectResolver>();
		}

		#region Data Retrieval

		public object First()
		{
			// inject the generic type here
			var method = typeof(ExpressionQuery).GetMethods().FirstOrDefault(w => w.Name == "First" && w.ReturnParameter != null && w.ReturnParameter.ParameterType.Name == "T");
			var genericMethod = method.MakeGenericMethod(new[] { Map.BaseType });

			return genericMethod.Invoke(this, null);
		}

		public T First<T>() 
		{
		    if (Map.DataReturnType == ObjectMapReturnType.Dynamic && typeof (T) != typeof (object))
		    {
		        throw new Exception(string.Format("Cannot convert dynamic selection to {0}", typeof (T).Name));
		    }

			var buildContainer = Build();

			using (var reader = Context.ExecuteQuery<T>(buildContainer.Sql, buildContainer.Parameters, Map))
			{
				return reader.First();
			}
		}

        public T FirstOrDefault<T>()
        {
            if (Map.DataReturnType == ObjectMapReturnType.Dynamic && typeof(T) != typeof(object))
            {
                throw new Exception(string.Format("Cannot convert dynamic selection to {0}", typeof(T).Name));
            }

            var buildContainer = Build();

            using (var reader = Context.ExecuteQuery<T>(buildContainer.Sql, buildContainer.Parameters, Map))
            {
                return reader.FirstOrDefault();
            }
        }

		public ICollection ToList()
		{
			// inject the generic type here
			var method = typeof(ExpressionQuery).GetMethods().FirstOrDefault(w => w.Name == "All" && w.ReturnType != typeof(ICollection));
			var genericMethod = method.MakeGenericMethod(new[] { Map.BaseType });
			var result = genericMethod.Invoke(this, null);

			return result as dynamic;
		}

        public List<T> ToList<T>()
		{
            if (Map.DataReturnType == ObjectMapReturnType.Dynamic && typeof(T) != typeof(object))
            {
                throw new Exception(string.Format("Cannot convert dynamic selection to {0}", typeof(T).Name));
            }

			var buildContainer = Build();

			using (var reader = Context.ExecuteQuery<T>(buildContainer.Sql, buildContainer.Parameters, Map))
			{
				return reader.Cast<T>().ToList();
			}
		}

		public IEnumerator GetEnumerator<T>()
		{
            if (Map.DataReturnType == ObjectMapReturnType.Dynamic && typeof(T) != typeof(object))
            {
                throw new Exception(string.Format("Cannot convert dynamic selection to {0}", typeof(T).Name));
            }

			var buildContainer = Build();

			var reader = Context.ExecuteQuery<T>(buildContainer.Sql, buildContainer.Parameters, Map);

			return reader.Cast<T>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			// inject the generic type here
			var method = typeof(ExpressionQuery).GetMethod("GetEnumerator");
			var genericMethod = method.MakeGenericMethod(new[] { Map.BaseType });

			return (IEnumerator)genericMethod.Invoke(this, null);
		}
		#endregion
    }
}
