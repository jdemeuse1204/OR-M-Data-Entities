using System;

namespace OR_M_Data_Entities.Expressions.Query.Tables
{
    public class AliasTable : DbTable
    {
        public AliasTable(Guid expressionQueryId, Type type, string alias)
            : base(expressionQueryId, type)
        {
            Alias = alias;
        }

        public string Alias { get; set; }
    }
}
