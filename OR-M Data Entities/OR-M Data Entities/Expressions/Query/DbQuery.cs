using System;
using OR_M_Data_Entities.Expressions.Resolution.Containers;

namespace OR_M_Data_Entities.Expressions.Query
{
    public class DbQuery
    {
        public readonly WhereResolutionContainer WhereResolution;
        public readonly SelectResolutionContainer SelectResolution;
        public readonly JoinResolutionContainer JoinResolution;
        public readonly Type BaseType;

        public string Sql { get; private set; }

        public DbQuery(Type baseType = null, WhereResolutionContainer whereResolution = null, SelectResolutionContainer selectResolution = null, JoinResolutionContainer joinResolution = null)
        {
            WhereResolution = whereResolution ?? new WhereResolutionContainer();
            SelectResolution = selectResolution ?? new SelectResolutionContainer();
            JoinResolution = joinResolution ?? new JoinResolutionContainer();
            BaseType = baseType;
        }

        protected DbQuery(DbQuery query)
        {
            WhereResolution = query.WhereResolution;
            SelectResolution = query.SelectResolution;
            JoinResolution = query.JoinResolution;
            BaseType = query.BaseType;
        }

        public void Resolve()
        {
            Sql = "";
        }
    }
}
