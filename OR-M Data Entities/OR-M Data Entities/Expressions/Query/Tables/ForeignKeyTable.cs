using System;

namespace OR_M_Data_Entities.Expressions.Query.Tables
{
    public class ForeignKeyTable : AliasTable
    {
        public ForeignKeyTable(Guid expressionQueryId, Type type, string foreignKeyTableName, string alias = "")
            : base(expressionQueryId, type, alias)
        {
            ForeignKeyTableName = foreignKeyTableName;
        }

        /// <summary>
        /// From the property
        /// </summary>
        public string ForeignKeyTableName { get; set; }
    }
}
