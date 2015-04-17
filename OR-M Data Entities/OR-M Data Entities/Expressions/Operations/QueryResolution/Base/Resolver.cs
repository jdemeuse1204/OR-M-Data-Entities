using System.Linq;
using OR_M_Data_Entities.Expressions.Operations.ObjectMapping.Base;
using OR_M_Data_Entities.Expressions.Operations.Payloads.Base;

namespace OR_M_Data_Entities.Expressions.Operations.QueryResolution.Base
{
	public abstract class Resolver
	{
		protected readonly ObjectMap Map;

		protected Resolver(ObjectMap map)
		{
			Map = map;
		}

		protected string ResolveJoins()
		{
			return Map.Tables.Aggregate(string.Empty, (current, table) => current + table.GetJoins());
		}

		protected string ResolveSelect()
		{
			return Map.Rows.HasValue ? string.Format("SELECT TOP {0} ", Map.Rows.Value) : "SELECT ";
		}

		protected string ResolveFrom()
		{
			return Map.FromTableName();
		}

		protected string ResolveColumns()
		{
			return Map.Tables.Aggregate(string.Empty, (current, table) => current + table.GetSelectColumns()).TrimEnd(',');
		}

		protected WhereContainer ResolveWheres()
		{
			var where = new WhereContainer();

			foreach (var objectTable in Map.Tables.Where(w => w.HasValidation()))
			{
				objectTable.GetWheres(where);
			}

			return where;
		}

		public abstract BuildContainer Resolve();
	}
}
