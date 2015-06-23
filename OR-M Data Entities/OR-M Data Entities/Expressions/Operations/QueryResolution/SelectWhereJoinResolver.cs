﻿using OR_M_Data_Entities.Expressions.Operations.ObjectMapping.Base;
using OR_M_Data_Entities.Expressions.Operations.Payloads.Base;
using OR_M_Data_Entities.Expressions.Operations.QueryResolution.Base;

namespace OR_M_Data_Entities.Expressions.Operations.QueryResolution
{
    public class SelectWhereJoinResolver : Resolver
    {
        public SelectWhereJoinResolver(ObjectMap map)
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
            var joins = ResolveJoins();

            result.Sql = string.Format("{0}{1} FROM {2}{3}",
                select,
                columns.TrimEnd(','),
                string.Format("[{0}] ", from),
                joins);

            for (var i = 0; i < where.ValidationStatements.Count; i++)
            {
                result.Sql += string.Format(i == 0 ? " WHERE {0} " : "AND {0} ", where.ValidationStatements[i]);
            }

            result.Parameters = where.Parameters;

            return result;
        }
    }
}