using System;
using System.Data;
using OR_M_Data_Entities.Commands;

namespace OR_M_Data_Entities.Expressions.Resolver
{
    public sealed class ExpressionWhereResult
    {
        public string PropertyName { get; set; }
        public Type PropertyType { get; set; }
        public string TableName { get; set; }

        private object _compareValue;
        public object CompareValue
        {
            get { return _compareValue; }
            set { _compareValue = (value ?? DBNull.Value); }
        }

        public SqlDbType Transform { get; set; }

        public ComparisonType ComparisonType { get; set; }

        public bool ShouldCast { get; set; }
    }
}
