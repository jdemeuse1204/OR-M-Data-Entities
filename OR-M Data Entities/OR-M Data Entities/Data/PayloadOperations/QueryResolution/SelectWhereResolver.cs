using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping.Base;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;
using OR_M_Data_Entities.Data.PayloadOperations.QueryResolution.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations.QueryResolution
{
    public class SelectWhereResolver : Resolver
    {
        public SelectWhereResolver(ObjectMap map)
            : base(map)
        {
        }

        public override BuildContainer Resolve()
        {
            var result = new BuildContainer();
            var where = ResolveWheres();
            var select = ResolveSelect();
            var from = ResolveFrom();
            var columns = ResolveColumns();

            result.Sql = string.Format("{0}{1} FROM {2}",
                select,
                columns.TrimEnd(','),
                string.Format("[{0}] ", from));

            for (var i = 0; i < where.ValidationStatements.Count; i++)
            {
                result.Sql += string.Format(i == 0 ? " WHERE {0} " : "AND {0} ", where.ValidationStatements[i]);
            }

            result.Parameters = where.Parameters;

            return result;
        }
    }
}
